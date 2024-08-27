using GameCast.Core.Models;

namespace GameCast.Server.Models;

public class RoomReservation(string code, Application app, string server, DateTime createdAt)
{
    
    /// <summary>
    ///     The unique Code of this reservation and the room that will be created from it.
    ///     A client can use this code to join the room.
    /// </summary>
    public string Code { get; private set; } = code;

    /// <summary>
    ///     Metadata about the application of this reservation.
    /// </summary>
    public Application Application { get; } = app;

    /// <summary>
    ///     The server that hosts this reservation.
    ///     The client should connect directly to the server to join the room.
    /// </summary>
    public string Server { get; private set; } = server;
    
    /// <summary>
    ///     The time when the reservation was created.
    ///     This is used to determine the ttl of this reservation.
    /// </summary>
    public DateTime CreatedAt { get; set; } = createdAt;
    
}