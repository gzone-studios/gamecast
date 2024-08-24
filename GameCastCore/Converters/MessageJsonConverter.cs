using System.Text.Json;
using System.Text.Json.Serialization;
using GameCast.Core.Models;

namespace GameCast.Core.Converters;

public class MessageJsonConverter : JsonConverter<IMessage>
{
    public override IMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the JSON object
        JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);

        JsonElement root = jsonDocument.RootElement;

        // Extract the "EventName" property
        if (!root.TryGetProperty("event", out JsonElement eventNameElement))
            throw new JsonException("event property not found");

        string? eventName = eventNameElement.GetString();

        // Determine the type based on "EventName" and deserialize accordingly
        IMessage? message = eventName switch
        {
            "SetRoomState" => JsonSerializer.Deserialize<SetRoomStateMessage<IRoomState>>(root.GetRawText(), options),
            "SetUserState" => JsonSerializer.Deserialize<SetUserStateMessage<IUserState>>(root.GetRawText(), options),
            "SendMessageToHost" => JsonSerializer.Deserialize<SendMessageToHostMessage<IUserMessage>>(root.GetRawText(),
                options),
            _ => throw new NotSupportedException($"event '{eventName}' is not supported")
        };

        return message;
    }

    public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}