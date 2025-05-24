using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CloudinaryDotNet;

namespace exam_postly_api.DTOs;

public class CreateOfferDTO
{
    public int Id { get; set; }

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
        public List<IFormFile> Images { get; set; }
}