namespace exam_postly_api.Models
{
    public class Offer
    {
        public int Id { get; set; }

        required
        public string Title { get; set; }

        required
        public double Price { get; set; }

        required
        public string ImageUrl { get; set; }
        
        required
        public int UserId { get; set; }
        
        required
        public User User { get; set; }
    }
}
