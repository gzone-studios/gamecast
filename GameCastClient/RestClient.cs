using System.Text.Json;
using GameCast.Core.Models.Request;
using GameCast.Core.Models.Response;

namespace GameCast.Client;

public class RestClient
{

    public const string EndpointRooms = "/api/rooms";
    
    private string _baseUrl;
    private HttpClient _client = new();

    public RestClient(string baseUrl)
    {
        _baseUrl = baseUrl;
    }
    
    public async Task<RoomDataResponse?> CreateRoom(CreateRoomRequest request)
    {
        try
        {
            string responseBody = await _client.GetStringAsync(_baseUrl + EndpointRooms);
            return JsonSerializer.Deserialize<RoomDataResponse>(responseBody);
        }
        catch (HttpRequestException e) { return null; }
    }

    public async Task<RoomDataResponse?> GetRoom(string code)
    {
        try
        {
            string responseBody = await _client.GetStringAsync(_baseUrl + EndpointRooms + "/" + code);
            return JsonSerializer.Deserialize<RoomDataResponse>(responseBody);
        }
        catch (HttpRequestException e) { return null; }
    }

}