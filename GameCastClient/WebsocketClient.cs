using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameCast.Core.Converters;
using GameCast.Core.Models;

namespace GameCast.Client;

public class WebsocketClient
{
    public delegate void ReceiveAsyncDelegate(IMessage message);

    private readonly ClientWebSocket _client = new();

    private static JsonSerializerOptions JsonSerializerOptions => new() { Converters = { new MessageJsonConverter() } };

    public ReceiveAsyncDelegate? OnReceiveAsync { get; set; }

    public async Task ConnectAsync(Uri uri)
    {
        _client.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();

        // receive messages
        byte[] buffer = new byte[256];
        while (_client.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result =
                await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            await _ReceiveAsync(result, buffer);
        }
    }

    public Task DisconnectAsync()
    {
        return _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    public async Task SendAsync(IMessage message)
    {
        if (_client.State != WebSocketState.Open)
            return;
        string jsonMessage = JsonSerializer.Serialize(message, JsonSerializerOptions);
        ArraySegment<byte> messageBytes = new(Encoding.UTF8.GetBytes(jsonMessage), 0, jsonMessage.Length);
        await _client.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private Task _ReceiveAsync(WebSocketReceiveResult result, byte[] buffer)
    {
        return result.MessageType switch
        {
            WebSocketMessageType.Text => _ReceiveTextAsync(result, buffer),
            WebSocketMessageType.Binary => _ReceiveBinaryAsync(result, buffer),
            WebSocketMessageType.Close => DisconnectAsync(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Task _ReceiveTextAsync(WebSocketReceiveResult result, byte[] buffer)
    {
        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        IMessage? message = JsonSerializer.Deserialize<IMessage>(jsonMessage, JsonSerializerOptions);
        if (message == null) return Task.CompletedTask;

        OnReceiveAsync?.Invoke(message);
        return Task.CompletedTask;
    }

    private Task _ReceiveBinaryAsync(WebSocketReceiveResult result, byte[] buffer)
    {
        throw new NotImplementedException();
    }
}