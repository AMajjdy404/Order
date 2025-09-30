namespace Order.API.Dtos
{
    public class OtpRequest
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
