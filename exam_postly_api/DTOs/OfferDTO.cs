using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CloudinaryDotNet;

namespace exam_postly_api.DTOs;

public class OfferDTO
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Price { get; set; }
    
    // [Required]
    // public string ImageUrl { get; set; }
    
    [Required]
    public IFormFile Image { get; set; }
}