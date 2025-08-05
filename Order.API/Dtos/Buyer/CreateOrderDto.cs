namespace Order.API.Dtos.Buyer
{
    public class CreateOrderDto
    {
        public List<OrderItemDto> Items { get; set; }
    }
}
