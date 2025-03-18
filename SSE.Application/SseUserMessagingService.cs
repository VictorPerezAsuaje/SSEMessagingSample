using System.Collections.Concurrent;
using System.Text.Json;

namespace SSE.Application;

public interface ISseUserMessagingService
{
    ConnectionStatus? GetUserStatus(string userId);
    Task SendHeartbeat(string userId);
    Task AddUserConnection(string userId, StreamWriter stream);
    void RemoveUserConnection(string userId);
    Task SendMessageToUserAsync<T>(string userId, T content);
}

public enum ConnectionStatus { Connected, Disconnected }
public class SseContext
{
    public StreamWriter Writer { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }
}

public class Message
{
    public object Content { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class SseUserMessagingService : ISseUserMessagingService
{
    private static readonly ConcurrentDictionary<string, SseContext> _userConnections = new();

    public ConnectionStatus? GetUserStatus(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var context)) return context.ConnectionStatus;

        return null;
    }

    public async Task SendMessageToUserAsync<T>(string userId, T content)
    {
        if (!_userConnections.TryGetValue(userId, out var context)) return;

        if (context.ConnectionStatus == ConnectionStatus.Disconnected) return;

        // Format required for SSE 'data: whatever' 
        string json = typeof(T) == typeof(string) ? Convert.ToString(content) : JsonSerializer.Serialize(content);
        await context.Writer.WriteLineAsync($"data: {json}\n");
        await context.Writer.FlushAsync();
    }

    public async Task SendHeartbeat(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var context))
        {
            await context.Writer.WriteLineAsync($"data: <3: Keep alive\n");
            await context.Writer.FlushAsync();
        }
    }

    public async Task AddUserConnection(string userId, StreamWriter stream)
    {
        if (_userConnections.TryGetValue(userId, out SseContext ctx))
        {
            ctx.ConnectionStatus = ConnectionStatus.Connected;
            ctx.Writer = stream;
            return;
        }

        _userConnections[userId] = new SseContext
        {
            ConnectionStatus = ConnectionStatus.Connected,
            Writer = stream
        };
    }

    public void RemoveUserConnection(string userId)
    {
        if (!_userConnections.TryGetValue(userId, out _)) return;

        _userConnections[userId].ConnectionStatus = ConnectionStatus.Disconnected;
    }
}
