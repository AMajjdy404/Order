namespace Order.API.Dtos.Dashboard
{
    public class SupplierStatementDashboardDto
    {
        public int Id { get; set; }
        public string SupplierName { get; set; }
        public string SupplierType { get; set; }
        public string CommercialName { get; set; }
        public decimal? WalletBalance { get; set; }
        public decimal Amount { get; set; }
        public string Title { get; set; }
        public string BuyerName { get; set; }
        public string PropertyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
