using exam_postly_api.DTOs;
using exam_postly_api.Models;
using Microsoft.EntityFrameworkCore;

namespace exam_postly_api.Services;

public class ChatService
{
    private readonly ApplicationDBContext _dbContext;
    private readonly UserService _userService;

    public ChatService(ApplicationDBContext dbContext, UserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<Chat> CreateChatAsync(Chat chat)
    {
        var savedChat = await _dbContext.Chats.AddAsync(chat);
        await _dbContext.SaveChangesAsync();
        
        return savedChat.Entity;
    }

    public async Task CreateDefaultChatAsync()
    {
        // var buyer = await _userService.GetUserByEmailAsync("urakanow@gmail.com");
        // var seller = await _userService.GetUserByEmailAsync("wonakaru@gmail.com");
        // var buyer = await _dbContext.Users.FindAsync("urakanow@gmail.com");
        // var seller = await _dbContext.Users.FindAsync("wonakaru@gmail.com");
        var chat = new Chat
        {
            BuyerId = 57,
            OfferId = 36,
            // Buyer = buyer,
            // Seller = seller
        };
        
        await CreateChatAsync(chat);
    }

    public async Task<Chat?> GetChatByIdAsync(Guid chatId)
    {
        var chat = await _dbContext.Chats
            .Include(chat => chat.Messages)
            .FirstOrDefaultAsync(chat => chat.Id == chatId);
        return chat;
    }

    public async Task<Message> CreateMessageAsync(Message message)
    {
        var savedMessage = await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
        
        return savedMessage.Entity;
    }

    public async Task CreateDefaultBuyerMessageAsync()
    {
        var message = new Message()
        {
            Text = "test message from buyer",
            ChatId = new Guid(),
            SenderId = 57
        };
        
        await CreateMessageAsync(message);
    }
    public async Task CreateDefaultSellerMessageAsync()
    {
        var message = new Message()
        {
            Text = "test message from seller",
            ChatId = new  Guid(),
            SenderId = 41
        };
        
        await CreateMessageAsync(message);
    }

    public async Task<ICollection<Message>> GetMessagesAsync(Guid chatId)
    {
        var chat = await _dbContext.Chats
            .Include(chat => chat.Messages)
            .FirstOrDefaultAsync(chat => chat.Id == chatId);
        return chat.Messages;
    }

    public async Task<Chat?> GetExistingChatAsync(int buyerId, int offerId)
    {
        var chat = await _dbContext.Chats.FirstOrDefaultAsync(chat => chat.BuyerId == buyerId && chat.OfferId == offerId);
        return chat;
    }

    public async Task<List<Chat>> GetChatsAsync(int userId)
    {
        var buyerChats = await _dbContext.Chats
        .Include(chat => chat.Messages)
        .Include(chat => chat.Buyer)
        .Include(chat => chat.Offer)
        .ThenInclude(offer => offer.User)
        .Where(chat => chat.BuyerId == userId)
        .ToListAsync();
        var sellerChats = await _dbContext.Chats
        .Include(chat => chat.Messages)
        .Include(chat => chat.Buyer)
        .Include(chat => chat.Offer)
        .ThenInclude(offer => offer.User)
        .Where(chat => chat.Offer.UserId == userId)
        .ToListAsync();

        return buyerChats.Concat(sellerChats).ToList();       // return chats;
    }

    public async Task<Message?> GetMessageByIdAsync(Guid messageId)
    {
        var message = await _dbContext.Messages.FirstOrDefaultAsync(message => message.Id == messageId);
        return message;
    }

    public async Task ReadMessageAsync(Message message)
    {
        message.IsRead = true;
        await _dbContext.SaveChangesAsync();
    }

    public bool IsChatUnread(Chat chat, int userId)
    {
        var messages = chat.Messages;
        return messages.Any(message => message.SenderId != userId && !message.IsRead);
    }

    public Message? GetLastMessage(Chat chat)
    {
        var lastMessage = chat.Messages
            .OrderBy(m => m.SendingTimeUTC)
            .LastOrDefault();
        return lastMessage;
    }

    public User GetOtherChatParticipant(Chat chat, int userId)
    {
        if (chat.BuyerId == userId)
        {
            return chat.Offer.User;
        }

        return chat.Buyer;
    }
}