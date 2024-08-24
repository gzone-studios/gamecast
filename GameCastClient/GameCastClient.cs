using GameCast.Core.Models;

namespace GameCast.Client;

public class GameCastClient<TRoomState, TUserState, TUserMessage> where TRoomState : IRoomState
    where TUserState : IUserState
    where TUserMessage : IUserMessage
{
    // Delegates
    public delegate void RoomCreatedEventHandler(string? code);
    public delegate void RoomDataChangedEventHandler(TRoomState? data);
    public delegate void RoomJoinedEventHandler(string? code);
    public delegate void RoomLeftEventHandler();
    public delegate void MessageToRoomOwnerEventHandlerEventHandler(string senderId, TUserMessage message);
    public delegate void UserDataChangedEventHandler(string userId, TUserState? data);

    private readonly RestClient _restClient;
    private readonly WebsocketClient _wsClient;
    
    public MessageToRoomOwnerEventHandlerEventHandler MessageToRoomOwnerEventHandler = (senderId, message) => { };
    public RoomCreatedEventHandler RoomCreated = code => { };
    public RoomDataChangedEventHandler RoomDataChanged = data => { };
    public RoomJoinedEventHandler RoomJoined = code => { };
    public RoomLeftEventHandler RoomLeft = () => { };
    public UserDataChangedEventHandler UserDataChanged = (userId, data) => { };

    public GameCastClient(string address)
    {
        Address = address;

        _restClient = new RestClient(address);
        
        _wsClient = new WebsocketClient();
        _wsClient.OnReceiveAsync = _OnReceiveAsync;
    }

    // Config
    public string Address { get; private set; }

    // Data
    public bool HasCurrentRoom { get; private set; }
    public string? CurrentRoom { get; private set; }
    public TRoomState? RoomState { get; private set; }
    public Dictionary<string, TUserState> UserData { get; } = new();

    public async Task CreateRoom(string code)
    {
        string uri = _CreateRoomUri(Address, code, "host", "NiklasZeroZero", Guid.NewGuid().ToString());
        await _wsClient.ConnectAsync(new Uri(uri));
        CurrentRoom = code;
        HasCurrentRoom = true;
        RoomCreated(CurrentRoom);
    }

    public async Task JoinRoom(string code)
    {
        string uri = _CreateRoomUri(Address, code, "player", "NiklasZeroZero", Guid.NewGuid().ToString());
        await _wsClient.ConnectAsync(new Uri(uri));
        CurrentRoom = code;
        HasCurrentRoom = true;
        RoomJoined(CurrentRoom);
    }

    private string _CreateRoomUri(string server, string code, string role, string name, string userId) 
        => $"{server}/api/room/{code}/play?role={role}&name={name}&user-id={userId}";
    
    public async Task LeaveRoom()
    {
        await _wsClient.DisconnectAsync();
        HasCurrentRoom = false;
        RoomLeft();
    }

    public Task SetRoomState(TRoomState? data)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        RoomState = data;
        RoomDataChanged(RoomState);
        return _wsClient.SendAsync(new SetRoomStateMessage<TRoomState>(CurrentRoom, data));
    }

    public Task SetUserData(string userId, TUserState? userData)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        _SetUserState(userId, userData);
        UserDataChanged(userId, userData);
        return _wsClient.SendAsync(new SetUserStateMessage<IUserState>(CurrentRoom, userId, userData));
    }

    public Task SendMessageToHost(TUserMessage message)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        return _wsClient.SendAsync(new SendMessageToHostMessage<TUserMessage>(CurrentRoom, null, message));
    }

    private void _OnReceiveAsync(IMessage message)
    {
        switch (message)
        {
            case SetRoomStateMessage<TRoomState> roomStateMessage:
                RoomState = roomStateMessage.Data;
                RoomDataChanged(RoomState);
                return;
            case SetUserStateMessage<TUserState> userStateMessage:
                _SetUserState(userStateMessage.UserId, userStateMessage.Data);
                UserDataChanged(userStateMessage.UserId, userStateMessage.Data);
                return;
            case SendMessageToHostMessage<TUserMessage> messageToHostMessage:
                MessageToRoomOwnerEventHandler(messageToHostMessage.SenderId!, messageToHostMessage.Data);
                return;
        }
    }

    private void _SetUserState(string userId, TUserState? userData)
    {
        // is userData is set to null, remove it from the dict
        if (userData == null)
        {
            UserData.Remove(userId);
            UserDataChanged(userId, default);
            return;
        }

        // update data
        UserData[userId] = userData;
    }
}