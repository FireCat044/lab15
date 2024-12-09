using ASP15.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public class DatabaseMonitorService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer _timer;
    private int _lastRecordId = 0;

    public DatabaseMonitorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckDatabase, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void CheckDatabase(object state)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Отримуємо нові записи
        var newRecords = dbContext.Records
            .Where(r => r.Id > _lastRecordId)
            .ToList();

        foreach (var record in newRecords)
        {
            _lastRecordId = record.Id;
            SendEmail(record);
        }
    }

    private void SendEmail(Record record)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("andreigorbunov044@gmail.com", "wdzkjarphowpprsp"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("andreigorbunov044@gmail.com"),
                Subject = "New Record Added",
                Body = $"A new record was added: {record.Name} at {record.CreatedAt}.",
                IsBodyHtml = true,
            };
            mailMessage.To.Add("andreigorbunov077@gmail.com");

            smtpClient.Send(mailMessage);

            Console.WriteLine($"Email sent for record: {record.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
