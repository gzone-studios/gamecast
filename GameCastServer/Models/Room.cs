namespace GameCast.Server.Models;

public class Room(string code, Application app, string server)
{
    /// <summary>
    ///     The unique Code of this Room.
    ///     A client can use this code to join the Room.
    /// </summary>
    public string Code { get; private set; } = code;

    /// <summary>
    ///     Metadata about the application of this room.
    /// </summary>
    public Application Application { get; } = app;

    /// <summary>
    ///     The server that hosts this room.
    ///     The client should connect directly to the server to join the game.
    /// </summary>
    public string Server { get; private set; } = server;

    /// <summary>
    ///     The id of the hosting user of this room.
    ///     The host is the only one who can change room or user data.
    /// </summary>
    public string? HostId { get; private set; }

    /// <summary>
    ///     All members of the room.
    ///     Members can but not must contain the owner of the room.
    ///     All changes in room or user data will be sent so all members.
    /// </summary>
    public List<string> Members { get; } = new();

    /// <summary>
    ///     The current state of the room.
    ///     This value can be set by the host to reflect all non member bound state of the room.
    ///     Examples include: Has the game started? Current Round? Current Time Remaining?
    /// </summary>
    public object? RoomState { get; }

    /// <summary>
    ///     The current state of the member.
    ///     This value can be set by the host to reflect all member bound state of the room.
    ///     Examples include: Current Score? Current Location? Current Lives?
    /// </summary>
    public Dictionary<string, object> MemberState { get; } = new();

    public int MemberCount => Members.Count;

    public bool IsFull => MemberCount >= Application.MaxPlayers;

    public bool HasHost => !string.IsNullOrEmpty(HostId);

    public bool IsHost(User user)
    {
        return IsHost(user.Identifier);
    }

    public bool IsHost(string userId)
    {
        return userId == HostId;
    }

    public void SetHost(string? hostId)
    {
        HostId = hostId;
    }

    public bool IsMember(User user)
    {
        return IsMember(user.Identifier);
    }

    public bool IsMember(string memberId)
    {
        return Members.Contains(memberId);
    }

    public void AddMember(string memberId)
    {
        Members.Add(memberId);
    }

    public bool RemoveMember(string memberId)
    {
        return Members.Remove(memberId);
    }

    public void SetRoomState(object? roomState)
    {
        roomState = roomState;
    }

    public object? GetMemberState(string memberId)
    {
        return MemberState.GetValueOrDefault(memberId);
    }

    public void SetMemberState(string memberId, object? memberState)
    {
        if (memberState == null)
            MemberState.Remove(memberId);
        else
            MemberState[memberId] = memberState;
    }
}