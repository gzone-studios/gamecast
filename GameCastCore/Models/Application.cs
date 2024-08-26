namespace GameCast.Core.Models;

public class Application
{
    public string Identifier { get; private set; }
    public string Tag { get; private set; }
    public int MinPlayers { get; private set; }
    public int MaxPlayers { get; private set; }

    public Application(string id, string tag, int minPlayers, int maxPlayers)
    {
        Identifier = id;
        Tag = tag;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
    }
    
}