using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;

public class BackupJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            // Імітація резервного копіювання даних
            string backupPath = "backup.txt";
            string dataToBackup = "BACKUP DATA";

            File.WriteAllText(backupPath, dataToBackup);

            Console.WriteLine($"{DateTime.Now}: Backup completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: Error during backup - {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
