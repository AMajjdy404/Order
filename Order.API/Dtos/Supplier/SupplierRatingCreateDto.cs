namespace Order.API.Dtos.Supplier
{
    public class SupplierRatingCreateDto
    {
        public int Rate { get; set; } // من 1 لـ 5 مثلاً
        public string Comment { get; set; }
    }

}
