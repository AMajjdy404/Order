using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Dtos;
using Order.API.Dtos.Buyer;
using Order.API.Dtos.Dashboard;
using Order.API.Dtos.Pagination;
using Order.API.Dtos.Supplier;
using Order.API.Helpers;
using Order.Domain.Interfaces;
using Order.Domain.Models;
using Order.Domain.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<SupplierProduct> _supplierProductRepo;
        private readonly IPasswordHasher<Supplier> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<BuyerController> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<SupplierOrder> _supplierOrderRepo;
        private readonly IGenericRepository<MyOrder> _myOrderRepo;
        private readonly IGenericRepository<SupplierPenalty> _supplierPenaltyRepo;
        private readonly IGenericRepository<ReturnOrderItem> _returnOrderItemsRepo;
        private readonly IGenericRepository<SupplierOrderItem> _supplierOrderItemRepo;
        private readonly IGenericRepository<SupplierStatement> _supplierStatementRepo;
        private readonly IGenericRepository<MyOrderItem> _myOrderItemRepo;

        public SupplierController(
            IGenericRepository<Supplier> supplierRepo,
             IGenericRepository<SupplierProduct> supplierProductRepo,
            IPasswordHasher<Supplier> passwordHasher,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<BuyerController> logger,
            IMapper mapper,
            IGenericRepository<Product> productRepo,
            IGenericRepository<SupplierOrder> supplierOrderRepo,
            IGenericRepository<MyOrder> myOrderRepo,
            IGenericRepository<SupplierPenalty> supplierPenaltyRepo,
            IGenericRepository<ReturnOrderItem> returnOrderItemsRepo,
            IGenericRepository<SupplierOrderItem> supplierOrderItemRepo,
            IGenericRepository<SupplierStatement> supplierStatementRepo,
            IGenericRepository<MyOrderItem> myOrderItemRepo
             )
        {
            _supplierRepo = supplierRepo;
            _supplierProductRepo = supplierProductRepo;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = mapper;
            _productRepo = productRepo;
            _supplierOrderRepo = supplierOrderRepo;
            _myOrderRepo = myOrderRepo;
            _supplierPenaltyRepo = supplierPenaltyRepo;
            _returnOrderItemsRepo = returnOrderItemsRepo;
            _supplierOrderItemRepo = supplierOrderItemRepo;
            _supplierStatementRepo = supplierStatementRepo;
            _myOrderItemRepo = myOrderItemRepo;
        }

        [HttpPost("register")]
        public async Task<ActionResult<SupplierToReturnDto>> Register([FromForm] RegisterSupplierDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من إن SupplierType صالح
            try
            {
                if (!Enum.TryParse<SupplierType>(registerDto.SupplierType, true, out _))
                    return BadRequest("Invalid SupplierType. Must be one of: Wholesale, BulkWholesale, Manufacturer, RestaurantCafeSupplier");
            }
            catch
            {
                return BadRequest("Invalid SupplierType format.");
            }

            // التحقق من رقم الهاتف
            var existingSupplier = await _supplierRepo.GetFirstOrDefaultAsync(b => b.PhoneNumber == registerDto.PhoneNumber);
            if (existingSupplier != null)
                return BadRequest("Phone number already exists.");

            string? warehouseImageUrl = null;
            try
            {
                warehouseImageUrl = DocumentSettings.UploadFile(registerDto.WarehouseImage, "SupplierWarehouse");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading images: {ex.Message}");
            }

            var supplier = _mapper.Map<Supplier>(registerDto);
            supplier.Password = _passwordHasher.HashPassword(supplier, registerDto.Password);
            supplier.WarehouseImageUrl = warehouseImageUrl;

            try
            {
                await _supplierRepo.AddAsync(supplier);
                await _supplierRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (warehouseImageUrl != null) DocumentSettings.DeleteFile(warehouseImageUrl, "SupplierWarehouse");
                return StatusCode(500, $"Error saving supplier: {ex.Message}");
            }

            var supplierToReturn = _mapper.Map<SupplierToReturnDto>(supplier);
            return Ok(new { message = "Supplier Added Successfully", Data = supplierToReturn });
        }


        [HttpPost("login")]
        public async Task<ActionResult<SupplierToReturnDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Starting login attempt for phone number {PhoneNumber}", loginDto.PhoneNumber);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for login attempt with phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return BadRequest(ModelState);
                }

                var supplier = await _supplierRepo.GetFirstOrDefaultAsync(b => b.PhoneNumber == loginDto.PhoneNumber);
                if (supplier == null)
                {
                    _logger.LogWarning("No buyer found with phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return Unauthorized("Invalid phone number or password.");
                }

                var verificationResult = _passwordHasher.VerifyHashedPassword(supplier, supplier.Password, loginDto.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Invalid password for phone number {PhoneNumber}", loginDto.PhoneNumber);
                    return Unauthorized("Invalid phone number or password.");
                }

                var token = await _tokenService.CreateTokenAsync(supplier, loginDto.RememberMe);
                _logger.LogInformation("Token generated successfully for phone number {PhoneNumber}", loginDto.PhoneNumber);

                var expiration = loginDto.RememberMe
                    ? DateTime.Now.AddDays(double.Parse(_configuration["JWT:RememberMeDurationInDays"]))
                    : DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]));
                _tokenService.StoreTokenInCookie(token, expiration, HttpContext);

                var supplierToReturn = _mapper.Map<SupplierToReturnDto>(supplier);
                supplierToReturn.Token = token;

                _logger.LogInformation("Login successful for phone number {PhoneNumber}", loginDto.PhoneNumber);
                return Ok(supplierToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in buyer with phone number {PhoneNumber}", loginDto.PhoneNumber);
                return StatusCode(500, $"Error logging in buyer: {ex.Message}");
            }
        }

        [HttpGet("getProductsByName")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<ReturnedProductDto>>> SearchProductsByName(
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
                // جلب كل المنتجات
                var productsQuery = await _productRepo.GetAllAsync();

                // لو فيه اسم متبعت، فلتر بالاسم
                if (!string.IsNullOrWhiteSpace(productName))
                {
                    productsQuery = productsQuery
                        .Where(p => p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // تحويل المنتجات إلى ReturnedProductDto
                var mappedProducts = productsQuery.Select(p => new ReturnedProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Company = p.Company,
                    ImageUrl = p.ImageUrl
                }).ToList();

                var totalItems = mappedProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedProducts = mappedProducts
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<ReturnedProductDto>
                {
                    Items = pagedProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                if (!pagedProducts.Any())
                    return NotFound(new { message = "No products found." });

                return Ok(new { message = "Products retrieved successfully", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving products: {ex.Message}");
            }
        }


        [HttpPost("addSupplierProduct")]
        [Authorize]
        public async Task<ActionResult> AddSupplierProduct([FromBody] AddSupplierProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var statusString = productDto.Status?.Trim().ToLower();
            if (string.IsNullOrEmpty(statusString) || !new[] { "active", "notactive" }.Contains(statusString))
                return BadRequest("Status must be 'Active' or 'NotActive'.");

            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
                return Unauthorized("Supplier ID not found in token.");

            var supplier = await _supplierRepo.GetByIdAsync(int.Parse(supplierId));
            if (supplier == null)
                return NotFound("Supplier not found.");

            var product = await _productRepo.GetByIdAsync(productDto.ProductId);
            if (product == null)
                return NotFound("Product not found.");

            Status status = statusString == "active" ? Status.Active : Status.NotActive;

            var supplierProduct = new SupplierProduct
            {
                ProductId = productDto.ProductId,
                SupplierId = int.Parse(supplierId),
                PriceBefore = productDto.PriceBefore ?? 0,
                PriceNow = productDto.PriceNow,
                Quantity = productDto.Quantity,
                Status = status,
                IsAvailable = productDto.Quantity > 0,
                MaxOrderLimit = productDto.MaxOrderLimit
            };

            try
            {
                await _supplierProductRepo.AddAsync(supplierProduct);
                await _supplierProductRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving supplier product: {ex.Message}");
            }

            return Ok(new { message = "Supplier product added successfully" });
        }

        [HttpPut("updateSupplierProduct/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateSupplierProduct(int id, [FromBody] UpdateSupplierProductDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplierProduct = await _supplierProductRepo.GetByIdAsync(id);
            if (supplierProduct == null)
                return NotFound("Supplier product not found.");

            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId) || supplierProduct.SupplierId != int.Parse(supplierId))
                return Unauthorized("You are not authorized to update this supplier product.");

            var statusString = updateDto.Status?.Trim().ToLower();
            if (string.IsNullOrEmpty(statusString) || !new[] { "active", "notactive" }.Contains(statusString))
                return BadRequest("Status must be 'Active' or 'NotActive'.");

            Status status = statusString == "active" ? Status.Active : Status.NotActive;

            supplierProduct.PriceBefore = updateDto.PriceBefore ?? supplierProduct.PriceBefore;
            supplierProduct.PriceNow = updateDto.PriceNow ?? supplierProduct.PriceNow;
            supplierProduct.Quantity = updateDto.Quantity;
            supplierProduct.Status = status;
            supplierProduct.IsAvailable = updateDto.Quantity > 0;
            supplierProduct.MaxOrderLimit = updateDto.MaxOrderLimit ?? supplierProduct.MaxOrderLimit;

            try
            {
                _supplierProductRepo.Update(supplierProduct);
                await _supplierProductRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating supplier product: {ex.Message}");
            }

            return Ok(new { message = "Supplier product updated successfully" });
        }


        [HttpGet("getPendingSupplierOrders")]
        [Authorize]
        public async Task<ActionResult> GetPendingSupplierOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // جلب الصفحات من الريبو مع include للعناصر فقط (تجنّب include الذي يخلق روابط عكسية كثيرة)
            var paged = await _supplierOrderRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: o => o.SupplierId == supplierId && o.Status == OrderStatus.Pending,
                orderBy: o => o.DeliveryDate,
                descending: true,
                includes: o => o.Items
            );

            // map/projection لإزالة الروابط العكسية وتجنّب cycles
            var dtoList = paged.Items.Select(so => new SupplierOrderDto
            {
                Id = so.Id,
                BuyerName = so.BuyerName,
                BuyerPhone = so.BuyerPhone,
                PropertyName = so.PropertyName,
                PropertyAddress = so.PropertyAddress,
                PropertyLocation = so.PropertyLocation,
                TotalAmount = so.TotalAmount,
                DeliveryDate = so.DeliveryDate.ToString("dd-MM-yyyy"),
                Status = so.Status.ToString(),
                Items = so.Items?.Select(i => new SupplierOrderItemDto
                {
                    Id = i.Id,
                    SupplierProductId = i.SupplierProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList() ?? new List<SupplierOrderItemDto>()
            }).ToList();

            var response = new PagedResponseDto<SupplierOrderDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            };

            return Ok(response);
        }


        [HttpPost("confirmOrCancelSupplierOrder/{id}")]
        [Authorize]
        public async Task<ActionResult> ConfirmOrCancelSupplierOrder(int id, [FromQuery] bool isConfirmed)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
                return Unauthorized("Supplier ID not found in token.");

            var supplierOrder = await _supplierOrderRepo.GetFirstOrDefaultAsync(
                so => so.Id == id && so.SupplierId == int.Parse(supplierId),
                query => query.Include(so => so.Supplier)
            );

            if (supplierOrder == null)
                return NotFound("Supplier order not found or not authorized.");

            if (supplierOrder.Status != OrderStatus.Pending)
                return BadRequest("Order must be in Pending status before marking as Confirmed.");

            var myOrder = await _myOrderRepo.GetFirstOrDefaultAsync(
                mo => mo.Id == supplierOrder.MyOrderId,
                query => query.Include(mo => mo.Items)
            );

            if (myOrder == null)
                return NotFound("My order not found for this supplier order.");

            var supplier = supplierOrder.Supplier;

            if (isConfirmed)
            {
                supplierOrder.Status = OrderStatus.Confirmed;
                myOrder.Status = OrderStatus.Confirmed;

                var profitPercentage = (decimal) supplier.ProfitPercentage / 100m;
                var commission = supplierOrder.TotalAmount * profitPercentage;

                supplier.WalletBalance -= commission;
                _supplierRepo.Update(supplier);

                var statement = new SupplierStatement
                {
                    SupplierId = supplier.Id,
                    SupplierOrderId = supplierOrder.Id,
                    Amount = commission,
                    Title = "رسوم الطلبية",
                    BuyerName = supplierOrder.BuyerName,
                    PropertyName = supplierOrder.PropertyName,
                    DeliveryDate = supplierOrder.DeliveryDate,
                    CreatedAt = DateTime.UtcNow
                };
                await _supplierStatementRepo.AddAsync(statement);

                await _supplierRepo.SaveChangesAsync();
                await _supplierStatementRepo.SaveChangesAsync();
            }
            else
            {
                supplierOrder.Status = OrderStatus.Canceled;
                myOrder.Status = OrderStatus.Canceled;

                decimal penalty = supplierOrder.TotalAmount * 0.005m;

                supplier.WalletBalance -= penalty;
                _supplierRepo.Update(supplier);

                var penaltyRecord = new SupplierPenalty
                {
                    SupplierId = supplier.Id,
                    SupplierOrderId = supplierOrder.Id,
                    PenaltyAmount = penalty,
                    Reason = "الغاء الطلبية",
                    BuyerName = supplierOrder.BuyerName,
                    DeliveryDate = supplierOrder.DeliveryDate,
                    CreatedAt = DateTime.UtcNow
                };

                await _supplierPenaltyRepo.AddAsync(penaltyRecord);

                await _supplierRepo.SaveChangesAsync();
                await _supplierPenaltyRepo.SaveChangesAsync();
            }

            _supplierOrderRepo.Update(supplierOrder);
            _myOrderRepo.Update(myOrder);
            await _supplierOrderRepo.SaveChangesAsync();
            await _myOrderRepo.SaveChangesAsync();

            return Ok(new
            {
                message = $"Order {(isConfirmed ? "confirm" : "cancel")}ed successfully",
                status = supplierOrder.Status.ToString()
            });
        }


        [HttpPut("supplier-orders/{orderId}/update-quantities")]
        [Authorize]
        public async Task<IActionResult> UpdateSupplierAndMyOrderQuantities(int orderId,[FromBody] List<UpdateOrderItemQuantityDto> updatedItems)
        {
            // 1️⃣ جلب الطلب المورد
            var supplierOrder = await _supplierOrderRepo.GetFirstOrDefaultAsync(
                o => o.Id == orderId && o.Status == OrderStatus.Pending,
                q => q.Include(o => o.Items)
            );

            if (supplierOrder == null)
                return NotFound("Supplier order not found or not pending.");

            // 2️⃣ تعديل الكميات + تعديل مخزون SupplierProduct
            foreach (var item in updatedItems)
            {
                var orderItem = supplierOrder.Items.FirstOrDefault(i => i.Id == item.ItemId);
                if (orderItem != null)
                {
                    var supplierProduct = await _supplierProductRepo.GetFirstOrDefaultAsync(
                        p => p.Id == orderItem.SupplierProductId
                    );

                    if (supplierProduct == null)
                        return BadRequest($"Supplier product not found for item {orderItem.Id}");

                    if (item.NewQuantity < orderItem.Quantity)
                    {
                        // تقليل الكمية → زيادة المخزون
                        var difference = orderItem.Quantity - item.NewQuantity;
                        supplierProduct.Quantity += difference;
                    }
                    else if (item.NewQuantity > orderItem.Quantity)
                    {
                        // زيادة الكمية → خصم من المخزون
                        var difference = item.NewQuantity - orderItem.Quantity;
                        if (supplierProduct.Quantity < difference)
                            return BadRequest($"Not enough stock for product {supplierProduct.Id}");

                        supplierProduct.Quantity -= difference;
                    }

                    // تعديل الكمية في الطلب
                    orderItem.Quantity = item.NewQuantity;

                    // تحديث المنتج
                    _supplierProductRepo.Update(supplierProduct);
                }
            }

            // تحديث المبلغ الإجمالي للطلب المورد
            supplierOrder.TotalAmount = supplierOrder.Items.Sum(i => i.Quantity * i.UnitPrice);
            _supplierOrderRepo.Update(supplierOrder);

            // 3️⃣ تحديث الطلب الرئيسي MyOrder
            var myOrder = await _myOrderRepo.GetFirstOrDefaultAsync(
                o => o.Id == supplierOrder.MyOrderId && o.Status == OrderStatus.Pending,
                q => q.Include(o => o.Items)
            );

            if (myOrder != null)
            {
                foreach (var supplierItem in supplierOrder.Items)
                {
                    var myOrderItem = myOrder.Items
                        .FirstOrDefault(i => i.SupplierProductId == supplierItem.SupplierProductId);

                    if (myOrderItem != null)
                        myOrderItem.Quantity = supplierItem.Quantity;
                }

                myOrder.TotalAmount = myOrder.Items.Sum(i => i.Quantity * i.UnitPrice);
                _myOrderRepo.Update(myOrder);
            }

            // 4️⃣ حفظ التغييرات
            await _supplierOrderRepo.SaveChangesAsync();

            return Ok("Quantities, total amounts, and stock updated successfully.");
        }

        [HttpPost("markAsShipped/{id}")]
        [Authorize]
        public async Task<ActionResult> MarkOrderAsShipped(int id)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
                return Unauthorized("Supplier ID not found in token.");

            var supplierOrder = await _supplierOrderRepo.GetFirstOrDefaultAsync(
                so => so.Id == id && so.SupplierId == int.Parse(supplierId)
            );

            if (supplierOrder == null)
                return NotFound("Order not found or not authorized.");

            if (supplierOrder.Status != OrderStatus.Confirmed)
                return BadRequest("Order must be in Confirmed status before marking as Shipped.");

            supplierOrder.Status = OrderStatus.Shipped;

            var myOrder = await _myOrderRepo.GetFirstOrDefaultAsync(mo => mo.Id == supplierOrder.MyOrderId);
            if (myOrder != null)
            {
                myOrder.Status = OrderStatus.Shipped;
                _myOrderRepo.Update(myOrder);
            }

            _supplierOrderRepo.Update(supplierOrder);
            await _supplierOrderRepo.SaveChangesAsync();
            await _myOrderRepo.SaveChangesAsync();

            return Ok(new { message = "Order marked as Shipped", status = supplierOrder.Status.ToString() });
        }

        [HttpPost("markAsDelivered/{id}")]
        [Authorize]
        public async Task<ActionResult> MarkOrderAsDelivered(int id)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
                return Unauthorized("Supplier ID not found in token.");

            var supplierOrder = await _supplierOrderRepo.GetFirstOrDefaultAsync(
                so => so.Id == id && so.SupplierId == int.Parse(supplierId)
            );

            if (supplierOrder == null)
                return NotFound("Order not found or not authorized.");

            if (supplierOrder.Status != OrderStatus.Shipped)
                return BadRequest("Order must be in Shipped status before marking as Delivered.");

            supplierOrder.Status = OrderStatus.Delivered;

            var myOrder = await _myOrderRepo.GetFirstOrDefaultAsync(mo => mo.Id == supplierOrder.MyOrderId);
            if (myOrder != null)
            {
                myOrder.Status = OrderStatus.Delivered;
                _myOrderRepo.Update(myOrder);
            }

            _supplierOrderRepo.Update(supplierOrder);
            await _supplierOrderRepo.SaveChangesAsync();
            await _myOrderRepo.SaveChangesAsync();

            return Ok(new { message = "Order marked as Delivered", status = supplierOrder.Status.ToString() });
        }

        [HttpGet("getConfirmedOrders")]
        [Authorize]
        public async Task<ActionResult> GetConfirmedSupplierOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // جلب الصفحات من الريبو مع include للعناصر فقط (تجنّب include الذي يخلق روابط عكسية كثيرة)
            var paged = await _supplierOrderRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: o => o.SupplierId == supplierId && o.Status == OrderStatus.Confirmed,
                orderBy: o => o.DeliveryDate,
                descending: true,
                includes: o => o.Items
            );

            // map/projection لإزالة الروابط العكسية وتجنّب cycles
            var dtoList = paged.Items.Select(so => new SupplierOrderDto
            {
                Id = so.Id,
                BuyerName = so.BuyerName,
                BuyerPhone = so.BuyerPhone,
                PropertyName = so.PropertyName,
                PropertyAddress = so.PropertyAddress,
                PropertyLocation = so.PropertyLocation,
                TotalAmount = so.TotalAmount,
                DeliveryDate = so.DeliveryDate.ToString("dd-MM-yyyy"),
                Status = so.Status.ToString(),
                Items = so.Items?.Select(i => new SupplierOrderItemDto
                {
                    Id = i.Id,
                    SupplierProductId = i.SupplierProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList() ?? new List<SupplierOrderItemDto>()
            }).ToList();

            var response = new PagedResponseDto<SupplierOrderDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            };

            return Ok(response);
        }

        [HttpGet("getShippedOrders")]
        [Authorize]
        public async Task<ActionResult> GetShippedSupplierOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // جلب الصفحات من الريبو مع include للعناصر فقط (تجنّب include الذي يخلق روابط عكسية كثيرة)
            var paged = await _supplierOrderRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: o => o.SupplierId == supplierId && o.Status == OrderStatus.Shipped,
                orderBy: o => o.DeliveryDate,
                descending: true,
                includes: o => o.Items
            );

            // map/projection لإزالة الروابط العكسية وتجنّب cycles
            var dtoList = paged.Items.Select(so => new SupplierOrderDto
            {
                Id = so.Id,
                BuyerName = so.BuyerName,
                BuyerPhone = so.BuyerPhone,
                PropertyName = so.PropertyName,
                PropertyAddress = so.PropertyAddress,
                PropertyLocation = so.PropertyLocation,
                TotalAmount = so.TotalAmount,
                DeliveryDate = so.DeliveryDate.ToString("dd-MM-yyyy"),
                Status = so.Status.ToString(),
                Items = so.Items?.Select(i => new SupplierOrderItemDto
                {
                    Id = i.Id,
                    SupplierProductId = i.SupplierProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList() ?? new List<SupplierOrderItemDto>()
            }).ToList();

            var response = new PagedResponseDto<SupplierOrderDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            };

            return Ok(response);
        }

        [HttpGet("getDeliveredOrders")]
        [Authorize]
        public async Task<IActionResult> GetDeliveredOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // جلب الصفحات من الريبو مع include للعناصر فقط (تجنّب include الذي يخلق روابط عكسية كثيرة)
            var paged = await _supplierOrderRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: o => o.SupplierId == supplierId && o.Status == OrderStatus.Delivered,
                orderBy: o => o.DeliveryDate,
                descending: true,
                includes: o => o.Items
            );

            // map/projection لإزالة الروابط العكسية وتجنّب cycles
            var dtoList = paged.Items.Select(so => new SupplierOrderDto
            {
                Id = so.Id,
                BuyerName = so.BuyerName,
                BuyerPhone = so.BuyerPhone,
                PropertyName = so.PropertyName,
                PropertyAddress = so.PropertyAddress,
                PropertyLocation = so.PropertyLocation,
                TotalAmount = so.TotalAmount,
                DeliveryDate = so.DeliveryDate.ToString("dd-MM-yyyy"),
                Status = so.Status.ToString(),
                Items = so.Items?.Select(i => new SupplierOrderItemDto
                {
                    Id = i.Id,
                    SupplierProductId = i.SupplierProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList() ?? new List<SupplierOrderItemDto>()
            }).ToList();

            var response = new PagedResponseDto<SupplierOrderDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            };

            return Ok(response);
        }


        [HttpPost("returnOrder")]
        [Authorize]
        public async Task<IActionResult> ReturnOrder([FromBody] List<ReturnOrderItemDto> items)
        {
            if (items == null || !items.Any())
                return BadRequest("No items to return.");

            foreach (var item in items)
            {
                // جلب عنصر الطلبية من المورد
                var supplierOrderItem = await _supplierOrderItemRepo.GetByIdAsync(item.SupplierOrderItemId);
                if (supplierOrderItem == null)
                    return NotFound($"Order item {item.SupplierOrderItemId} not found.");

                // جلب الطلبية نفسها
                var supplierOrder = await _supplierOrderRepo.GetByIdAsync(supplierOrderItem.SupplierOrderId);
                if (supplierOrder == null)
                    return NotFound($"Supplier order {supplierOrderItem.SupplierOrderId} not found.");

                if (supplierOrder.Status != OrderStatus.Delivered)
                    return BadRequest($"Order {supplierOrder.Id} is not delivered yet. Cannot process return.");

                // إضافة المرتجع
                var returnItem = new ReturnOrderItem
                {
                    SupplierOrderItemId = item.SupplierOrderItemId,
                    ReturnedQuantity = item.Quantity,
                    ReturnDate = DateTime.UtcNow
                };

                await _returnOrderItemsRepo.AddAsync(returnItem);
                await _returnOrderItemsRepo.SaveChangesAsync();

                // تحديث المخزون
                var supplierProduct = await _supplierProductRepo.GetByIdAsync(supplierOrderItem.SupplierProductId);
                if (supplierProduct == null)
                    return NotFound($"Supplier product {supplierOrderItem.SupplierProductId} not found.");

                supplierProduct.Quantity += item.Quantity;

                if (supplierProduct.Quantity > 0)
                {
                    supplierProduct.Status = Status.Active;
                    supplierProduct.IsAvailable = true;
                }
                _supplierProductRepo.Update(supplierProduct);

                // تعديل الكمية في الطلبية نفسها
                supplierOrderItem.Quantity -= item.Quantity;
                if (supplierOrderItem.Quantity < 0) supplierOrderItem.Quantity = 0;
                _supplierOrderItemRepo.Update(supplierOrderItem);

                // تعديل الكمية في MyOrderItem لو الطلبية مرتبطة
                var myOrderItem = await _myOrderItemRepo.GetFirstOrDefaultAsync(m => m.MyOrderId == supplierOrder.MyOrderId && m.SupplierProductId == supplierOrderItem.SupplierProductId);
                if (myOrderItem != null)
                {
                    myOrderItem.Quantity -= item.Quantity;
                    if (myOrderItem.Quantity < 0) myOrderItem.Quantity = 0;
                    _myOrderItemRepo.Update(myOrderItem);
                }

                // تعديل السعر الكلي في SupplierOrder
                supplierOrder.TotalAmount -= supplierOrderItem.UnitPrice * item.Quantity;
                if (supplierOrder.TotalAmount < 0) supplierOrder.TotalAmount = 0;
                _supplierOrderRepo.Update(supplierOrder);

                // تعديل السعر الكلي في MyOrder لو الطلبية مرتبطة
                if (supplierOrder.MyOrderId != 0)
                {
                    var myOrder = await _myOrderRepo.GetByIdAsync(supplierOrder.MyOrderId);
                    if (myOrder != null)
                    {
                        myOrder.TotalAmount -= supplierOrderItem.UnitPrice * item.Quantity;
                        if (myOrder.TotalAmount < 0) myOrder.TotalAmount = 0;
                        _myOrderRepo.Update(myOrder);
                    }
                }

                await _supplierProductRepo.SaveChangesAsync();
                await _supplierOrderItemRepo.SaveChangesAsync();
                await _myOrderItemRepo.SaveChangesAsync();
                await _supplierOrderRepo.SaveChangesAsync();
                await _myOrderRepo.SaveChangesAsync();
            }

            return Ok("Return processed successfully.");
        }


        #region Statistics

        [HttpGet("getSupplierStatements")]
        [Authorize]
        public async Task<IActionResult> GetSupplierStatements([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var paged = await _supplierStatementRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: s => s.SupplierId == supplierId,
                orderBy: s => s.CreatedAt,
                descending: true
            );

            var dtoList = paged.Items.Select(s => new SupplierStatementDto
            {
                Id = s.Id,
                SupplierOrderId = s.SupplierOrderId ?? 0,
                Amount = s.Amount,
                Title = s.Title,
                BuyerName = s.BuyerName,
                PropertyName = s.PropertyName,
                DeliveryDate = s.DeliveryDate?.ToString("dd-MM-yyyy"),
                CreatedAt = s.CreatedAt.ToString("dd-MM-yyyy HH:mm")
            }).ToList();

            return Ok(new PagedResponseDto<SupplierStatementDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            });
        }

        [HttpGet("getReturnOrders")]
        [Authorize]
        public async Task<IActionResult> GetReturnOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var paged = await _returnOrderItemsRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: r => r.SupplierOrderItem.SupplierOrder.SupplierId == supplierId,
                orderBy: r => r.ReturnDate,
                descending: true,
                includes: r => r.SupplierOrderItem
            );

            var dtoList = paged.Items.Select(r => new ReturnOrderDto
            {
                Id = r.Id,
                SupplierOrderItemId = r.SupplierOrderItemId,
                ReturnedQuantity = r.ReturnedQuantity,
                ReturnDate = r.ReturnDate.ToString("dd-MM-yyyy"),
                ProductName = r.SupplierOrderItem.ProductName
            }).ToList();

            return Ok(new PagedResponseDto<ReturnOrderDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            });
        }

        [HttpGet("getSupplierPenalties")]
        [Authorize]
        public async Task<IActionResult> GetSupplierPenalties([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var supplierIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierIdStr))
                return Unauthorized("Supplier ID not found in token.");

            if (!int.TryParse(supplierIdStr, out var supplierId))
                return BadRequest("Invalid supplier id.");

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var paged = await _supplierPenaltyRepo.GetPagedAsync(
                page,
                pageSize,
                predicate: p => p.SupplierId == supplierId,
                orderBy: p => p.CreatedAt,
                descending: true
            );

            var dtoList = paged.Items.Select(p => new SupplierPenaltyDto
            {
                Id = p.Id,
                SupplierOrderId = p.SupplierOrderId,
                PenaltyAmount = p.PenaltyAmount,
                Reason = p.Reason,
                BuyerName = p.BuyerName,
                DeliveryDate = p.DeliveryDate.ToString("dd-MM-yyyy"),
                CreatedAt = p.CreatedAt.ToString("dd-MM-yyyy HH:mm")
            }).ToList();

            return Ok(new PagedResponseDto<SupplierPenaltyDto>
            {
                Items = dtoList,
                Page = page,
                PageSize = pageSize,
                TotalItems = paged.TotalItems,
                TotalPages = (int)Math.Ceiling((double)paged.TotalItems / pageSize)
            });
        }


        #endregion

    }
}
