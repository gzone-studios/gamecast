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

    private static readonly Random Random = new();
    private static Timer? _timer;

    private readonly ConcurrentDictionary<string, RoomReservation> _reservations = new();
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    
    public bool ReservationExists(string code)
    {
        return _reservations.ContainsKey(code);
    }
    
    public bool RoomExists(string code)
    {
        return _rooms.ContainsKey(code);
    }

    public RoomReservation? GetReservation(string code)
    {
        return _reservations.GetValueOrDefault(code);
    }

    public RoomReservation? GetAndRemoveReservation(string code)
    {
        return _reservations.TryRemove(code, out RoomReservation? reservation) ? reservation : null;
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

    /// <summary>
    /// Create a new Room reservation.
    /// A reservation does not hold users or state.
    /// A reservation has a limited lifetime.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public RoomReservation CreateReservation(Application app)
    {
        string roomCode = GenerateUniqueRoomCode();
        
        RoomReservation reservation = new(roomCode, app, "localhost", DateTime.Now);
        _reservations[roomCode] = reservation;
        
        return reservation;
    }
    
    /// <summary>
    /// Create a room from a reservation.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Room? CreateRoomFromReservation(string code)
    {
        RoomReservation? reservation = GetAndRemoveReservation(code);
        if (reservation == null) return null;
        
        Room room = new(reservation.Code, reservation.Application, reservation.Server, DateTime.Now);
        _rooms[room.Code] = room;

        return room;
    }

    public void RemoveReservation(string code)
    {
        _reservations.TryRemove(code, out _);
    }
    
    public void RemoveRoom(string code)
    {
        _rooms.TryRemove(code, out _);
    }

    public void CloseRoom(string code)
    {
        RemoveRoom(code);
    }
    
    /// <summary>
    /// Purge all reservations that are older than the maxLifetimeInMinutes
    /// </summary>
    /// <param name="maxLifetimeInMinutes"></param>
    public void PurgeOldReservations(int maxLifetimeInMinutes = 1)
    {
        foreach ((string? key, RoomReservation? r) in _reservations)
        {
            TimeSpan duration = DateTime.Now - r.CreatedAt;
            if(duration.TotalMinutes > maxLifetimeInMinutes)
                RemoveReservation(key);
        }
    }
    
    /// <summary>
    /// Purge all rooms that are older than the maxLifetimeInMinutes
    /// </summary>
    /// <param name="maxLifetimeInMinutes"></param>
    public void PurgeOldRooms(int maxLifetimeInMinutes = 120)
    {
        foreach ((string? key, RoomReservation? r) in _reservations)
        {
            TimeSpan duration = DateTime.Now - r.CreatedAt;
            if(duration.TotalMinutes > maxLifetimeInMinutes)
                CloseRoom(key);
        }
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
                .Select(_ => CHARACTERS[Random.Next(CHARACTERS.Length)]).ToArray());
        } while (BAD_ROOMS.Contains(roomCode));

        return roomCode;
    }
}