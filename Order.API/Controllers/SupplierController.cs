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

        public SupplierController(
            IGenericRepository<Supplier> supplierRepo,
             IGenericRepository<SupplierProduct> supplierProductRepo,
            IPasswordHasher<Supplier> passwordHasher,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<BuyerController> logger,
            IMapper mapper,
            IGenericRepository<Product> productRepo,
            IGenericRepository<SupplierOrder> supplierOrderRepo)
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



        [HttpGet("getSupplierOrders")]
        [Authorize]
        public async Task<ActionResult> GetSupplierOrders()
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
                return Unauthorized("Supplier ID not found in token.");

            var orders = await _supplierOrderRepo.GetAllAsync(
                so => so.SupplierId == int.Parse(supplierId),
                q => q.Include(so => so.Items)
                      .ThenInclude(oi => oi.SupplierProduct)
                      .ThenInclude(sp => sp.Product)
            );

            var result = orders.Select(so => new
            {
                supplierOrderId = so.Id,
                buyerName = so.BuyerName,
                buyerPhone = so.BuyerPhone,
                propertyName = so.PropertyName,
                propertyAddress = so.PropertyAddress,
                propertyLocation = so.PropertyLocation,
                totalAmount = so.TotalAmount,
                deliveryDate = so.DeliveryDate.ToString("dd-MM-yyyy"),
                paymentMethod = so.PaymentMethod,
                status = so.Status.ToString(),
                items = so.Items.Select(i => new
                {
                    productName = i.ProductName,
                    quantity = i.Quantity,
                    unitPrice = i.UnitPrice
                })
            });

            return Ok(result);
        }


    }
}
