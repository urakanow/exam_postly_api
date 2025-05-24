namespace exam_postly_api.Models;

public class Image
{
    public int Id { get; set; }
    
    required 
        public string Url { get; set; }
    
    required
        public int OfferId { get; set; }
    
    required
        public Offer Offer { get; set; }
}