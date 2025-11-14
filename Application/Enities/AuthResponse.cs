namespace Application.Enities
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}