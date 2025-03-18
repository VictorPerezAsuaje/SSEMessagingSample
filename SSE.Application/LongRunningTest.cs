namespace SSE.Application;

public class LongRunningTest
{
    ISseUserMessagingService _sse;
    public LongRunningTest(ISseUserMessagingService sse)
    {
        _sse = sse;
    }

    public async Task TakeLongTime(string userId)
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(1000);

            await _sse.SendMessageToUserAsync(userId, $"User '{userId}', attempt {i + 1} / 10");
        }
    }

    public async Task WaitAndSendObject<T>(string userId, T obj)
    {
        for (int i = 5; i > 0; i--)
        {
            await Task.Delay(1000);
            await _sse.SendMessageToUserAsync(userId, $"Object sent to user '{userId}' in {i}...");
        }

        await _sse.SendMessageToUserAsync(userId, obj);
    }
}

