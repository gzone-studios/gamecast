using System.Text.Json.Serialization;

namespace GameCast.Core.Models.Request;

public struct CreateRoomRequest {
    [JsonPropertyName("appId")] public string AppId { get; set; }
    [JsonPropertyName("appTag")] public string AppTag { get; set; }
    [JsonPropertyName("minPlayers")] public int MinPlayers { get; set; }
    [JsonPropertyName("maxPlayers")] public int MaxPlayers { get; set; }
    [JsonPropertyName("password")] public string? Password { get; private set; }

    public CreateRoomRequest(string appId, string appTag, int minPlayers, int maxPlayers, string? password = null)
    {
        AppId = appId;
        AppTag = appTag;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        Password = password;
    }
}