using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Dtos;
using Order.API.Dtos.Buyer;
using Order.API.Dtos.Dashboard;
using Order.API.Dtos.Pagination;
using Order.API.Dtos.Supplier;
using Order.API.Helpers;
using Order.Application;
using Order.Domain.Interfaces;
using Order.Domain.Models;
using Order.Domain.Services;



namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly UserManager<AppOwner> _userManager;
        private readonly SignInManager<AppOwner> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<Buyer> _buyerRepo;
        private readonly ILogger<BuyerController> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IPasswordHasher<Supplier> _passwordHasher;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<MyOrder> _myOrderRepo;
        private readonly IGenericRepository<SupplierOrder> _supplierOrderRepo;
        private readonly IGenericRepository<ReturnOrderItem> _returnOrderRepo;
        private readonly IGenericRepository<Advertisement> _advertisementRepo;
        private readonly IGenericRepository<SupplierStatement> _supplierStatementRepo;

        public DashboardController(
            IGenericRepository<Product> productRepo,
            UserManager<AppOwner> userManager,
            SignInManager<AppOwner> signInManager,
            ITokenService tokenService,
            IConfiguration configuration,
             IGenericRepository<Buyer> buyerRepo,
             ILogger<BuyerController> logger,
            IMapper mapper,
             IGenericRepository<Supplier> supplierRepo,
            IPasswordHasher<Supplier> passwordHasher,
            INotificationService notificationService,
            IGenericRepository<MyOrder> myOrderRepo,
            IGenericRepository<SupplierOrder> supplierOrderRepo,
            IGenericRepository<ReturnOrderItem> returnOrderRepo,
            IGenericRepository<Advertisement> advertisementRepo,
            IGenericRepository<SupplierStatement> supplierStatementRepo)
        {
            _productRepo = productRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _buyerRepo = buyerRepo;
            _logger = logger;
            _mapper = mapper;
            _supplierRepo = supplierRepo;
            _passwordHasher = passwordHasher;
            _notificationService = notificationService;
            _myOrderRepo = myOrderRepo;
            _supplierOrderRepo = supplierOrderRepo;
            _returnOrderRepo = returnOrderRepo;
            _advertisementRepo = advertisementRepo;
            _supplierStatementRepo = supplierStatementRepo;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppOwnerResponseDto>> login([FromBody] DahboardLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appOwner = await _userManager.FindByEmailAsync(loginDto.Email);
            if (appOwner == null)
                return Unauthorized("Invalid email");

            var result = await _signInManager.PasswordSignInAsync(appOwner.UserName, loginDto.Password, loginDto.RememberMe, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid password.");

            var token = await _tokenService.CreateTokenAsync(appOwner, _userManager, loginDto.RememberMe);
            var expiration = loginDto.RememberMe
                ? DateTime.Now.AddDays(double.Parse(_configuration["JWT:RememberMeDurationInDays"]))
                : DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]));

            _tokenService.StoreTokenInCookie(token, expiration, HttpContext);

            var roles = await _userManager.GetRolesAsync(appOwner);
            return Ok(new AppOwnerResponseDto
            {
                Id = appOwner.Id,
                Email = appOwner.Email,
                Roles = roles.ToList(),
                Token = token
            });
        }

        #region App Owner
        [HttpPost("addAppOwner")]
        public async Task<IActionResult> Register([FromBody] RegisterAppOwnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email is already registered.");

            if (dto.Role != "Editor" && dto.Role != "Admin")
                return BadRequest("Role must be either 'Editor' or 'Admin'.");

            var appOwner = new AppOwner
            {
                Email = dto.Email,
                UserName = dto.Email.Split("@")[0]
            };

            var result = await _userManager.CreateAsync(appOwner, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(appOwner, dto.Role);

            return Ok(new
            {
                message = "AppOwner registered successfully",
                userId = appOwner.Id,
                email = appOwner.Email,
                role = dto.Role
            });
        }

        [HttpPut("updateAppOwner/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAppOwner(string id, [FromBody] UpdateAppOwnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("AppOwner not found.");

                user.Email = dto.Email ?? user.Email;

                if (!string.IsNullOrEmpty(dto.Email))
                    user.UserName = user.Email.Split("@")[0];

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }

                return Ok("AppOwner updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("getAppOwnerById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppOwnerById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id is required.");

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("AppOwner not found.");

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new AppOwnerResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("getAllAppOwners")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAppOwners(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            try
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));

                var totalItems = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = new List<AppOwnerResponseDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    items.Add(new AppOwnerResponseDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = roles.ToList()
                    });
                }

                return Ok(new PagedResult<AppOwnerResponseDto>
                {
                    Items = items,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        } 
        #endregion



        #region Main Page

        [HttpGet("buyers/count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBuyersCount()
        {
            try
            {
                var buyers = await _buyerRepo.GetAllAsync();
                var count = buyers.Count;

                return Ok(new { message = "Buyers count retrieved successfully", Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving Buyers count: {ex.Message}");
            }
        }

        [HttpGet("suppliers/count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuppliersCount()
        {
            try
            {
                var suppliers = await _supplierRepo.GetAllAsync();
                var count = suppliers.Count;

                return Ok(new { message = "Suppliers count retrieved successfully", Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving suppliers count: {ex.Message}");
            }
        }

        [HttpGet("orders/pending/today-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingOrdersCountToday()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var orders = await _myOrderRepo.GetAllAsync(o =>
                    o.Status == OrderStatus.Pending &&
                    o.OrderDate == today
                );

                var count = orders.Count();

                return Ok(new { message = "Pending orders count for today retrieved successfully", Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving pending orders count for today: {ex.Message}");
            }
        }

        [HttpGet("orders/pending/month-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingOrdersCountThisMonth()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateOnly(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var orders = await _myOrderRepo.GetAllAsync(o =>
                    o.Status == OrderStatus.Pending &&
                    o.OrderDate >= startOfMonth &&
                    o.OrderDate <= endOfMonth
                );

                var count = orders.Count();

                return Ok(new { message = "Pending orders count for this month retrieved successfully", Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving pending orders count for this month: {ex.Message}");
            }
        }



        #endregion


        #region Buyer
        [HttpGet("getInactiveBuyers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<BuyerToReturnDto>>> GetInactiveBuyers(int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving inactive buyers, page {Page}, pageSize {PageSize}", page, pageSize);

                var buyers = await _buyerRepo.GetPagedAsync(
                    page: page,
                    pageSize: pageSize,
                    predicate: b => !b.IsActive,
                    orderBy: b => b.Id,
                    descending: false
                );

                var mappedBuyers = _mapper.Map<PagedResult<BuyerToReturnDto>>(buyers);
                _logger.LogInformation("Retrieved {Count} inactive buyers successfully", mappedBuyers.Items.Count);
                var totalItems = mappedBuyers.Items.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var response = new PagedResponseDto<BuyerToReturnDto>
                {
                    Items = mappedBuyers.Items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inactive buyers");
                return StatusCode(500, $"Error retrieving inactive buyers: {ex.Message}");
            }
        }

        [HttpPut("activateBuyer/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BuyerToReturnDto>> ActivateBuyer(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to activate buyer with ID {BuyerId}", id);

                var buyer = await _buyerRepo.GetByIdAsync(id);
                if (buyer == null)
                {
                    _logger.LogWarning("Buyer with ID {BuyerId} not found", id);
                    return NotFound("Buyer not found.");
                }

                if (buyer.IsActive)
                {
                    _logger.LogWarning("Buyer with ID {BuyerId} is already activated", id);
                    return BadRequest("Buyer account is already activated.");
                }

                buyer.IsActive = true;

                _buyerRepo.Update(buyer);
                await _buyerRepo.SaveChangesAsync();

                // إرسال إشعار للـ Buyer
                if (!string.IsNullOrWhiteSpace(buyer.DeviceToken))
                {
                    await _notificationService.SendNotificationAsync(
                        buyer.DeviceToken,
                        "Account Activated 🎉",
                        "Your account has been activated successfully. Welcome!"
                    );
                }

                var buyerToReturn = _mapper.Map<BuyerToReturnDto>(buyer);
                _logger.LogInformation("Buyer with ID {BuyerId} activated successfully", id);
                return Ok(new { message = "Account Activated Successfully", Data = buyerToReturn });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating buyer with ID {BuyerId}", id);
                return StatusCode(500, $"Error activating buyer: {ex.Message}");
            }
        }

        #endregion


        #region Supplier
        [HttpPut("updateSupplier/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SupplierToReturnDto>> UpdateSupplier(int id, [FromForm] UpdateSupplierDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null)
                return NotFound("Supplier not found.");

            // التحقق من إن SupplierType صالح إذا تم تمريره
            if (!string.IsNullOrEmpty(updateDto.SupplierType))
            {
                try
                {
                    if (!Enum.TryParse<SupplierType>(updateDto.SupplierType, true, out var supplierType))
                        return BadRequest($"Invalid SupplierType. Must be one of: {string.Join(", ", Enum.GetNames(typeof(SupplierType)))}");
                    supplier.SupplierType = supplierType;
                }
                catch
                {
                    return BadRequest("Invalid SupplierType format.");
                }
            }

            // التحقق من رقم الهاتف إذا تم تمريره
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
            {
                var existingSupplier = await _supplierRepo.GetFirstOrDefaultAsync(b => b.PhoneNumber == updateDto.PhoneNumber && b.Id != id);
                if (existingSupplier != null)
                    return BadRequest("Phone number already exists.");
            }

            // تحديث الحقول إذا تم تمريرها
            supplier.Email = updateDto.Email ?? supplier.Email;
            supplier.Name = updateDto.Name ?? supplier.Name;
            supplier.CommercialName = updateDto.CommercialName ?? supplier.CommercialName;
            supplier.PhoneNumber = updateDto.PhoneNumber ?? supplier.PhoneNumber;
            supplier.WarehouseLocation = updateDto.WarehouseLocation ?? supplier.WarehouseLocation;
            supplier.WarehouseAddress = updateDto.WarehouseAddress ?? supplier.WarehouseAddress;
            supplier.DeliveryMethod = updateDto.DeliveryMethod ?? supplier.DeliveryMethod;
            supplier.ProfitPercentage = updateDto.ProfitPercentage ?? supplier.ProfitPercentage;
            supplier.MinimumOrderItems = updateDto.MinimumOrderItems ?? supplier.MinimumOrderItems;
            supplier.DeliveryDays = updateDto.DeliveryDays ?? supplier.DeliveryDays;


            // تحديث كلمة المرور إذا تم تمريرها
            if (!string.IsNullOrEmpty(updateDto.Password))
                supplier.Password = _passwordHasher.HashPassword(supplier, updateDto.Password);

            // تحديث صورة المستودع إذا تم تمريرها
            if (updateDto.WarehouseImage != null)
            {
                try
                {
                    var newImageUrl = DocumentSettings.UploadFile(updateDto.WarehouseImage, "SupplierWarehouse");
                    if (!string.IsNullOrEmpty(supplier.WarehouseImageUrl))
                        DocumentSettings.DeleteFile(supplier.WarehouseImageUrl, "SupplierWarehouse");
                    supplier.WarehouseImageUrl = newImageUrl;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error uploading image: {ex.Message}");
                }
            }

            try
            {
                _supplierRepo.Update(supplier);
                await _supplierRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(supplier.WarehouseImageUrl))
                    DocumentSettings.DeleteFile(supplier.WarehouseImageUrl, "SupplierWarehouse");
                return StatusCode(500, $"Error updating supplier: {ex.Message}");
            }

            var supplierToReturn = _mapper.Map<SupplierToReturnDto>(supplier);
            return Ok(new { message = "Supplier Updated Successfully", Data = supplierToReturn });
        }

        [HttpDelete("deleteSupplier/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null)
                return NotFound("Supplier not found.");

            if (!string.IsNullOrEmpty(supplier.WarehouseImageUrl))
                DocumentSettings.DeleteFile(supplier.WarehouseImageUrl, "SupplierWarehouse");

            _supplierRepo.Delete(supplier);
            await _supplierRepo.SaveChangesAsync();

            return Ok(new { message = "Supplier deleted successfully" });
        }

        [HttpGet("getAllSuppliers")]
        [Authorize]
        public async Task<ActionResult<PagedResponseDto<SupplierDto>>> GetAllSuppliers(
        int page = 1,
        int pageSize = 10,
        string? name = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                var suppliersQuery = await _supplierRepo.GetAllAsync();

                // فلترة بالاسم (nullable)
                if (!string.IsNullOrWhiteSpace(name))
                {
                    suppliersQuery = suppliersQuery
                        .Where(s => s.Name.Contains(name) || s.CommercialName.Contains(name))
                        .ToList();
                }

                var mappedSuppliers = suppliersQuery.Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email,
                    CommercialName = s.CommercialName,
                    PhoneNumber = s.PhoneNumber,
                    SupplierType = s.SupplierType.ToString(),
                    WarehouseLocation = s.WarehouseLocation,
                    WarehouseAddress = s.WarehouseAddress,
                    WarehouseImageUrl = $"{_configuration["BaseApiUrl"]}{s.WarehouseImageUrl}",
                    DeliveryMethod = s.DeliveryMethod,
                    ProfitPercentage = s.ProfitPercentage,
                    MinimumOrderPrice = 0,
                    MinimumOrderItems = s.MinimumOrderItems,
                    DeliveryDays = s.DeliveryDays,
                    WalletBalance = s.WalletBalance
                }).ToList();

                var totalItems = mappedSuppliers.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedSuppliers = mappedSuppliers
                    .OrderBy(i => i.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagedResponseDto<SupplierDto>
                {
                    Items = pagedSuppliers,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving suppliers: {ex.Message}");
            }
        }


        [HttpPost("addToSupplierWallet")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AddToSupplierWallet([FromBody] AddToSupplierWalletDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
            if (supplier == null)
                return NotFound("Supplier not found.");

            if (dto.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            supplier.WalletBalance ??= 0;
            supplier.WalletBalance += dto.Amount;

            _supplierRepo.Update(supplier);
            await _supplierRepo.SaveChangesAsync();

            return Ok(new
            {
                message = $"Amount {dto.Amount} added successfully to supplier {supplier.Name} wallet.",
                newBalance = supplier.WalletBalance
            });
        }


        #endregion


        [HttpGet("getSupplierOrders")]
        public async Task<IActionResult> GetSupplierOrders(
        [FromQuery] string? orderStatus,
        [FromQuery] string? buyerName,
        [FromQuery] string? supplierType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            try
            {
                // فلتر الحالة
                OrderStatus? statusFilter = null;
                if (!string.IsNullOrWhiteSpace(orderStatus))
                {
                    if (Enum.TryParse<OrderStatus>(orderStatus, true, out var parsedStatus))
                        statusFilter = parsedStatus;
                    else
                        return BadRequest("Invalid order status.");
                }

                // فلتر نوع المورد
                SupplierType? supplierTypeFilter = null;
                if (!string.IsNullOrWhiteSpace(supplierType))
                {
                    if (Enum.TryParse<SupplierType>(supplierType, true, out var parsedSupplierType))
                        supplierTypeFilter = parsedSupplierType;
                    else
                        return BadRequest("Invalid supplier type.");
                }

                // الفلتر الرئيسي
                Expression<Func<SupplierOrder, bool>>? filter = o =>
                    (!statusFilter.HasValue || o.Status == statusFilter.Value) &&
                    (string.IsNullOrWhiteSpace(buyerName) || o.BuyerName.Contains(buyerName)) &&
                    (!supplierTypeFilter.HasValue || o.Supplier.SupplierType == supplierTypeFilter.Value);

                // جلب البيانات
                var orders = await _supplierOrderRepo.GetAllAsync(
                    filter: filter,
                    includes: new Expression<Func<SupplierOrder, object>>[]
                    {
                o => o.Supplier,
                o => o.Items
                    }
                );

                // pagination
                var totalCount = orders.Count();
                var pagedOrders = orders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // تحويل للـ DTO
                var orderDtos = pagedOrders.Select(o => new ReturnedSupplierOrderDto
                {
                    Id = o.Id,
                    TotalAmount = o.TotalAmount,
                    DeliveryDate = o.DeliveryDate,
                    PaymentMethod = o.PaymentMethod,
                    Status = o.Status.ToString(),
                    WalletPaymentAmount = o.WalletPaymentAmount,

                    SupplierId = o.SupplierId,
                    SupplierName = o.Supplier?.Name,
                    SupplierType = o.Supplier.SupplierType.ToString(),

                    BuyerName = o.BuyerName,
                    BuyerPhone = o.BuyerPhone,
                    PropertyName = o.PropertyName,
                    PropertyAddress = o.PropertyAddress,
                    PropertyLocation = o.PropertyLocation,

                    Items = o.Items.Select(i => new SupplierOrderItemDto
                    {
                        Id = i.Id,
                        SupplierProductId = i.SupplierProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.Quantity * i.UnitPrice
                    }).ToList()
                });

                return Ok(new
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = orderDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("getReturnOrders")]
        public async Task<IActionResult> GetReturnOrders(
        [FromQuery] string? buyerName,
        [FromQuery] int? day,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            try
            {
                // فلتر بالاسم + التاريخ
                Expression<Func<ReturnOrderItem, bool>>? filter = r =>
                    (string.IsNullOrWhiteSpace(buyerName) ||
                     r.SupplierOrderItem.SupplierOrder.BuyerName.Contains(buyerName)) &&

                    // فلترة بالتاريخ (Year / Month / Day)
                    ((!day.HasValue && !month.HasValue && !year.HasValue) ||

                     (year.HasValue && !month.HasValue && !day.HasValue &&
                      r.ReturnDate.Year == year.Value) ||

                     (year.HasValue && month.HasValue && !day.HasValue &&
                      r.ReturnDate.Year == year.Value &&
                      r.ReturnDate.Month == month.Value) ||

                     (year.HasValue && month.HasValue && day.HasValue &&
                      r.ReturnDate.Year == year.Value &&
                      r.ReturnDate.Month == month.Value &&
                      r.ReturnDate.Day == day.Value));

                var returnOrders = await _returnOrderRepo.GetAllAsync(
                    filter: filter,
                    includes: new Expression<Func<ReturnOrderItem, object>>[]
                    {
                r => r.SupplierOrderItem,
                r => r.SupplierOrderItem.SupplierOrder,
                r => r.SupplierOrderItem.SupplierOrder.Supplier
                    }
                );

                var totalCount = returnOrders.Count();

                // ✅ Group by SupplierOrderId
                var groupedReturns = returnOrders
                    .GroupBy(r => r.SupplierOrderItem.SupplierOrderId)
                    .Select(g => new GroupedReturnOrderDto
                    {
                        SupplierOrderId = g.Key,
                        OrderDate = g.First().SupplierOrderItem.SupplierOrder.DeliveryDate.ToString("dd-MM-yyyy"),
                        BuyerName = g.First().SupplierOrderItem.SupplierOrder.BuyerName,
                        TotalReturnedQuantity = g.Sum(x => x.ReturnedQuantity),
                        TotalRefundAmount = g.Sum(x => x.ReturnedQuantity * x.SupplierOrderItem.UnitPrice),

                        Items = g.Select(r => new ReturnOrderDto
                        {
                            Id = r.Id,
                            SupplierOrderItemId = r.SupplierOrderItemId,
                            SupplierOrderId = g.Key, // ✅ added
                            ReturnedQuantity = r.ReturnedQuantity,
                            ReturnDate = r.ReturnDate.ToString("dd-MM-yyyy"),
                            ProductName = r.SupplierOrderItem.ProductName,
                            UnitPrice = r.SupplierOrderItem.UnitPrice
                        }).ToList()
                    })
                    .OrderByDescending(x => x.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = groupedReturns
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("addAdvertisement")]
        public async Task<IActionResult> AddAdvertisement([FromForm] CreateAdvertisementDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Image is required.");

            // Check allowed file types (jpg, png, jpeg)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(dto.Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only JPG, JPEG, and PNG files are allowed.");

            // Check max file size
            var maxFileSize = 5 * 1024 * 1024;
            if (dto.Image.Length > maxFileSize)
                return BadRequest("File size must be less than 5MB.");

            string? imageUrl = null;
            try
            {
                // رفع الصورة
                imageUrl = DocumentSettings.UploadFile(dto.Image, "Advertisements");

                var ad = new Advertisement
                {
                    Name = dto.Name,
                    ImageUrl = imageUrl
                };

                await _advertisementRepo.AddAsync(ad);
                var result = await _advertisementRepo.SaveChangesAsync();

                if (result == 0)
                {
                    if (imageUrl != null)
                        DocumentSettings.DeleteFile(imageUrl, "Advertisements");

                    return StatusCode(500, "Failed to save advertisement.");
                }

                return Ok(new
                {
                    Message = "Advertisement added successfully",
                    Data = ad
                });
            }
            catch (Exception ex)
            {
                if (imageUrl != null)
                    DocumentSettings.DeleteFile(imageUrl, "Advertisements");

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("deleteAdvertisement/{id}")]
        public async Task<IActionResult> DeleteAdvertisement(int id)
        {
            var ad = await _advertisementRepo.GetByIdAsync(id);
            if (ad == null)
                return NotFound("Advertisement not found.");

            DocumentSettings.DeleteFile(ad.ImageUrl, "Advertisements");

            _advertisementRepo.Delete(ad);
            await _advertisementRepo.SaveChangesAsync();

            return Ok(new { Message = "Advertisement deleted successfully" });
        }

        [HttpPut("updateAdvertisement/{id}")]
        public async Task<IActionResult> UpdateAdvertisement(int id, [FromForm] UpdateAdvertisementDto dto)
        {
            var ad = await _advertisementRepo.GetByIdAsync(id);
            if (ad == null)
                return NotFound("Advertisement not found.");

            string? newImageUrl = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    ad.Name = dto.Name;

                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(dto.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("Only JPG, JPEG, and PNG files are allowed.");

                    var maxFileSize = 5 * 1024 * 1024;
                    if (dto.Image.Length > maxFileSize)
                        return BadRequest("File size must be less than 5MB.");

                    newImageUrl = DocumentSettings.UploadFile(dto.Image, "Advertisements");

                    if (!string.IsNullOrEmpty(ad.ImageUrl))
                        DocumentSettings.DeleteFile(ad.ImageUrl, "Advertisements");

                    ad.ImageUrl = newImageUrl;
                }

                _advertisementRepo.Update(ad);
                var result = await _advertisementRepo.SaveChangesAsync();

                if (result == 0)
                {
                    if (newImageUrl != null)
                        DocumentSettings.DeleteFile(newImageUrl, "Advertisements");

                    return StatusCode(500, "Failed to update advertisement.");
                }

                return Ok(new
                {
                    Message = "Advertisement updated successfully",
                    Data = ad
                });
            }
            catch (Exception ex)
            {
                if (newImageUrl != null)
                    DocumentSettings.DeleteFile(newImageUrl, "Advertisements");

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("getAllAdvertisements")]
        public async Task<ActionResult<PagedResult<AdvertisementDto>>> GetAllAdvertisements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            Expression<Func<Advertisement, bool>> predicate =
                a => string.IsNullOrEmpty(search) || a.Name.Contains(search);

            Expression<Func<Advertisement, object>> orderBy = a => a.Id;

            var pagedResult = await _advertisementRepo.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                predicate: predicate,
                orderBy: orderBy,
                descending: false
            );

            var result = new PagedResult<AdvertisementDto>
            {
                TotalItems = pagedResult.TotalItems,
                Items = pagedResult.Items.Select(a => new AdvertisementDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = $"{_configuration["BaseApiUrl"]}{a.ImageUrl}"
                }).ToList()
            };

            return Ok(result);
        }



        [HttpGet("getSupplierStatements")]
        [Authorize]
        public async Task<ActionResult<PagedResult<SupplierStatementDashboardDto>>> GetSupplierStatements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? commercialName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            var query = _supplierStatementRepo.GetAllQueryable(
                include: q => q.Include(s => s.Supplier)
            );

            // date filteration
            if (fromDate.HasValue)
                query = query.Where(s => s.CreatedAt.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(s => s.CreatedAt.Date <= toDate.Value.Date);

            // commercialName filteration
            if (!string.IsNullOrWhiteSpace(commercialName))
                query = query.Where(s => s.Supplier.CommercialName.Contains(commercialName));

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierStatementDashboardDto
                {
                    Id = s.Id,
                    SupplierName = s.Supplier.Name,
                    CommercialName = s.Supplier.CommercialName,
                    SupplierType = s.Supplier.SupplierType.ToString(),
                    WalletBalance = s.Supplier.WalletBalance,
                    Amount = s.Amount,
                    Title = s.Title,
                    BuyerName = s.BuyerName,
                    PropertyName = s.PropertyName,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<SupplierStatementDashboardDto>
            {
                Items = items,
                TotalItems = totalItems
            });
        }





    }
}

