using System.Text.Json.Serialization;

namespace GameCast.Core.Models.Response;

public class RoomDataResponse
{
    [JsonPropertyName("code")] public string Code { get; private set; }

    [JsonPropertyName("appId")] public string ApplicationId { get; private set; }
    [JsonPropertyName("appTag")] public string ApplicationTag { get; private set; }
    [JsonPropertyName("minPlayers")] public int MinPlayers { get; private set; }
    [JsonPropertyName("maxPlayers")] public int MaxPlayers { get; private set; }

    [JsonPropertyName("server")] public string Server { get; private set; }

    [JsonPropertyName("full")] public bool IsFull { get; private set; }

    public RoomDataResponse(string roomCode, string appId, string appTag, int minPlayers, int maxPlayers,
        string server, bool isFull)
    {
        Code = roomCode;
        ApplicationId = appId;
        ApplicationTag = appTag;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        Server = server;
        IsFull = isFull;
    }
    
}