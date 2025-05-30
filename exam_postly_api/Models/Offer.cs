namespace exam_postly_api.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        
        required 
            public int State { get; set; }

        required
        public string Title { get; set; }
        
        required
            public string Description { get; set; }

        required
            public int Category {get; set;}
        
        required
        public double Price { get; set; }
        
        required
            public string Contacter {get; set;}
        
        required
            public string Email {get; set;}
        
        required
            public string PhoneNumber {get; set;}
        
        required
            public string Address {get; set;}
        
        required
        public int UserId { get; set; }
        
        required
        public User User { get; set; }

        required
            public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}
