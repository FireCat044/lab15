using Microsoft.AspNetCore.SignalR;


public class NotificationService : IHostedService, IDisposable
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;
    private Timer _timer;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification service started.");
        _timer = new Timer(SendNotifications, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    private async void SendNotifications(object state)
    {
        var message = $"Server time: {DateTime.Now}";
        _logger.LogInformation("Broadcasting message: {Message}", message);

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification service stopped.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
