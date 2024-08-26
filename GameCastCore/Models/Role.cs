namespace GameCast.Core.Models;

public enum Role
{
    Host,
    Player,
}

public static class RoleExtensions
{

    public static string StringValue(this Role role)
    {
        return role switch
        {
            Role.Host => "host",
            Role.Player => "player",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    public static Role? TryParse(string? role)
    {
        return role switch
        {
            "host" => Role.Host,
            "player" => Role.Player,
            _ => null
        };
    }
    
}