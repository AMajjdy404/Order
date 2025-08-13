namespace Order.API.Dtos.Supplier
{
    public class UpdateOrderItemQuantityDto
    {
        public int ItemId { get; set; }
        public int NewQuantity { get; set; }
    }

}
