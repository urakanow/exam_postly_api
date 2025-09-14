using System.Security.Claims;
using exam_postly_api.DTOs;
using exam_postly_api.Models;
using exam_postly_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace exam_postly_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly ChatService _chatService;

    public ChatController(IHubContext<ChatHub> chatHub, ChatService chatService)
    {
        _chatHub = chatHub;
        _chatService = chatService;
    }

    [Route("create-chat")]
    [HttpPost(Name = "CreateChat")]
    public async Task<Guid> CreateChat([FromBody] int offerId)
    {
        var buyerId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var existingChat = await _chatService.GetExistingChatAsync(buyerId, offerId);
        if(existingChat != null)
            return existingChat.Id;
        
        var chat = new Chat()
        {
            BuyerId = buyerId,
            OfferId = offerId
        };
        var savedChat = await _chatService.CreateChatAsync(chat);
        // _chatService.CreateDefaultChatAsync();
        return savedChat.Id;
    }

    [Route("chats")]
    [HttpGet(Name = "GetChats")]
    public async Task<List<ChatPreviewDTO>> GetChats()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        var chats = await _chatService.GetChatsAsync(userId);
        if(chats.Count == 0)
            return new List<ChatPreviewDTO>();
        
        var chatPreviewDTOs = chats.Select(chat => new ChatPreviewDTO() {
            Id = chat.Id,
            IsUnread = _chatService.IsChatUnread(chat, userId),
            SenderName = _chatService.GetOtherChatParticipant(chat, userId).Username,
            OfferTitle = chat.Offer.Title,
            LastMessage = _chatService.GetLastMessage(chat),
        }).ToList();
        return chatPreviewDTOs;
    }
    
    [Route("{chatId}")]
    [HttpGet(Name = "GetChat")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        var chat = await _chatService.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(new { message = "Chat not found" });

        // await _chatService.CreateDefaultSellerMessageAsync();
        // await _chatService.CreateDefaultBuyerMessageAsync();
        return Ok(new { message = "chat id: " + chatId, chat });
    }

    [Route("{chatId}/message")]
    [HttpPost(Name = "SendMessage")]
    public async Task<IActionResult> SendMessage(NewMessageDTO dto)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var chat = await _chatService.GetChatByIdAsync(dto.ChatId);
        if (chat == null)
            return NotFound(new { message = "Chat not found" });

        var message = new Message()
        {
            ChatId = dto.ChatId,
            SenderId = userId,
            Text = dto.MessageText
        };
        var savedMessage = await _chatService.CreateMessageAsync(message);
        
        await _chatHub.Clients.Group($"chat-{dto.ChatId}")
            .SendAsync("ReceiveMessage", new {
                id = savedMessage.Id,
                text = savedMessage.Text,
                senderId = savedMessage.SenderId,
                sentAt = savedMessage.SendingTimeUTC
            });
        
        return Ok();
    }

    [Route("read-message")]
    [HttpPost(Name = "ReadMessage")]
    public async Task<IActionResult> ReadMessage([FromBody] string messageId)
    {
        if (!Guid.TryParse(messageId, out var messageGuid))
        {
            return BadRequest("Invalid GUID format");
        }
        
        var message = await _chatService.GetMessageByIdAsync(messageGuid);
        if(message == null)
            return NotFound(new { message = "Message not found" });

        try
        {
            await _chatService.ReadMessageAsync(message);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500,  new { message = ex.Message });
        }
    }

    [Route("read-chat")]
    [HttpPost(Name = "ReadChat")]
    public async Task<IActionResult> ReadChat([FromBody] string chatId)
    {
        if (!Guid.TryParse(chatId, out var chatGuid))
        {
            return BadRequest("Invalid GUID format");
        }
        
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var chat = await _chatService.GetChatByIdAsync(chatGuid);
            foreach (var message in chat.Messages)
            {
                if (message.SenderId == userId)
                    continue;

                await _chatService.ReadMessageAsync(message);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}