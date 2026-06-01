using Microsoft.AspNetCore.Mvc;

namespace CRMService.Web.Service.BackgroundServices
{
    public static class BackgroundUpdateResponseExtensions
    {
        public static IActionResult ToBackgroundUpdateResponse(this ControllerBase controller, bool started)
        {
            return started
                ? controller.Accepted(new { message = "Обновление запущено." })
                : controller.Conflict(new { message = "Обновление уже выполняется." });
        }
    }
}
