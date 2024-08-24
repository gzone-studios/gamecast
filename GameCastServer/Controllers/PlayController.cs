using System.Net.WebSockets;
using GameCast.Server.Handlers;
using GameCast.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameCast.Server.Controllers;

[ApiController]
public class PlayController(GameHandler gameHandler) : ControllerBase
{
    // WS /api/rooms/{code}/play
    [Route("api/rooms/{code}/play")]
    public async Task<IActionResult> Websocket(string code)
    {
        // only allow websockets
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();

        // validate connection data (role, name, userid)
        string? role = HttpContext.Request.Query["role"];
        string? userId = HttpContext.Request.Query["user-id"];
        string? userName = HttpContext.Request.Query["name"];
        User? user = gameHandler.ValidateConnection(code, role, userId, userName);
        if (user == null)
            return BadRequest();

        // accept websocket
        WebSocket socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await gameHandler.OnConnected(socket, code, user.Identifier, role!);

        // listen for websocket
        await Receive(socket, (result, buffer) => _ = gameHandler.ReceiveAsync(socket, result, buffer));

        return Ok();
    }

    private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    {
        byte[] buffer = new byte[1024 * 4];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result =
                    await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}