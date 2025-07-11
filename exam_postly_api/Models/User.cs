namespace exam_postly_api.Models
{
    public class User
    {
        public int Id { get; set; }

        required
            public string Username { get; set; }
        
        required
            public string Email { get; set; } = "";

        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }
        public string PhoneNumber { get; set; } = "";
        public string GoogleId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string City { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Address { get; set; } = "";
        public string ApartmentNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
        public bool IsVerified { get; set; } = false;
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<RestoreToken> RestoreTokens { get; set; } = new List<RestoreToken>();
        public VerifyToken? VerifyToken { get; set; } =  null;
    }
}
