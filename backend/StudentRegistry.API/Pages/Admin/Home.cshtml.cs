using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRegistry.Application.Constants;

namespace StudentRegistry.API.Pages.Admin
{
    [Authorize(Roles = AuthConstants.RoleAdmin)]
    public class HomeModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
