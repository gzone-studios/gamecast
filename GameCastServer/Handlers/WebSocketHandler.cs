using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameCast.Core.Converters;
using GameCast.Core.Models;

namespace GameCast.Server.Handlers;

public abstract class WebSocketHandler(ConnectionManager connectionManager)
{
    private static JsonSerializerOptions JsonSerializerOptions => new() { Converters = { new MessageJsonConverter() } };

    protected virtual Task<string> OnConnected(WebSocket socket)
    {
        return Task.FromResult(connectionManager.AddSocket(socket));
    }

    protected virtual async Task OnDisconnected(WebSocket socket)
    {
        string socketId = connectionManager.GetId(socket);
        await connectionManager.RemoveSocket(socket);
    }

    public async Task SendMessageAsync(string socketId, IMessage message)
    {
        await SendMessageAsync(connectionManager.GetSocketById(socketId), message);
    }

    protected async Task SendMessageAsync(WebSocket socket, IMessage message)
    {
        if (socket.State != WebSocketState.Open)
            return;
        string jsonMessage = JsonSerializer.Serialize(message, JsonSerializerOptions);
        ArraySegment<byte> messageBytes = new(Encoding.UTF8.GetBytes(jsonMessage), 0, jsonMessage.Length);
        await socket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task SendMessageToAllAsync(IMessage message)
    {
        foreach (KeyValuePair<string, WebSocket> pair in connectionManager.GetAllSockets())
            if (pair.Value.State == WebSocketState.Open)
                await SendMessageAsync(pair.Value, message);
    }

    public Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
    {
        return result.MessageType switch
        {
            WebSocketMessageType.Text => ReceiveTextAsync(socket, result, buffer),
            WebSocketMessageType.Binary => ReceiveBinaryAsync(socket, result, buffer),
            WebSocketMessageType.Close => OnDisconnected(socket),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task ReceiveTextAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
    {
        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        IMessage? message = JsonSerializer.Deserialize<IMessage>(jsonMessage, JsonSerializerOptions);
        if (message == null) return;

        await OnReceiveAsync(socket, message);
    }

    public async Task ReceiveBinaryAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
    {
        throw new NotImplementedException();
    }

    public abstract Task OnReceiveAsync(WebSocket socket, IMessage message);
}