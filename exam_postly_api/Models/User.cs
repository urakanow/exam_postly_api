namespace exam_postly_api.Models
{
    public class User
    {
        public int Id { get; set; }

        required
        public string Username { get; set; }

        required
        public string Email { get; set; }

        required
        public string PasswordHash { get; set; }

        required
        public string Salt { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
