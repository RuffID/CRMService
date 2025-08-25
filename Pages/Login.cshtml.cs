using CRMService.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
{
    public class LoginModel(SignInHandler indexHandler) : PageModel
    {
        [BindProperty]
        public UserPage? User { get; set; }

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (!await indexHandler.SignIn(User, ModelState))
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
