using exam_postly_api.Models;

namespace exam_postly_api.DTOs;

public class ChatPreviewDTO
{
    public Guid Id { get; set; }
    public bool IsUnread { get; set; }
    public string SenderName { get; set; }
    public string OfferTitle { get; set; }
    public Message? LastMessage { get; set; }
}