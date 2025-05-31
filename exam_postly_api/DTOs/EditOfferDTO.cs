using System.ComponentModel.DataAnnotations;

namespace exam_postly_api.DTOs;

public class EditOfferDTO
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Price { get; set; }
}