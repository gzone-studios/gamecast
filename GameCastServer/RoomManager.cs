using System.Collections.Concurrent;
using GameCast.Core.Models;
using GameCast.Server.Models;

namespace GameCast.Server;

public class RoomManager
{
    private static readonly string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int NUM_CHARACTERS = 4;

    private static readonly string[] BAD_ROOMS =
    {
        "FUCK", "SHIT", "CUNT", "TWAT", "PISS", "DICK", "COCK", "TITS", "DAMN", "HELL",
        "CRAP", "SLUT", "JERK", "BUTT", "ARSE", "PRAT", "TURD", "MUFF", "SCUM", "SUCK"
    };

    private static readonly Random _random = new();

    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public bool RoomExists(string code)
    {
        return _rooms.ContainsKey(code);
    }

    public Room? GetRoom(string code)
    {
        return _rooms.GetValueOrDefault(code);
    }

    public List<Room> GetRoomsByHostId(User user)
    {
        return GetRoomsByHostId(user.Identifier);
    }

    public List<Room> GetRoomsByHostId(string userId)
    {
        return _rooms.Where(r => r.Value.IsHost(userId)).Select(r => r.Value).ToList();
    }


    public List<Room> GetRoomsByMemberId(User user)
    {
        return GetRoomsByMemberId(user.Identifier);
    }

    public List<Room> GetRoomsByMemberId(string userId)
    {
        return _rooms.Where(r => r.Value.IsMember(userId)).Select(r => r.Value).ToList();
    }

    public string CreateRoom(Application app)
    {
        string roomCode = GenerateUniqueRoomCode();

        Room room = new(roomCode, app, "localhost");
        _rooms[roomCode] = room;

        return roomCode;
    }

    public void RemoveRoom(string code)
    {
        _rooms.TryRemove(code, out _);
    }

    public string GenerateUniqueRoomCode()
    {
        string code;
        do
        {
            code = GenerateRoomCode();
        } while (_rooms.ContainsKey(code));

        return code;
    }

    public string GenerateRoomCode()
    {
        string roomCode;
        do
        {
            roomCode = new string(Enumerable.Range(0, NUM_CHARACTERS)
                .Select(_ => CHARACTERS[_random.Next(CHARACTERS.Length)]).ToArray());
        } while (BAD_ROOMS.Contains(roomCode));

        return roomCode;
    }
}