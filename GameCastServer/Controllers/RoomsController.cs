using GameCast.Core.Models.Request;
using GameCast.Core.Models.Response;
using GameCast.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameCast.Server.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(RoomManager roomManager) : ControllerBase
{
    // POST /api/rooms
    [HttpPost]
    public IActionResult CreateRoom(CreateRoomRequest request)
    {
        // create room
        Application app = new(request.AppId, request.AppTag, request.MinPlayers, request.MaxPlayers);
        string roomCode = roomManager.CreateRoom(app);

        // return ok
        Room? room = roomManager.GetRoom(roomCode);
        return CreatedAtAction("GetRoomByCode", new { code = roomCode }, BuildResponse(room));
    }

    // GET /api/rooms/{code}
    [HttpGet("{code}")]
    public IActionResult GetRoomByCode(string code)
    {
        Room? room = roomManager.GetRoom(code);

        if (room == null)
            return NotFound();

        return Ok(BuildResponse(room));
    }

    private static RoomDataResponse? BuildResponse(Room? room)
    {
        if (room == null) return null;
        return new RoomDataResponse(
            room.Code,
            room.Application.Identifier,
            room.Application.Tag,
            room.Application.MinPlayers,
            room.Application.MaxPlayers,
            room.Server,
            room.IsFull
        );
    }
}