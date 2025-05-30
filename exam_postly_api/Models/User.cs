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

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string City { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Address { get; set; } = "";
        public string ApartmentNumber { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
