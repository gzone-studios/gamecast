using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace GameCast.Server;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    public WebSocket GetSocketById(string id)
    {
        return _sockets.FirstOrDefault(p => p.Key == id).Value;
    }

    public ConcurrentDictionary<string, WebSocket> GetAllSockets()
    {
        return _sockets;
    }

    public string GetId(WebSocket socket)
    {
        return _sockets.FirstOrDefault(p => p.Value == socket).Key;
    }

    public string AddSocket(WebSocket socket)
    {
        string socketId = CreateConnectionId();
        _sockets.TryAdd(socketId, socket);
        return socketId;
    }

    public async Task RemoveSocket(WebSocket socket, string description = "Connection closed")
    {
        string id = GetId(socket);
        if (!string.IsNullOrEmpty(id)) _sockets.TryRemove(id, out _);

        if (socket.State != WebSocketState.Aborted)
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                description,
                CancellationToken.None);
    }

    private string CreateConnectionId()
    {
        return Guid.NewGuid().ToString();
    }
}