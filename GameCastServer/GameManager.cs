using GameCast.Server.Models;

namespace GameCast.Server;

public class GameManager(UserManager userManager, RoomManager roomManager)
{
    public bool DoesRoomExist(string roomCode)
    {
        Room? room = roomManager.GetRoom(roomCode);
        return room != null;
    }

    public bool CanJoinRoom(string roomCode, string userId, string role)
    {
        Room? room = roomManager.GetRoom(roomCode);
        if (room == null) return false;
        return CanJoinRoom(room, userId, role);
    }

    public bool CanJoinRoom(Room room, string userId, string role)
    {
        return role switch
        {
            "host" => !room.HasHost || room.IsHost(userId),
            "player" => !room.IsFull || room.IsMember(userId),
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

    public bool JoinRoom(string roomCode, string userId, string role)
    {
        Room? room = roomManager.GetRoom(roomCode);
        if (room == null) return false;
        return JoinRoom(room, userId, role);
    }

    public bool JoinRoom(Room room, string userId, string role)
    {
        if (!CanJoinRoom(room, userId, role)) return false;
        switch (role)
        {
            case "host":
                room.SetHost(userId);
                return true;
            case "player":
                room.AddMember(userId);
                return true;
        }

        return false;
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