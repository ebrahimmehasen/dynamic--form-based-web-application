using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRegistry.Application.Constants;

namespace StudentRegistry.API.Pages.Editor
{
    [Authorize(Roles = AuthConstants.RoleEditor)]
    public class StudentRecordsEditorModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
