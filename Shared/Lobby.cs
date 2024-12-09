namespace Shared;

public record class Lobby
{
    public Guid Id { get; set; }
    public string Name { get; set;}

    public List<User> Users { get; set; }
    public List<Message> Messages { get; set; }

    public User Creator {get;set;}

    public DateTimeOffset CreatedOn { get; set; }

    public Lobby(string name, User user)
    {
        Id = Guid.CreateVersion7(); // ULID
        Name = name;
        Users = new List<User>(){user};
        Messages = new List<Message>();

        CreatedOn = DateTimeOffset.UtcNow;

        Creator = user;
    }

    public Lobby(Guid id, string name, List<User> users, List<Message> messages, User creator, DateTimeOffset createdOn)
    {
        Id = id;
        Name = name;
        Users = users;
        Messages = messages;
        Creator = creator;
        CreatedOn = createdOn;
    }

    public Lobby()
    {
        
    }
}
