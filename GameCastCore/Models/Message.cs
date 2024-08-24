using System.Text.Json.Serialization;

namespace GameCast.Core.Models;

public interface IMessage
{
    public string EventName { get; }
    public string Room { get; }
}

public class SetRoomStateMessage<T> : IMessage where T : IRoomState
{
    [JsonPropertyName("event")] public string EventName => "SetRoomState";
    [JsonPropertyName("room")] public string Room { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }

    public SetRoomStateMessage(string room, T? data)
    {
        Room = room;
        Data = data;
    }
}

public class SetUserStateMessage<T> : IMessage where T : IUserState
{
    [JsonPropertyName("event")] public string EventName => "SetUserState";
    [JsonPropertyName("room")] public string Room { get; set; }
    [JsonPropertyName("user-id")] public string UserId { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }

    public SetUserStateMessage(string room, string userId, T? data)
    {
        Room = room;
        UserId = userId;
        Data = data;
    }
}

public class SendMessageToHostMessage<T> : IMessage where T : IUserMessage
{
    [JsonPropertyName("event")] public string EventName => "SendMessageToHost";
    [JsonPropertyName("room")] public string Room { get; set; }
    [JsonPropertyName("sender-id")] public string? SenderId { get; set; } 
    [JsonPropertyName("data")] public T Data { get; set; }

    public SendMessageToHostMessage(string room, string? senderId, T data)
    {
        Room = room;
        SenderId = senderId;
        Data = data;
    }
}