using Microsoft.AspNetCore.Mvc;

public class RecordController : Controller
{
    private readonly RecordService _recordService;

    public RecordController(RecordService recordService)
    {
        _recordService = recordService;
    }

    // GET: /Record/Add
    public IActionResult Add()
    {
        return View();
    }

    // POST: /Record/Add
    [HttpPost]
    public async Task<IActionResult> Add(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            ViewBag.ErrorMessage = "Назва не може бути порожньою.";
            return View();
        }

        try
        {
            var record = await _recordService.AddRecordAsync(name);
            ViewBag.SuccessMessage = $"Запис успішно додано: {record.Name}";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"Сталася помилка: {ex.Message}";
            return View();
        }
    }
}
