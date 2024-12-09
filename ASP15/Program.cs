using ASP15.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using Quartz.Spi;

namespace ASP15
{
    public class Program
    {
        [Obsolete]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR();

            builder.Services.AddHostedService<NotificationService>();

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpClient();

            builder.Services.AddHostedService<CurrencyExchangeService>();

            builder.Services.AddHostedService<WebPageMonitorService>();

            builder.Services.AddScoped<RecordService>();

            builder.Services.AddHostedService<DatabaseMonitorService>();


            builder.Services.AddQuartz(q =>
            {
                // Створення запланованого завдання
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Додаємо завдання до Quartz
                var jobKey = new JobKey("BackupJob");
                q.AddJob<BackupJob>(opts => opts.WithIdentity(jobKey));

                // Розклад: виконувати кожні 10 хвилин
                q.AddTrigger(opts => opts
                    .ForJob(jobKey) // Прив'язка до завдання
                    .WithIdentity("BackupJobTrigger") // Ідентифікатор тригера
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(10)
                        .RepeatForever()));
            });

            // Додаємо Quartz хостинг
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated(); // Автоматичне створення бази
            }

            app.MapGet("/currency-rates", (IMemoryCache memoryCache) =>
            {
                if (memoryCache.TryGetValue("currencyRates", out string cachedRates))
                {
                    return Results.Ok(cachedRates);
                }
                return Results.NotFound("Currency rates not cached.");
            });

            app.MapHub<NotificationHub>("/notifications");



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Record}/{action=Add}/{id?}");

            app.Run();
        }
    }
}
