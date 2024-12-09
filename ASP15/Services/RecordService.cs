using ASP15.Models;
using Microsoft.EntityFrameworkCore;

public class RecordService
{
    private readonly AppDbContext _dbContext;

    public RecordService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Record> AddRecordAsync(string name)
    {
        var record = new Record { Name = name };
        _dbContext.Records.Add(record);
        await _dbContext.SaveChangesAsync();
        return record;
    }

    public async Task<List<Record>> GetAllRecordsAsync()
    {
        return await _dbContext.Records.ToListAsync();
    }
}
