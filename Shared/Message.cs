namespace Shared;

public sealed record Message(Guid LobbyId,User User, string Content);
