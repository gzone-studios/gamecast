namespace GameCast.Server.Models;

public class User(string id, string name)
{
    public string Identifier { get; set; } = id;

    public string Name { get; set; } = name;
}