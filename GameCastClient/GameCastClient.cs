using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using GameCast.Core.Models;
using GameCast.Core.Models.Request;
using GameCast.Core.Models.Response;

namespace GameCast.Client;



public class GameCastClient<TRoomState, TUserState, TUserMessage> where TRoomState : IRoomState
    where TUserState : IUserState
    where TUserMessage : IUserMessage
{
    
    // delegate definitions
    public delegate void RoomStateChangedDelegate(TRoomState? data);
    public delegate void UserStateChangedDelegate(string userId, TUserState? data);
    public delegate void MessageToHostDelegate(string senderId, TUserMessage message);
    
    // delegates
    private RoomStateChangedDelegate? _roomStateChanged;
    private UserStateChangedDelegate? _userStateChanged;
    private MessageToHostDelegate? _messageToHost;
    
    // clients
    private readonly HttpClient _http = new();
    private readonly WebsocketClient _ws = new();
    
    // server data
    private readonly string _server;
    private readonly int _port;
    private readonly bool _secure;
    
    // app data
    private readonly Application? _application;
    
    // user data
    private readonly string _userId;
    private string? _userName;
    
    // room data
    public bool HasCurrentRoom => string.IsNullOrEmpty(CurrentRoom);
    public string? CurrentRoom { get; private set; }
    public TRoomState? RoomState { get; private set; }
    public ConcurrentDictionary<string, TUserState> UserState { get; } = new();
    
    // schemes
    private string WsScheme => _secure ? "wss://" : "ws://";
    private string HttpScheme => _secure ? "https://" : "http://";
    
    // Constructors

    public GameCastClient(string server, int port, bool secure = false, string? userId = null, Application? application = null)
    {
        _server = server;
        _port = port;
        _secure = secure;
        _application = application;
        
        _userId = userId ?? Guid.NewGuid().ToString();
        
        _ws.OnReceiveAsync = _OnReceiveAsync;
    }
    
    // 
    
    /// <summary>
    /// Changes the username. This change will only be reflected when reconnection to a room
    /// </summary>
    /// <param name="userName"></param>
    public void SetUserName(string? userName)
    {
        _userName = userName;
    }
    
    public Task<RoomDataResponse?> CreateRoom(string? password = null)
    {
        // only create a room if application information is provided
        if(_application == null) return Task.FromResult<RoomDataResponse?>(null);

        // create room
        CreateRoomRequest request = new(_application.Identifier, _application.Tag, _application.MinPlayers,
            _application.MaxPlayers, password);
        return CreateRoom(request);
    }
    
    public async Task<RoomDataResponse?> CreateRoom(CreateRoomRequest request)
    {
        // create room reservation
        Uri uri = BuildUri(HttpScheme, _server, _port, $"/api/rooms");
        string requestBody = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _http.PostAsync(uri, content);
        if (!response.IsSuccessStatusCode) return null;
        
        // return room data
        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RoomDataResponse>(responseBody);
    }

    public async Task<RoomDataResponse?> GetRoom(string code)
    {
        // GET request
        Uri uri = BuildUri(HttpScheme, _server, _port, $"/api/rooms/{code}");
        HttpResponseMessage response = await _http.GetAsync(uri);
        if (!response.IsSuccessStatusCode) return null;
        
        // parse json
        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RoomDataResponse>(responseBody);
    }
    
    /// <summary>
    /// Join a room using the room code.
    /// This query the room server and connects directly to it.
    /// </summary>
    /// <param name="code">The room code you want to join</param>
    /// <param name="role"></param>
    public async Task JoinRoom(string code, Role role)
    {
        // only join room if not currently in a room
        if(CurrentRoom != null) return;
        
        // get room data
        RoomDataResponse? response = await GetRoom(code);
        if(response == null) return;
        
        // join room using room data response
        await JoinRoom(response, role);
    }

    /// <summary>
    /// Try to join a room.
    /// If the application is set, this method fails to join a room with another appTag.
    /// The RoomDataResponse should not be stale.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="role"></param>
    public async Task JoinRoom(RoomDataResponse data, Role role)
    {
        // only join room if not currently in a room
        if(CurrentRoom != null) return;
        
        // if app is set, only join if app matches room's app
        if(_application != null && data.ApplicationTag != _application.Tag ) return;
        
        // build room uri
        Dictionary<string, string> queryParams = new()
        {
            { "role", role.StringValue() },
            { "user-id", _userId }
        };
        if(!string.IsNullOrEmpty(_userName)) queryParams.Add("name", _userName);
        Uri uri = BuildUri(WsScheme, data.Server, _port, $"/api/rooms/{data.Code}/play", queryParams);
        
        // join room
        await _ws.ConnectAsync(uri);
        CurrentRoom = data.Code;
    }
    
    /// <summary>
    /// Leave the current room
    /// </summary>
    public async Task LeaveRoom()
    {
        // only leave room if currently in a room
        if(CurrentRoom == null) return;
        
        // leave room
        CurrentRoom = null;
        await _ws.DisconnectAsync();
    }
    
    /// <summary>
    /// Sets the current room state.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public Task SetRoomState(TRoomState? state)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        RoomState = state;
        _roomStateChanged?.Invoke(RoomState);
        return _ws.SendAsync(new SetRoomStateMessage<TRoomState>(CurrentRoom, state));
    }

    public void OnRoomStateChanged(RoomStateChangedDelegate? @delegate)
    {
        _roomStateChanged = @delegate;
    }
    
    public Task SetUserState(string userId, TUserState? state)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        _SetUserState(userId, state);
        _userStateChanged?.Invoke(userId, state);
        return _ws.SendAsync(new SetUserStateMessage<IUserState>(CurrentRoom, userId, state));
    }
    
    public void OnUserStateChanged(UserStateChangedDelegate? @delegate)
    {
        _userStateChanged = @delegate;
    }

    public Task SendMessageToHost(TUserMessage message)
    {
        if (CurrentRoom == null) return Task.CompletedTask;
        return _ws.SendAsync(new SendMessageToHostMessage<TUserMessage>(CurrentRoom, null, message));
    }
    
    public void OnMessageToHost(MessageToHostDelegate? @delegate)
    {
        _messageToHost = @delegate;
    }
    
    private void _SetUserState(string userId, TUserState? userState)
    {
        if (userState == null)
            UserState.TryRemove(userId, out _);
        else
            UserState[userId] = userState;
    }
    
    private void _OnReceiveAsync(IMessage message)
    {
        switch (message)
        {
            case SetRoomStateMessage<TRoomState> roomStateMessage:
                RoomState = roomStateMessage.Data;
                _roomStateChanged?.Invoke(RoomState);
                return;
            case SetUserStateMessage<TUserState> userStateMessage:
                _SetUserState(userStateMessage.UserId, userStateMessage.Data);
                _userStateChanged?.Invoke(userStateMessage.UserId, userStateMessage.Data);
                return;
            case SendMessageToHostMessage<TUserMessage> messageToHostMessage:
                _messageToHost?.Invoke(messageToHostMessage.SenderId!, messageToHostMessage.Data);
                return;
        }
    }
    
    public static Uri BuildUri(string scheme, string server, int port, string endpoint,
        Dictionary<string, string>? queryParams = null)
    {
        // create uri builder
        UriBuilder builder = new()
        {
            Scheme = scheme,
            Host = server,
            Port = port,
            Path = endpoint
        };
        
        // add query params
        if (queryParams != null && queryParams.Count > 0)
        {
            NameValueCollection query = new();
            foreach (KeyValuePair<string, string> param in queryParams)
            {
                query[param.Key] = param.Value;
            }
            builder.Query = query.ToString();
        }
        
        // return uri
        return builder.Uri;
    }
    
}