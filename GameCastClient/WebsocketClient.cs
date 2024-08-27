using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameCast.Core.Converters;
using GameCast.Core.Models;

namespace GameCast.Client;

public class WebsocketClient
{
    public delegate void ReceiveMessageDelegate(IMessage message);

    private ClientWebSocket? _client;

    private static JsonSerializerOptions JsonSerializerOptions => new() { Converters = { new MessageJsonConverter() } };

    private ReceiveMessageDelegate? _ReceiveMessage { get; set; }

    public WebSocketState State => _client?.State ?? WebSocketState.None;
    
    public async Task ConnectAsync(Uri uri)
    {
        if(_client != null) return;
        _client = new ClientWebSocket();
        await _client.ConnectAsync(uri, CancellationToken.None);
        _Receive();
    }

    private async void _Receive() 
    {
        if(_client == null) return;
        byte[] buffer = new byte[256];
        while (_client.State == WebSocketState.Open)
        {
            Debug.Print("Receiving...");
            WebSocketReceiveResult result =
                await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            await _ReceiveAsync(result, buffer);
        }
    }
    
    public async Task DisconnectAsync()
    {
        if(_client is not { State: WebSocketState.Open }) return;
        Debug.Print("Disconnecting...");
        Debug.Print(State.ToString());
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        _client.Dispose();
        _client = null;
    }

    public async Task SendAsync(IMessage message)
    {
        if (_client is not { State: WebSocketState.Open }) return;
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

        _ReceiveMessage?.Invoke(message);
        return Task.CompletedTask;
    }

    private Task _ReceiveBinaryAsync(WebSocketReceiveResult result, byte[] buffer)
    {
        throw new NotImplementedException();
    }

    public void OnReceiveMessage(ReceiveMessageDelegate @delegate)
    {
        _ReceiveMessage = @delegate;
    }
    
}