namespace exam_postly_api.Models;

public class Favorite
{
    public int Id { get; set; }
    required 
        public int UserId { get; set; }
    required
        public int OfferId { get; set; }
    required
        public User User { get; set; }
    required 
        public Offer Offer { get; set; }
}