using GameCast.Core.Models;
using GameCast.Server.Models;

namespace GameCast.Server;

public class GameManager(UserManager userManager, RoomManager roomManager)
{
    public bool DoesRoomExist(string roomCode)
    {
        Room? room = roomManager.GetRoom(roomCode);
        return room != null;
    }

    public bool CanJoinRoom(string roomCode, string userId, Role role)
    {
        Room? room = roomManager.GetRoom(roomCode);
        if (room == null) return false;
        return CanJoinRoom(room, userId, role);
    }

    public bool CanJoinRoom(Room room, string userId, Role role)
    {
        return role switch
        {
            Role.Host => !room.HasHost || room.IsHost(userId),
            Role.Player => !room.IsFull || room.IsMember(userId),
            _ => false
        };
    }

    public Room? GetRoom(string roomCode)
    {
        return roomManager.GetRoom(roomCode);
    }

    public List<Room> GetAllRoomsByMemberId(string userId)
    {
        return roomManager.GetRoomsByMemberId(userId);
    }

    public bool JoinRoom(string roomCode, string userId, Role role)
    {
        Room? room = roomManager.GetRoom(roomCode);
        if (room == null) return false;
        return JoinRoom(room, userId, role);
    }

    public bool JoinRoom(Room room, string userId, Role role)
    {
        if (!CanJoinRoom(room, userId, role)) return false;
        switch (role)
        {
            case Role.Host:
                room.SetHost(userId);
                return true;
            case Role.Player:
                room.AddMember(userId);
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    public void LeaveRooms(User user)
    {
        LeaveRooms(user.Identifier);
    }

    public void LeaveRooms(string userId)
    {
        foreach (Room room in roomManager.GetRoomsByHostId(userId))
            room.SetHost(null);
        foreach (Room room in roomManager.GetRoomsByMemberId(userId))
            room.RemoveMember(userId);
    }
}