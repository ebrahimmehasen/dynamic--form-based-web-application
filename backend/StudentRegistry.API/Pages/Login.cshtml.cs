using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRegistry.Application.Constants;
using System.Security.Claims;

namespace StudentRegistry.API.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Already-authenticated users skip the login form entirely and land straight back on
            // their role's page — avoids a confusing "log in again" prompt on a live session.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role != null)
                {
                    return Redirect(AuthConstants.GetRedirectUrl(role));
                }
            }

            return Page();
        }
    }
}
