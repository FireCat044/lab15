using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class WebPageMonitorService : IHostedService, IDisposable
{
    private readonly HttpClient _httpClient;
    private Timer _timer;
    private readonly string _url = "https://example.com"; 
    private readonly string _logFilePath = "webpage_status_log.txt"; 

    public WebPageMonitorService()
    {
        _httpClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(PerformCheck, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _httpClient.Dispose();
    }

    private async void PerformCheck(object state)
    {
        try
        {
            var response = await _httpClient.GetAsync(_url);
            string status = response.IsSuccessStatusCode ? "UP" : "DOWN";
            string logMessage = $"{DateTime.UtcNow}: {_url} is {status}";
            LogToFile(logMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"{DateTime.UtcNow}: Error checking {_url} - {ex.Message}";
            LogToFile(errorMessage);
        }
    }

    private void LogToFile(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                writer.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}
