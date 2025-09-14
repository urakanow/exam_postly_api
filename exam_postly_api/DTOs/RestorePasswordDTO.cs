namespace exam_postly_api.DTOs;

public class RestorePasswordDTO
{
    required
        public string token { get; set; }
    
    required
        public string newPassword { get; set; }
}