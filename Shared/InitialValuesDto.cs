namespace Shared;

public sealed record class InitialValuesDto (List<User> ConnectedUsers, List<Lobby> Lobbies);
