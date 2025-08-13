using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Dtos;
using Order.API.Dtos.Buyer;
using Order.API.Dtos.Pagination;
using Order.API.Dtos.Supplier;
using Order.API.Helpers;
using Order.Domain.Interfaces;
using Order.Domain.Models;
using Order.Domain.Services;
using Order.Infrastructure.Data;


namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerController : ControllerBase
    {
        private readonly IGenericRepository<Buyer> _buyerRepo;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Buyer> _passwordHasher;
        private readonly ILogger<BuyerController> _logger;
        private readonly IGenericRepository<BuyerOrder> _buyerOrderRepo;
        private readonly IGenericRepository<OrderItem> _orderItemRepo;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<SupplierProduct> _supplierProductRepo;
        private readonly IGenericRepository<Product> _productRepo;
      
        private readonly IGenericRepository<SupplierOrder> _supplierOrderRepo;
        private readonly IGenericRepository<MyOrder> _myOrderRepo;
        private readonly IGenericRepository<ReferralCode> _referralCodeRepo;
        private readonly IUnitOfWork _unitOfWork;
        

        public BuyerController(
            IGenericRepository<Buyer> buyerRepo,
            IConfiguration configuration,
            ITokenService tokenService,
            IMapper mapper,
            IPasswordHasher<Buyer> passwordHasher,
            ILogger<BuyerController> logger,
            IGenericRepository<BuyerOrder> buyerOrderRepo,
            IGenericRepository<OrderItem> orderItemRepo,
            IGenericRepository<Supplier> supplierRepo,
             IGenericRepository<SupplierProduct> supplierProductRepo,
             IGenericRepository<Product> productRepo,
         
             IGenericRepository<SupplierOrder> supplierOrderRepo,
             IGenericRepository<MyOrder> myOrderRepo,
             IGenericRepository<ReferralCode> referralCodeRepo,
             IUnitOfWork unitOfWork
            
            )
        {
            _buyerRepo = buyerRepo;
            _configuration = configuration;
            _tokenService = tokenService;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _buyerOrderRepo = buyerOrderRepo;
            _orderItemRepo = orderItemRepo;
            _supplierRepo = supplierRepo;
            _supplierProductRepo = supplierProductRepo;
            _productRepo = productRepo;
            _supplierOrderRepo = supplierOrderRepo;
            _myOrderRepo = myOrderRepo;
            _referralCodeRepo = referralCodeRepo;
            _unitOfWork = unitOfWork;
            
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterBuyer([FromForm] RegisterBuyerDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if phone number already exists
            var existingBuyer = await _buyerRepo.GetFirstOrDefaultAsync(b => b.PhoneNumber == registerDto.PhoneNumber);
            if (existingBuyer != null)
                return BadRequest("Phone number already exists.");

            string? insideImagePath = null;
            string? outsideImagePath = null;
            try
            {
                insideImagePath = DocumentSettings.UploadFile(registerDto.PropertyInsideImage, "BuyerPlace");
                outsideImagePath = DocumentSettings.UploadFile(registerDto.PropertyOutsideImage, "BuyerPlace");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading images: {ex.Message}");
            }

            var buyer = _mapper.Map<Buyer>(registerDto);
            buyer.Password = _passwordHasher.HashPassword(buyer, registerDto.Password);
            buyer.PropertyInsideImagePath = insideImagePath;
            buyer.PropertyOutsideImagePath = outsideImagePath;
            buyer.IsActive = false;

            // توليد كود دعوة واحد وتخزينه في جدول ReferralCodes
            var referralCode = new ReferralCode
            {
                InvitationCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // كود 8 أحرف
                BuyerId = 0 // سيتم تحديثه بعد حفظ الـ Buyer
            };

            // التحقق من كود الدعوة لو موجود
            if (!string.IsNullOrEmpty(registerDto.ReferralCode))
            {
                var referrerCode = await _referralCodeRepo.GetFirstOrDefaultAsync(rc => rc.InvitationCode == registerDto.ReferralCode);
                if (referrerCode != null)
                {
                    var referrer = await _buyerRepo.GetByIdAsync(referrerCode.BuyerId);
                    if (referrer != null)
                    {
                        // إضافة 100 جنيه لمحفظة المدعو
                        referrer.WalletBalance += 100m;
                         _buyerRepo.Update(referrer); // حفظ تحديث الرصيد
                                                           // مسح الكود بعد الاستخدام
                        _referralCodeRepo.Delete(referrerCode);
                        await _referralCodeRepo.SaveChangesAsync();
                    }
                }
            }

            try
            {
                await _buyerRepo.AddAsync(buyer);
                await _buyerRepo.SaveChangesAsync();

                // تحديث ReferralCode بـ BuyerId بعد حفظ الـ Buyer
                referralCode.BuyerId = buyer.Id;
                await _referralCodeRepo.AddAsync(referralCode);
                await _referralCodeRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Delete uploaded images if saving fails
                if (insideImagePath != null) DocumentSettings.DeleteFile(insideImagePath, "BuyerPlace");
                if (outsideImagePath != null) DocumentSettings.DeleteFile(outsideImagePath, "BuyerPlace");
                return StatusCode(500, $"Error saving buyer: {ex.Message}");
            }

            var buyerToReturn = _mapper.Map<BuyerToReturnDto>(buyer);

            return Ok(new { message = "Buyer Created Successfully", Data = buyerToReturn });
        }

        [HttpPost("login")]
        public async Task<ActionResult<BuyerToReturnDto>> LoginBuyer([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Starting login attempt for phone number {PhoneNumber}", loginDto.PhoneNumber);

                // التحقق من صحة النموذج
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for login attempt with phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return BadRequest(ModelState);
                }

                // البحث عن المشتري بناءً على رقم الهاتف
                var buyer = await _buyerRepo.GetFirstOrDefaultAsync(b => b.PhoneNumber == loginDto.PhoneNumber);
                if (buyer == null)
                {
                    _logger.LogWarning("No buyer found with phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return Unauthorized("Invalid phone number or password.");
                }

                // التحقق من حالة التفعيل
                if (!buyer.IsActive)
                {
                    _logger.LogWarning("Login attempt failed for phone number {PhoneNumber}: Account is not activated", loginDto.PhoneNumber);
                    return Unauthorized("Account is not activated. Please contact the administrator.");
                }

                // التحقق من كلمة المرور
                var verificationResult = _passwordHasher.VerifyHashedPassword(buyer, buyer.Password, loginDto.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Invalid password for phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return Unauthorized("Invalid phone number or password.");
                }

                // إنشاء التوكن
                var token = await _tokenService.CreateTokenAsync(buyer, loginDto.RememberMe);
                _logger.LogInformation("Token generated successfully for phone number {PhoneNumber}", loginDto.PhoneNumber);

                // تخزين التوكن في ملف تعريف الارتباط
                var expiration = loginDto.RememberMe
                    ? DateTime.Now.AddDays(double.Parse(_configuration["JWT:RememberMeDurationInDays"]))
                    : DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]));
                _tokenService.StoreTokenInCookie(token, expiration, HttpContext);

                // إعداد الكائن المُرجع
                var buyerToReturn = _mapper.Map<BuyerToReturnDto>(buyer);
                buyerToReturn.Token = token;

                _logger.LogInformation("Login successful for phone number {PhoneNumber}", loginDto.PhoneNumber);
                return Ok(buyerToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in buyer with phone number {PhoneNumber}", loginDto.PhoneNumber);
                return StatusCode(500, $"Error logging in buyer: {ex.Message}");
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetBuyerProfile()
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            var buyer = await _buyerRepo.GetByIdAsync(int.Parse(buyerId));
            if (buyer == null)
                return NotFound("Buyer not found.");

            // جلب كود الدعوة لو موجود
            var referralCode = await _referralCodeRepo.GetFirstOrDefaultAsync(rc => rc.BuyerId == int.Parse(buyerId));
            string referralCodeValue = referralCode?.InvitationCode ?? "لا يوجد";

            var profile = new
            {
                FullName = buyer.FullName,
                PhoneNumber = buyer.PhoneNumber,
                PropertyName = buyer.PropertyName,
                PropertyType = buyer.PropertyType,
                WalletBalance = buyer.WalletBalance,
                ReferralCode = referralCodeValue
            };

            return Ok(new { message = "Profile retrieved successfully", Data = profile });
        }

        [HttpGet("categories")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories()
        {
            try
            {
                var products = await _productRepo.GetAllAsync();
                var categories = products
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .Select(c => new CategoryDto { Name = c })
                    .ToList();

                return Ok(new { message = "Categories retrieved successfully", Data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving categories: {ex.Message}");
            }
        }

        [HttpGet("companies")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies([FromQuery] string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest(new { message = "Category is required." });

            try
            {
                var products = await _productRepo.GetAllAsync(p => p.Category == category);
                var companies = products
                    .Select(p => p.Company)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .Select(c => new CompanyDto { Name = c })
                    .ToList();

                if (!companies.Any())
                    return NotFound(new { message = $"No companies found for category '{category}'." });

                return Ok(new { message = "Companies retrieved successfully", Data = companies });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving companies: {ex.Message}");
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<ProductDto>>> SearchProducts(
            [FromQuery] string? productName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                // جلب المنتجات مع تصفية باسم المنتج (إذا وُجد)
                var productsQuery = await _productRepo.GetAllAsync();
                if (!string.IsNullOrEmpty(productName))
                {
                    productsQuery = productsQuery
                        .Where(p => p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // جلب أقل PriceNow لكل منتج من SupplierProduct
                var productIds = productsQuery.Select(p => p.Id).ToList();
                var supplierProducts = await _supplierProductRepo.GetAllAsync(sp => productIds.Contains(sp.ProductId));

                // تجميع أقل PriceNow لكل منتج
                var lowestPrices = supplierProducts
                    .GroupBy(sp => sp.ProductId)
                    .Select(g => new { ProductId = g.Key, LowestPriceNow = g.Min(sp => sp.PriceNow) })
                    .ToDictionary(x => x.ProductId, x => x.LowestPriceNow);

                // تحويل المنتجات إلى ProductDto مع إضافة LowestPriceNow
                var mappedProducts = productsQuery.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Company = p.Company,
                    ImageUrl = p.ImageUrl,
                    LowestPriceNow = lowestPrices.ContainsKey(p.Id) ? lowestPrices[p.Id] : 0
                }).ToList();

                var totalItems = mappedProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedProducts = mappedProducts
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<ProductDto>
                {
                    Items = pagedProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedProducts.Any())
                    return NotFound(new { message = "No products found for the specified criteria." });

                return Ok(new { message = "Products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving products: {ex.Message}");
            }
        }

        [HttpGet("searchByCategory")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<ProductDto>>> SearchProductsByCategory(
            [FromQuery] string category,
            [FromQuery] string? productName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(category))
                return BadRequest(new { message = "Category is required." });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                // جلب المنتجات داخل الفئة مع تصفية باسم المنتج (إذا وُجد)
                var productsQuery = await _productRepo.GetAllAsync();
                productsQuery = productsQuery.Where(p => p.Category == category).ToList();

                if (!string.IsNullOrEmpty(productName))
                {
                    productsQuery = productsQuery
                        .Where(p => p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // جلب أقل PriceNow لكل منتج من SupplierProduct
                var productIds = productsQuery.Select(p => p.Id).ToList();
                var supplierProducts = await _supplierProductRepo.GetAllAsync(sp => productIds.Contains(sp.ProductId));

                // تجميع أقل PriceNow لكل منتج
                var lowestPrices = supplierProducts
                    .GroupBy(sp => sp.ProductId)
                    .Select(g => new { ProductId = g.Key, LowestPriceNow = g.Min(sp => sp.PriceNow) })
                    .ToDictionary(x => x.ProductId, x => x.LowestPriceNow);

                // تحويل المنتجات إلى ProductDto مع إضافة LowestPriceNow
                var mappedProducts = productsQuery.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Company = p.Company,
                    ImageUrl = p.ImageUrl,
                    LowestPriceNow = lowestPrices.ContainsKey(p.Id) ? lowestPrices[p.Id] : 0
                }).ToList();

                var totalItems = mappedProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedProducts = mappedProducts
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<ProductDto>
                {
                    Items = pagedProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedProducts.Any())
                    return NotFound(new { message = $"No products found for category '{category}'." });

                return Ok(new { message = "Products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving products: {ex.Message}");
            }
        }

        [HttpGet("products")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<ProductDto>>> GetProductsByCompanyAndCategory(
            [FromQuery] string? company,
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                // جلب المنتجات بناءً على الشركة و/أو الفئة
                var productsQuery = await _productRepo.GetAllAsync();
                if (!string.IsNullOrEmpty(company) && !string.IsNullOrEmpty(category))
                {
                    productsQuery = productsQuery.Where(p => p.Company == company && p.Category == category).ToList();
                }
                else if (!string.IsNullOrEmpty(company))
                {
                    productsQuery = productsQuery.Where(p => p.Company == company).ToList();
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    productsQuery = productsQuery.Where(p => p.Category == category).ToList();
                }
                // إذا مفيش company ولا category، يرجّع كل المنتجات

                // جلب أقل PriceNow لكل منتج من SupplierProduct
                var productIds = productsQuery.Select(p => p.Id).ToList();
                var supplierProducts = await _supplierProductRepo.GetAllAsync(sp => productIds.Contains(sp.ProductId));

                // تجميع أقل PriceNow لكل منتج
                var lowestPrices = supplierProducts
                    .GroupBy(sp => sp.ProductId)
                    .Select(g => new { ProductId = g.Key, LowestPriceNow = g.Min(sp => sp.PriceNow) })
                    .ToDictionary(x => x.ProductId, x => x.LowestPriceNow);

                // تحويل المنتجات إلى ProductDto مع إضافة LowestPriceNow
                var mappedProducts = productsQuery.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Company = p.Company,
                    ImageUrl = p.ImageUrl,
                    LowestPriceNow = lowestPrices.ContainsKey(p.Id) ? lowestPrices[p.Id] : 0
                }).ToList();

                var totalItems = mappedProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedProducts = mappedProducts
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<ProductDto>
                {
                    Items = pagedProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedProducts.Any())
                    return NotFound(new { message = "No products found for the specified criteria." });

                return Ok(new { message = "Products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving products: {ex.Message}");
            }
        }

        [HttpGet("productWithSuppliers")]
        [Authorize]
        public async Task<ActionResult<ProductWithSuppliersDto>> GetProductWithSuppliers([FromQuery] int productId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (productId <= 0)
                return BadRequest(new { message = "Product ID must be greater than zero" });

            try
            {
                // جلب المنتج
                var product = await _productRepo.GetByIdAsync(productId);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {productId} not found." });

                // جلب SupplierProduct اللي مرتبط بالمنتج مع بيانات Supplier
                var supplierProducts = await _supplierProductRepo.GetAllAsync(
                    sp => sp.ProductId == productId,
                    sp => sp.Product,
                    sp => sp.Supplier
                );

                // تحويل المنتج إلى ProductDto مع LowestPriceNow
                var lowestPrice = supplierProducts.Any() ? supplierProducts.Min(sp => sp.PriceNow) : 0;
                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Company = product.Company,
                    ImageUrl = product.ImageUrl,
                    LowestPriceNow = lowestPrice
                };

                // تجميع Supplier مع SupplierProduct الخاص بالمنتج
                var supplierWithProducts = supplierProducts
                    .GroupBy(sp => sp.Supplier)
                    .Select(g => new SupplierWithProductDto
                    {
                        Supplier = _mapper.Map<SupplierDto>(g.Key),
                        SupplierProduct = _mapper.Map<SupplierProductDto>(g.First(sp => sp.ProductId == productId))
                    })
                    .ToList();

                // إنشاء الـ Response
                var response = new ProductWithSuppliersDto
                {
                    Product = productDto,
                    Suppliers = supplierWithProducts
                };

                return Ok(new { message = "Product and suppliers retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving product and suppliers: {ex.Message}");
            }
        }

        [HttpGet("supplier/{id}")]
        [Authorize]
        public async Task<ActionResult<SupplierDto>> GetSupplierById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id <= 0)
                return BadRequest(new { message = "Supplier ID must be greater than zero" });

            try
            {
                // جلب المورد بناءً على الـ Id
                var supplier = await _supplierRepo.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(new { message = $"Supplier with ID {id} not found." });

                // تحويل المورد إلى SupplierDto
                var supplierDto = _mapper.Map<SupplierDto>(supplier);

                return Ok(new { message = "Supplier retrieved successfully", Data = supplierDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving supplier: {ex.Message}");
            }
        }

        [HttpGet("supplier/{id}/products")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<SupplierProductDto>>> GetSupplierProductsBySupplierId(
            [FromRoute] int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id <= 0)
                return BadRequest(new { message = "Supplier ID must be greater than zero" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                var supplier = await _supplierRepo.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(new { message = $"Supplier with ID {id} not found." });

                var supplierProducts = await _supplierProductRepo.GetAllAsync(
                    sp => sp.SupplierId == id,
                    sp => sp.Product
                );

                var mappedSupplierProducts = _mapper.Map<List<SupplierProductDto>>(supplierProducts);

                var totalItems = mappedSupplierProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedSupplierProducts = mappedSupplierProducts
                    .OrderBy(sp => sp.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<SupplierProductDto>
                {
                    Items = pagedSupplierProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedSupplierProducts.Any())
                    return NotFound(new { message = $"No products found for supplier with ID {id}." });

                return Ok(new { message = "Supplier products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving supplier products: {ex.Message}");
            }
        }


        [HttpGet("supplier/{id}/products/offers")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<SupplierProductDto>>> GetSupplierOffers(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (id <= 0)
                return BadRequest(new { message = "Supplier ID must be greater than zero" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                var supplier = await _supplierRepo.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(new { message = $"Supplier with ID {id} not found." });

                var supplierProducts = await _supplierProductRepo.GetAllAsync(
                    sp => sp.SupplierId == id && sp.PriceBefore.HasValue && sp.PriceBefore.Value != 0,
                    sp => sp.Product
                );

                var mappedSupplierProducts = _mapper.Map<List<SupplierProductDto>>(supplierProducts);

                var totalItems = mappedSupplierProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedSupplierProducts = mappedSupplierProducts
                    .OrderBy(sp => sp.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<SupplierProductDto>
                {
                    Items = pagedSupplierProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedSupplierProducts.Any())
                    return NotFound(new { message = $"No offers found for supplier with ID {id}." });

                return Ok(new { message = "Supplier offers retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving supplier offers: {ex.Message}");
            }
        }


        [HttpGet("supplier/{id}/products/normal")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<SupplierProductDto>>> GetSupplierNormalProducts(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (id <= 0)
                return BadRequest(new { message = "Supplier ID must be greater than zero" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                var supplier = await _supplierRepo.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(new { message = $"Supplier with ID {id} not found." });

                var supplierProducts = await _supplierProductRepo.GetAllAsync(
                    sp => sp.SupplierId == id && (!sp.PriceBefore.HasValue || sp.PriceBefore.Value == 0),
                    sp => sp.Product
                );

                var mappedSupplierProducts = _mapper.Map<List<SupplierProductDto>>(supplierProducts);

                var totalItems = mappedSupplierProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedSupplierProducts = mappedSupplierProducts
                    .OrderBy(sp => sp.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<SupplierProductDto>
                {
                    Items = pagedSupplierProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedSupplierProducts.Any())
                    return NotFound(new { message = $"No normal products found for supplier with ID {id}." });

                return Ok(new { message = "Supplier normal products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving supplier products: {ex.Message}");
            }
        }


        [HttpPost("addToCart")]
        [Authorize]
        public async Task<ActionResult> AddToCart([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            var buyer = await _buyerRepo.GetByIdAsync(int.Parse(buyerId));
            if (buyer == null)
                return NotFound("Buyer not found.");

            var pendingOrder = await _buyerOrderRepo.GetFirstOrDefaultAsync(
                bo => bo.BuyerId == int.Parse(buyerId),
                query => query.Include(bo => bo.OrderItems)
            );

            if (pendingOrder == null)
            {
                pendingOrder = new BuyerOrder
                {
                    BuyerId = int.Parse(buyerId),
                    OrderDate = DateTime.Now,
                    TotalAmount = 0
                };
                await _buyerOrderRepo.AddAsync(pendingOrder);
                await _buyerOrderRepo.SaveChangesAsync();
            }

            decimal totalAmount = pendingOrder.TotalAmount;
            var orderItems = new List<OrderItem>();

            foreach (var item in orderDto.Items)
            {
                var supplierProduct = await _supplierProductRepo.GetByIdAsync(item.SupplierProductId);
                if (supplierProduct == null || !supplierProduct.IsAvailable)
                    return NotFound($"Supplier product with ID {item.SupplierProductId} not found or not available.");

                if (supplierProduct.Quantity < item.Quantity)
                    return BadRequest($"Insufficient quantity for product {item.SupplierProductId}.");

                // التحقق من MaxOrderLimit (معاملة ك-int، وتحقق لو أكبر من صفر)
                var existingQuantity = pendingOrder.OrderItems?.Where(oi => oi.SupplierProductId == item.SupplierProductId).Sum(oi => oi.Quantity) ?? 0;
                if (supplierProduct.MaxOrderLimit > 0 && (existingQuantity + item.Quantity) > supplierProduct.MaxOrderLimit)
                    return BadRequest($"Maximum order limit of {supplierProduct.MaxOrderLimit} exceeded for product {item.SupplierProductId}. Current: {existingQuantity}, Requested: {item.Quantity}");

                var existingItem = pendingOrder.OrderItems?.FirstOrDefault(oi => oi.SupplierProductId == item.SupplierProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                    totalAmount += supplierProduct.PriceNow * item.Quantity;
                    _orderItemRepo.Update(existingItem);
                }
                else
                {
                    var orderItem = new OrderItem
                    {
                        SupplierProductId = item.SupplierProductId,
                        Quantity = item.Quantity,
                        UnitPrice = supplierProduct.PriceNow,
                        BuyerOrderId = pendingOrder.Id
                    };
                    orderItems.Add(orderItem);
                    totalAmount += supplierProduct.PriceNow * item.Quantity;
                }
            }

            await _orderItemRepo.SaveChangesAsync();
            foreach (var orderItem in orderItems)
            {
                await _orderItemRepo.AddAsync(orderItem);
            }
            await _orderItemRepo.SaveChangesAsync();

            pendingOrder.TotalAmount = totalAmount;
            _buyerOrderRepo.Update(pendingOrder);
            await _buyerOrderRepo.SaveChangesAsync();

            return Ok(new { message = "Item(s) added to cart successfully.", totalAmount = pendingOrder.TotalAmount });
        }

        [HttpDelete("removeFromCart/{cartItemId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("Buyer ID not found in token.");

                var cartItem = await _orderItemRepo.GetFirstOrDefaultAsync(
                    oi => oi.Id == cartItemId && oi.BuyerOrder.BuyerId == int.Parse(buyerId),
                    query => query.Include(oi => oi.BuyerOrder).Include(oi => oi.SupplierProduct)
                );

                if (cartItem == null)
                    return NotFound("Cart item not found or not authorized.");

                _orderItemRepo.Delete(cartItem);

                var buyerOrder = cartItem.BuyerOrder;
                buyerOrder.TotalAmount -= cartItem.UnitPrice * cartItem.Quantity;
                if (buyerOrder.TotalAmount < 0) buyerOrder.TotalAmount = 0;
                _buyerOrderRepo.Update(buyerOrder);

                await _orderItemRepo.SaveChangesAsync();
                await _buyerOrderRepo.SaveChangesAsync();

                return Ok(new { message = "Item removed from cart successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for cartItemId: {cartItemId}", cartItemId);
                return StatusCode(500, $"Error removing item from cart: {ex.Message}");
            }
        }

        [HttpDelete("clearCart")]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("Buyer ID not found in token.");

                // نجيب الطلب اللي هو cart (أول واحد مثلا أو اللي لسه مش confirmed)
                var buyerOrder = await _buyerOrderRepo.GetFirstOrDefaultAsync(
                    bo => bo.BuyerId == int.Parse(buyerId),
                    query => query.Include(bo => bo.OrderItems)
                );

                if (buyerOrder == null)
                    return NotFound("Cart not found.");

                if (buyerOrder.OrderItems == null || !buyerOrder.OrderItems.Any())
                    return BadRequest("Cart is already empty.");

                // نحذف كل العناصر
                foreach (var item in buyerOrder.OrderItems.ToList())
                {
                    _orderItemRepo.Delete(item);
                }

                // نحدث الإجمالي
                buyerOrder.TotalAmount = 0;
                _buyerOrderRepo.Update(buyerOrder);

                await _orderItemRepo.SaveChangesAsync();
                await _buyerOrderRepo.SaveChangesAsync();

                return Ok(new { message = "Cart cleared successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart.");
                return StatusCode(500, $"Error clearing cart: {ex.Message}");
            }
        }


        [HttpPost("cart/update-quantity")]
        [Authorize]
        public async Task<IActionResult> UpdateCartQuantity(int orderItemId, string action)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            var orderItem = await _orderItemRepo.GetFirstOrDefaultAsync(
                oi => oi.Id == orderItemId && oi.BuyerOrder.BuyerId == int.Parse(buyerId),
                query => query
                    .Include(oi => oi.SupplierProduct)
            );

            if (orderItem == null)
                return NotFound("Product not found in cart.");

            int maxOrderLimit = orderItem.SupplierProduct.MaxOrderLimit;
            int availableStock = orderItem.SupplierProduct.Quantity;

            if (action == "plus")
            {
                if (orderItem.Quantity >= maxOrderLimit)
                    return BadRequest($"You have reached the maximum quantity ({maxOrderLimit}) for this product.");

                if (orderItem.Quantity >= availableStock)
                    return BadRequest($"Only {availableStock} items are available in stock.");

                orderItem.Quantity += 1;
            }
            else if (action == "minus")
            {
                if (orderItem.Quantity <= 1)
                    return BadRequest("Minimum quantity is 1.");

                orderItem.Quantity -= 1;
            }
            else
            {
                return BadRequest("Invalid action. Use 'plus' or 'minus'.");
            }

            var buyerOrder = await _buyerOrderRepo.GetFirstOrDefaultAsync(
                bo => bo.Id == orderItem.BuyerOrderId,
                query => query.Include(o => o.OrderItems)
            );

            if (buyerOrder != null)
            {
                buyerOrder.TotalAmount = buyerOrder.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                _buyerOrderRepo.Update(buyerOrder);
            }

            _orderItemRepo.Update(orderItem);

            await _buyerOrderRepo.SaveChangesAsync();
            await _orderItemRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Quantity updated successfully",
                quantity = orderItem.Quantity
            });
        }

        [HttpGet("cart")]
        [Authorize]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            var pendingOrder = await _buyerOrderRepo.GetFirstOrDefaultAsync(
            bo => bo.BuyerId == int.Parse(buyerId),
            query => query
                .Include(bo => bo.OrderItems)
                    .ThenInclude(oi => oi.SupplierProduct)
                        .ThenInclude(sp => sp.Product)
                .Include(bo => bo.OrderItems)
                    .ThenInclude(oi => oi.SupplierProduct)
                        .ThenInclude(sp => sp.Supplier)
            );


            if (pendingOrder == null || pendingOrder.OrderItems == null || !pendingOrder.OrderItems.Any())
                return NotFound("No pending order found.");

            var cartSuppliers = pendingOrder.OrderItems
                .Where(oi => oi.SupplierProduct != null && oi.SupplierProduct.Supplier != null)
                .GroupBy(oi => oi.SupplierProduct.Supplier)
                .Select(g => new CartSupplierDto
                {
                    Supplier = _mapper.Map<SupplierDto>(g.Key),
                    Items = g.Select(oi => new CartItemDto
                    {
                        Id = oi.Id,
                        SupplierProductId = oi.SupplierProductId,
                        ProductName = oi.SupplierProduct?.Product?.Name ?? "Unknown Product",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList(),
                    TotalPrice = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    TotalItems = g.Count(),
                    MinimumOrderPrice = g.Key.MinimumOrderPrice,
                    MinimumOrderPriceProgress = $"{g.Sum(oi => oi.Quantity * oi.UnitPrice)}/{g.Key.MinimumOrderPrice}",
                    MinimumOrderItems = g.Key.MinimumOrderItems,
                    MinimumOrderItemsProgress = $"{g.Count()}/{g.Key.MinimumOrderItems}",
                    IsValid = (g.Sum(oi => oi.Quantity * oi.UnitPrice) >= g.Key.MinimumOrderPrice) && (g.Count() >= g.Key.MinimumOrderItems)
                }).ToList();

            var cartDto = new CartDto
            {
                OrderId = pendingOrder.Id,
                Suppliers = cartSuppliers,
                GrandTotal = pendingOrder.TotalAmount
            };

            return Ok(new { message = "Cart retrieved successfully", Data = cartDto });
        }


        [HttpPost("confirmOrder")]
        [Authorize]
        public async Task<ActionResult> ConfirmOrder([FromBody] ConfirmOrderDto confirmDto)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            var pendingOrder = await _buyerOrderRepo.GetFirstOrDefaultAsync(
                bo => bo.BuyerId == int.Parse(buyerId),
                query => query
                    .Include(bo => bo.OrderItems)
                        .ThenInclude(oi => oi.SupplierProduct)
                            .ThenInclude(sp => sp.Product)
                    .Include(bo => bo.OrderItems)
                        .ThenInclude(oi => oi.SupplierProduct)
                            .ThenInclude(sp => sp.Supplier)
            );

            if (pendingOrder == null)
                return NotFound("No pending order found.");

            var totalOrderAmount = pendingOrder.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            var buyer = await _buyerRepo.GetByIdAsync(int.Parse(buyerId));
            if (buyer == null)
                return NotFound("Buyer not found.");

            decimal walletAmount = confirmDto.WalletAmount ?? 0;
            if (walletAmount > 0)
            {
                if (walletAmount > buyer.WalletBalance)
                    return BadRequest("Insufficient wallet balance.");
                if (walletAmount > totalOrderAmount)
                    return BadRequest("Wallet amount cannot exceed order total.");
            }

            var currentDateTime = DateTime.UtcNow;
            var currentDate = DateOnly.FromDateTime(currentDateTime);
            var minDeliveryDate = currentDate.AddDays(1);

            var supplierGroups = pendingOrder.OrderItems.GroupBy(oi => oi.SupplierProduct.Supplier);
            foreach (var group in supplierGroups)
            {
                var minDateForSupplier = currentDate.AddDays(group.Key.DeliveryDays);
                if (minDeliveryDate < minDateForSupplier)
                    minDeliveryDate = minDateForSupplier;
            }

            if (confirmDto.DeliveryDate < minDeliveryDate)
                return BadRequest($"Delivery date must be at least {minDeliveryDate:dd-MM-yyyy}.");

            var remainingAmount = totalOrderAmount - walletAmount;

            var orderDate = DateOnly.FromDateTime(currentDateTime);
            var myOrder = new MyOrder
            {
                Status = OrderStatus.Pending,
                DeliveryDate = confirmDto.DeliveryDate,
                OrderDate = orderDate,
                BuyerOrderId = pendingOrder.Id,
                BuyerId = int.Parse(buyerId),
                TotalAmount = totalOrderAmount
            };

            foreach (var orderItem in pendingOrder.OrderItems)
            {
                myOrder.Items.Add(new MyOrderItem
                {
                    SupplierProductId = orderItem.SupplierProductId,
                    ProductName = orderItem.SupplierProduct?.Product?.Name ?? "Unknown Product",
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    SupplierName = orderItem.SupplierProduct?.Supplier?.Name ?? "Unknown Supplier"
                });

                orderItem.SupplierProduct.Quantity -= orderItem.Quantity;
                _supplierProductRepo.Update(orderItem.SupplierProduct);
            }

            await _myOrderRepo.AddAsync(myOrder);
            await _myOrderRepo.SaveChangesAsync();

            foreach (var group in supplierGroups)
            {
                var totalPrice = group.Sum(oi => oi.Quantity * oi.UnitPrice);
                var totalItems = group.Count();

                if (totalPrice < group.Key.MinimumOrderPrice || totalItems < group.Key.MinimumOrderItems)
                    return BadRequest($"Order for supplier {group.Key.Name} does not meet minimum requirements.");

                var walletPaymentAmount = walletAmount > 0 ? (totalPrice * walletAmount / totalOrderAmount) : 0;
                var supplierOrder = new SupplierOrder
                {
                    SupplierId = group.Key.Id,
                    MyOrderId = myOrder.Id,
                    TotalAmount = totalPrice - walletPaymentAmount,
                    DeliveryDate = confirmDto.DeliveryDate,
                    PaymentMethod = walletAmount > 0 ? "Wallet" : "Cash",
                    Status = OrderStatus.Pending,
                    BuyerName = buyer.FullName,
                    BuyerPhone = buyer.PhoneNumber,
                    PropertyName = buyer.PropertyName,
                    PropertyAddress = buyer.PropertyAddress,
                    PropertyLocation = buyer.PropertyLocation,
                    Items = group.Select(oi => new SupplierOrderItem
                    {
                        SupplierProductId = oi.SupplierProductId,
                        ProductName = oi.SupplierProduct.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList(),
                    WalletPaymentAmount = walletPaymentAmount
                };

                await _supplierOrderRepo.AddAsync(supplierOrder);

                if (walletAmount > 0)
                {
                    group.Key.WalletBalance += walletPaymentAmount;
                    _supplierRepo.Update(group.Key);
                }
            }

            if (walletAmount > 0)
            {
                buyer.WalletBalance -= walletAmount;
                _buyerRepo.Update(buyer);
            }

            _buyerOrderRepo.Delete(pendingOrder);
            await _buyerOrderRepo.SaveChangesAsync();
            
            await _supplierOrderRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Order confirmed successfully",
                remainingAmount = remainingAmount
            });
        }

        [HttpGet("myOrders")]
        [Authorize]
        public async Task<ActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Buyer ID not found in token.");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var myOrdersQuery = (await _myOrderRepo.GetAllAsync(mo => mo.BuyerId == int.Parse(buyerId)))
                                .AsQueryable()
                                .Include(mo => mo.Items);

            var totalCount = await myOrdersQuery.CountAsync();
            if (totalCount == 0)
                return NotFound("No orders found for this buyer.");

            var orders = await myOrdersQuery
                .OrderByDescending(mo => mo.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(mo => new
                {
                    OrderId = mo.Id,
                    DeliveryDate = mo.DeliveryDate,
                    OrderDate = mo.OrderDate,
                    TotalAmount = mo.TotalAmount,
                    Status = mo.Status.ToString(),
                    Items = mo.Items.Select(i => new
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        SupplierName = i.SupplierName,
                        UnitPrice = i.UnitPrice,
                        SupplierProductId = i.SupplierProductId
                    }).ToList()
                })
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Orders = orders
            };

            return Ok(response);
        }

    }
}
