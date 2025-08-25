using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CRMService.Pages
{
    public class ReportModel : PageModel
    {
        private readonly ILogger<ReportModel> _logger;

        public ReportModel(ILogger<ReportModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostDataAsync()
        {
            List<EmployeeStat> data = new List<EmployeeStat>
            {
                new EmployeeStat { EmployeeId = 1, FullName = "Иванов Иван Иванович", ResolvedCount = 32, CurrentCount = 4, LoggedMinutes = 845 },
                new EmployeeStat { EmployeeId = 2, FullName = "Петров Пётр Петрович", ResolvedCount = 21, CurrentCount = 6, LoggedMinutes = 410 },
                new EmployeeStat { EmployeeId = 3, FullName = "Сидорова Анна Сергеевна", ResolvedCount = 40, CurrentCount = 2, LoggedMinutes = 1090 },
                new EmployeeStat { EmployeeId = 4, FullName = "Кузнецов Максим Олегович", ResolvedCount = 15, CurrentCount = 9, LoggedMinutes = 270 }
            };
            await Task.CompletedTask;
            return new JsonResult(data);
        }        
    }

    // Описывает запись статистики сотрудника
        public sealed class EmployeeStat
        {
            // Хранит идентификатор сотрудника
            public int EmployeeId { get; set; }
            // Хранит ФИО сотрудника
            public string FullName { get; set; } = string.Empty;
            // Хранит количество решённых заявок
            public int ResolvedCount { get; set; }
            // Хранит количество текущих заявок
            public int CurrentCount { get; set; }
            // Хранит списанное время в минутах
            public int LoggedMinutes { get; set; }
        }
}
