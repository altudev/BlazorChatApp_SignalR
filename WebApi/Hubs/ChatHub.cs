using Microsoft.AspNetCore.SignalR;
using Shared;

namespace WebApi.Hubs;

public sealed class ChatHub : Hub
{
    private static readonly List<Message> messages = new();

    public Task SendMessageAsync(string userName, string message)
    {
        messages.Add(new Message(userName, message));

        return Clients
        .AllExcept(Context.ConnectionId)
        .SendAsync("ReceiveMessage", userName, message);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString();

            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserConnected", userName);

            await Clients.Caller.SendAsync("ReceiveMessages", messages);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString();

        await Clients.Others.SendAsync("UserDisconnected", userName);

        await base.OnDisconnectedAsync(exception);
    }
}
