using System.Text.Json.Serialization;

namespace GameCast.Core.Models.Response;

public class RoomDataResponse
{
    [JsonPropertyName("code")] public string Code { get; set; }

    [JsonPropertyName("appId")] public string ApplicationId { get; set; }
    [JsonPropertyName("appTag")] public string ApplicationTag { get; set; }
    [JsonPropertyName("minPlayers")] public int MinPlayers { get; set; }
    [JsonPropertyName("maxPlayers")] public int MaxPlayers { get; set; }

    [JsonPropertyName("server")] public string Server { get; set; }

    [JsonPropertyName("full")] public bool IsFull { get; set; }
}