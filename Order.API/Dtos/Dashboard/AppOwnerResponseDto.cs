namespace Order.API.Dtos.Dashboard
{
    public class AppOwnerResponseDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

        public string Token { get; set; }
    }
}
