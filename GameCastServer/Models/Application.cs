namespace GameCast.Server.Models;

public class Application(string id, string tag, int minPlayers, int maxPlayers)
{
    public string Identifier { get; private set; } = id;
    public string Tag { get; private set; } = tag;
    public int MinPlayers { get; private set; } = minPlayers;
    public int MaxPlayers { get; private set; } = maxPlayers;
}