namespace exam_postly_api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        required
        public string TokenHash { get; set; }

        required
        public int UserId { get; set; }

        required
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;//there'll be automatic cleanups in the db

        required
        public User User { get; set; }
    }
}
