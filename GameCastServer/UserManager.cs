using System.Collections.Concurrent;
using GameCast.Server.Models;

namespace GameCast.Server;

public class UserManager
{
    private readonly ConcurrentDictionary<string, string> _sockets = new();

    private readonly ConcurrentDictionary<string, User> _users = new();

    public User? GetUserById(string id)
    {
        return _users.GetValueOrDefault(id);
    }

    public void AddUser(User user)
    {
        _users[user.Identifier] = user;
    }

    public void RemoveUser(User user)
    {
        RemoveUserById(user.Identifier);
    }

    public void RemoveUserById(string id)
    {
        _users.TryRemove(id, out _);
        _sockets.TryRemove(id, out _);
    }

    public string? GetSocketId(string userId)
    {
        return _sockets.GetValueOrDefault(userId);
    }

    public User? GetUserBySocketId(string socketId)
    {
        string? userId = GetUserIdBySocketId(socketId);
        return string.IsNullOrEmpty(userId) ? null : GetUserById(userId);
    }

    public string? GetUserIdBySocketId(string socketId)
    {
        return _sockets.FirstOrDefault(s => s.Value == socketId).Key;
    }

    public void SetSocketId(User user, string? socketId)
    {
        SetSocketId(user.Identifier, socketId);
    }

    public void SetSocketId(string userId, string? socketId)
    {
        if (string.IsNullOrEmpty(socketId))
            _sockets.TryRemove(userId, out _);
        else
            _sockets[userId] = socketId;
    }
}