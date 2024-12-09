using Microsoft.Extensions.Caching.Memory;


public class CurrencyExchangeService : IHostedService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CurrencyExchangeService> _logger;
    private Timer _timer;
    private readonly string _logFilePath = "CurrencyExchangeLog.txt"; 

    public CurrencyExchangeService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        ILogger<CurrencyExchangeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Встановлюємо таймер для виклику API 
        _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void ExecuteTask(object state)
    {
        _logger.LogInformation("Fetching currency exchange rates...");

        try
        {
            if (!_memoryCache.TryGetValue("currencyRates", out string cachedRates))
            {
                // Якщо кешу немає, робимо запит до зовнішнього API
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");

                // Зберігаємо результат в кеш
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10)); // 10 хвилин

                _memoryCache.Set("currencyRates", response, cacheEntryOptions);

                // Записуємо отримані дані у файл
                SaveToFile(response);

                _logger.LogInformation("Currency exchange rates fetched and cached.");
            }
            else
            {
                _logger.LogInformation("Using cached currency exchange rates.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
        }
    }

    private void SaveToFile(string data)
    {
        try
        {
            // Записуємо дані у файл з додаванням часу запису
            using (var writer = new StreamWriter(_logFilePath, true)) 
            {
                writer.WriteLine($"[{DateTime.Now}] {data}");
            }

            _logger.LogInformation("Currency exchange rates logged to file.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to write to log file: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
