using System.Collections.Generic;

namespace StudentRegistry.Application.Constants
{
    // Central place for role names and their post-login landing page. Only the Viewer role has a
    // real destination today (the Student Records Review page) — Editor/Admin land on placeholder
    // pages until their real sections are built, without any change needed here or in AuthService.
    public static class AuthConstants
    {
        public const string RoleViewer = "Viewer";
        public const string RoleEditor = "Editor";
        public const string RoleAdmin = "Admin";

        public static readonly IReadOnlyDictionary<string, string> RoleRedirects = new Dictionary<string, string>
        {
            [RoleViewer] = "/admin/student-records-review",
            [RoleEditor] = "/editor",
            [RoleAdmin] = "/admin"
        };

        public const string DefaultRedirect = "/login";

        public static string GetRedirectUrl(string role) =>
            RoleRedirects.TryGetValue(role, out var url) ? url : DefaultRedirect;
    }
}
