using Microsoft.AspNetCore.Mvc;
using SSE.Application;
using System.Text;

namespace SSEMessaging.Controllers;

[ApiController]
[Route("sse")]
public class SSEController : ControllerBase
{
    private readonly ISseUserMessagingService _sse;

    public SSEController(ISseUserMessagingService messagingService)
    {
        _sse = messagingService;
    }

    [HttpGet("connect/{userId}")]
    public async Task Connect(string userId)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        var stream = new StreamWriter(Response.Body, Encoding.ASCII);

        await _sse.AddUserConnection(userId, stream);
        await _sse.SendMessageToUserAsync(userId, $"User '{userId}' connected");

        ConnectionStatus? status = _sse.GetUserStatus(userId);
        while (!HttpContext.RequestAborted.IsCancellationRequested && 
            status != null && 
            status != ConnectionStatus.Disconnected)
        {
            await _sse.SendHeartbeat(userId);
            await Task.Delay(5000); 
            status = _sse.GetUserStatus(userId); // Renew status in case it requested to disconnect
        }

        _sse.RemoveUserConnection(userId);
    }

    [HttpGet("disconnect/{userId}")]
    public IActionResult Disconnect(string userId)
    {
        _sse.RemoveUserConnection(userId);
        return Ok();
    }


    [HttpGet("task/{userId}")]
    public async Task<IActionResult> LongTask(string userId)
    {
        LongRunningTest test = new LongRunningTest(_sse);
        await test.TakeLongTime(userId);

        return Ok(new
        {
            Message = "Task completed",
            Data = $"User '{userId}' completed the task at: {DateTime.Now}."
        });
    }

    [HttpGet("task-with-obj/{userId}")]
    public async Task<IActionResult> WaitAndSendObject(string userId)
    {
        LongRunningTest test = new LongRunningTest(_sse);
        await test.WaitAndSendObject(userId, new
        {
            Name = "Victor",
            Age = 30,
            Country = "Spain",
            IsActive = true
        });

        return Ok(new
        {
            Message = "Task completed",
            Data = $"User '{userId}' completed the task at: {DateTime.Now}."
        });
    }
}
