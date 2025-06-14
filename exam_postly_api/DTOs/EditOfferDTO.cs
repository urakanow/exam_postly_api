using System.ComponentModel.DataAnnotations;
using exam_postly_api.Models;

namespace exam_postly_api.DTOs;

public class EditOfferDTO
{
    public int Id { get; set; }
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
        
    // required
    //     public int UserId { get; set; }

    required
        public List<EditOfferImage> Images { get; set; }
}

public class EditOfferImage
{
    public IFormFile? FileImage { get; set; }
    public string? CloudinaryImage { get; set; }
}