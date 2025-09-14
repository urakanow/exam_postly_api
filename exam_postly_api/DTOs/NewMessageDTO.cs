namespace exam_postly_api.DTOs;

public class NewMessageDTO
{
    public Guid ChatId { get; set; }
    public string MessageText { get; set; }
}