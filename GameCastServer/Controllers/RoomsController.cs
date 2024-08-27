using GameCast.Core.Models;
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
    public IActionResult CreateRoomReservation(CreateRoomRequest request)
    {
        // create room
        Application app = new(request.AppId, request.AppTag, request.MinPlayers, request.MaxPlayers);
        RoomReservation reservation = roomManager.CreateReservation(app);

        // return ok
        return CreatedAtAction("GetRoomInfoByCode", new { code = reservation.Code }, BuildResponse(reservation));
    }

    // GET /api/rooms/{code}
    [HttpGet("{code}")]
    public IActionResult GetRoomInfoByCode(string code)
    {
        Room? room = roomManager.GetRoom(code);

        if (room == null)
            return NotFound();

        return Ok(BuildResponse(room));
    }

    private static RoomDataResponse? BuildResponse(Room? room)
    {
        if (room == null) return null;
        return new RoomDataResponse()
        {
            Code = room.Code,
            ApplicationId = room.Application.Identifier,
            ApplicationTag = room.Application.Tag,
            MinPlayers = room.Application.MinPlayers,
            MaxPlayers = room.Application.MaxPlayers,
            Server = room.Server,
            IsFull = room.IsFull
        };
    }
    
    private static RoomDataResponse? BuildResponse(RoomReservation? room)
    {
        if (room == null) return null;
        return new RoomDataResponse()
        {
            Code = room.Code,
            ApplicationId = room.Application.Identifier,
            ApplicationTag = room.Application.Tag,
            MinPlayers = room.Application.MinPlayers,
            MaxPlayers = room.Application.MaxPlayers,
            Server = room.Server,
            IsFull = false
        };
    }
}