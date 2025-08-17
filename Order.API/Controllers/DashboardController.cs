using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            INotificationService notificationService)
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
                Username = appOwner.UserName,
                Email = appOwner.Email,
                Roles = roles.ToList(),
                Token = token
            });
        }


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
            supplier.MinimumOrderPrice = updateDto.MinimumOrderPrice ?? supplier.MinimumOrderPrice;
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
        public async Task<ActionResult<PagedResponseDto<SupplierDto>>> GetAllSuppliers(int page = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than zero" });

            try
            {
                var suppliers = await _supplierRepo.GetAllAsync();
                var mappedSuppliers = new List<SupplierDto>();

                foreach (var supplier in suppliers)
                {
                    mappedSuppliers.Add(new SupplierDto
                    {
                        Id = supplier.Id,
                        Name = supplier.Name,
                        Email = supplier.Email,
                        CommercialName = supplier.CommercialName,
                        PhoneNumber = supplier.PhoneNumber,
                        SupplierType = supplier.SupplierType.ToString(),
                        WarehouseLocation = supplier.WarehouseLocation,
                        WarehouseAddress = supplier.WarehouseAddress,
                        WarehouseImageUrl = $"{_configuration["BaseApiUrl"]}{supplier.WarehouseImageUrl}",
                        DeliveryMethod = supplier.DeliveryMethod,
                        ProfitPercentage = supplier.ProfitPercentage,
                        MinimumOrderPrice = supplier.MinimumOrderPrice,
                        MinimumOrderItems = supplier.MinimumOrderItems,
                        DeliveryDays = supplier.DeliveryDays
                    });
                }

                var totalItems = mappedSuppliers.Count;
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
    }


    #endregion


}

