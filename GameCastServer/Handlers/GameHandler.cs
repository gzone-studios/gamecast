using System.Diagnostics;
using System.Net.WebSockets;
using GameCast.Core.Models;
using GameCast.Server.Models;

namespace GameCast.Server.Handlers;

public class GameHandler(ConnectionManager connectionManager, UserManager userManager, GameManager gameManager)
    : WebSocketHandler(connectionManager)
{
    
    private readonly ConnectionManager _connectionManager = connectionManager;

    public User? ValidateConnection(string? roomCode, string? userId, string? username, Role role)
    {
        // validate room code
        if (string.IsNullOrEmpty(roomCode))
            return null;

        // validate room
        if (!gameManager.DoesRoomExist(roomCode))
            return null;
        
        // validate user data
        if (string.IsNullOrEmpty(userId))
            return null;
        if (string.IsNullOrEmpty(username))
            return null;

        // add user
        User user = new(userId, username);

        // can user join room
        bool canJoinRoom = gameManager.CanJoinRoom(roomCode, user.Identifier, role);
        if (!canJoinRoom)
            return null;

        userManager.AddUser(user);
        return user;
    }

    public async Task<string> OnConnected(WebSocket socket, string roomCode, string userId, Role role)
    {
        // set socket id
        string socketId = await base.OnConnected(socket);
        userManager.SetSocketId(userId, socketId);

        // join room
        bool success = gameManager.JoinRoom(roomCode, userId, role);
        if (success) return socketId;

        // disconnect
        await OnDisconnected(socket);
        return socketId;
    }

    protected override Task OnDisconnected(WebSocket socket)
    {
        string socketId = _connectionManager.GetId(socket);
        User? user = userManager.GetUserBySocketId(socketId);

        if (user != null)
        {
            gameManager.LeaveRooms(user);
            userManager.RemoveUser(user);
        }

        return base.OnDisconnected(socket);
    }

    public override async Task OnReceiveAsync(WebSocket socket, IMessage message)
    {
        string socketId = _connectionManager.GetId(socket);
        if (string.IsNullOrEmpty(socketId)) return;

        User? sender = userManager.GetUserBySocketId(socketId);
        if (sender == null) return;

        Room? room = gameManager.GetRoom(message.Room);
        if (room == null) return;

        switch (message.EventName)
        {
            // SetRoomState - Can only be called by room host. Cached on server.
            case "SetRoomState":
                if (!room.IsHost(sender)) return;
                if (message is not SetRoomStateMessage<IRoomState> roomStateMessage) return;
                room.SetRoomState(roomStateMessage.Data);
                await SendMessageToRoomMembers(message);
                break;
            // SetUserState - Can only be called by room host. Cached on server.
            case "SetUserState":
                if (!room.IsHost(sender)) return;
                if (message is not SetUserStateMessage<IUserState> userStateMessage) return;
                room.SetMemberState(userStateMessage.UserId, userStateMessage.Data);
                await SendMessageToRoomMembers(message);
                break;
            // SendMessageToHost - Can only be called by members;
            case "SendMessageToHost":
                if (!room.IsMember(sender)) return;
                if (message is not SendMessageToHostMessage<IUserMessage> messageToHostMessage) return;
                SendMessageToHostMessage<IUserMessage> message2Host = new(messageToHostMessage.Room, sender.Identifier,
                    messageToHostMessage.Data);
                await SendMessageToRoomHost(message2Host);
                break;
        }
    }

    public async Task SendMessageToRoomMembers(IMessage message)
    {
        Debug.Print($"Sending message {message.EventName} to {message.Room}: {message}");
        Room? room = gameManager.GetRoom(message.Room);
        if (room == null) return;
        foreach (string member in room.Members)
        {
            string? socketId = userManager.GetSocketId(member);
            if (socketId == null) continue;
            WebSocket socket = _connectionManager.GetSocketById(socketId);
            await SendMessageAsync(socket, message);
        }
    }

    public async Task SendMessageToRoomHost(IMessage message)
    {
        Room? room = gameManager.GetRoom(message.Room);
        if (room?.HostId == null) return;
        string? socketId = userManager.GetSocketId(room.HostId);
        if (socketId == null) return;
        WebSocket socket = _connectionManager.GetSocketById(socketId);
        await SendMessageAsync(socket, message);
    }
}