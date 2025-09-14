using CRMService.Models.PageModels;
using CRMService.Pages.PageHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
{
    public class LoginModel(SignInHandler indexHandler) : PageModel
    {
        [BindProperty]
        public UserPage UserPage { get; set; } = new();

        public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken ct)
        {
            if (!await indexHandler.SignIn(UserPage, ModelState, ct))
            {
                if (!ModelState.IsValid)
                    return Page();
                else
                    return Unauthorized();
            }

            return Redirect(returnUrl ?? "/");
        }
    }
}
