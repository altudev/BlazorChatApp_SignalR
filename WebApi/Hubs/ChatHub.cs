using Microsoft.AspNetCore.SignalR;
using Shared;

namespace WebApi.Hubs;

public sealed class ChatHub : Hub
{
    private static readonly List<User> connectedUsers = new();
    private static readonly List<Lobby> lobbies = new ();

    public Task SendMessageAsync(Guid lobbyId, string userMessage)
    {
        var user = connectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        var lobby = lobbies.FirstOrDefault(l => l.Id == lobbyId);

        if (lobby is not null)
        {
            var message = new Message(lobbyId, user, userMessage);

            lobby.Messages.Add(message);

            return Clients.Group(lobbyId.ToString()).SendAsync("ReceiveMessage",message );
        }
        
        return Task.CompletedTask;
    }

    public async Task<Lobby> CreateLobby(string lobbyName)
    {
        var user = connectedUsers.First(x => x.ConnectionId == Context.ConnectionId);

        var lobby = new Lobby(lobbyName, user);

        lobbies.Add(lobby);

        await Groups.AddToGroupAsync(Context.ConnectionId,lobby.Id.ToString());

        await Clients.All.SendAsync("NewLobbyCreated", lobby);

        return lobby;
    }

    public async Task JoinLobby(Guid lobbyId)
    {
        var user = connectedUsers.First(x => x.ConnectionId == Context.ConnectionId);

        if (user is null)
            return;

        var lobby = lobbies.FirstOrDefault(l => l.Id == lobbyId);

        if (lobby is null)
            return;

        lobby.Users.Add(user);

        await Groups.AddToGroupAsync(Context.ConnectionId,lobbyId.ToString());

        await Clients.Group(lobbyId.ToString()).SendAsync("UserJoinedLobby",lobby, user);

        await Clients.Others.SendAsync("UserJoinedLobbyNotification",lobbyId, user);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString();

            var user = new User(Context.ConnectionId,userName);

            connectedUsers.Add(user);

            await Clients.Others.SendAsync("UserConnected", user);

            await Clients.Caller.SendAsync("GetInitialValues",new InitialValuesDto(connectedUsers,lobbies));

            // await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString();

        var user = connectedUsers.First(x => x.ConnectionId == Context.ConnectionId);

       await Clients.Others.SendAsync("UserDisconnected", user);

       var userLobbies = lobbies.Where(x => x.Users.Any(x => x.ConnectionId == Context.ConnectionId)).ToList();

        if(userLobbies.Any()){
            userLobbies.ForEach(async lobby => {
                lobby.Users.Remove(connectedUsers.First(x => x.ConnectionId == Context.ConnectionId));

                if(lobby.Users.Count == 0)
                    lobbies.Remove(lobby);

                    await Clients.Group(lobby.Id.ToString()).SendAsync("UserLeftLobby", user);

                // if(lobby.Creator.userName == userName){
                //     lobbies.Remove(lobby);
                // }
            });
            
        }

        if(user is not null)
         connectedUsers.Remove(user);

        await base.OnDisconnectedAsync(exception);
    }
}
