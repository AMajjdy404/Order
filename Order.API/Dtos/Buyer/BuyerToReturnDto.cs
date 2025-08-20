namespace Order.API.Dtos.Buyer
{
    public class BuyerToReturnDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string PropertyLocation { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyInsideImagePath { get; set; }
        public string PropertyOutsideImagePath { get; set; }
        public bool IsActive { get; set; }
        public decimal WalletBalance { get; set; }
        public string? DeviceToken { get; set; }
        public string Token { get; set; } = "";
    }
}
