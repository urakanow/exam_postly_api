using Microsoft.AspNetCore.SignalR;

namespace exam_postly_api.Services;

public class ChatHub : Hub
{
    public async Task JoinConversation(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    public async Task SendMessage(int chatId, string message)
    {
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", message);
    }
}