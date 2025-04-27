using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace exam_postly_api.DTOs;

public class OfferDTO
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Price { get; set; }
    
    [Required]
    public string ImageUrl { get; set; }
}