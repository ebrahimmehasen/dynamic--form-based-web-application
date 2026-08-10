namespace StudentRegistry.Application.DTOs
{
    // Username/password are collected as-is from the client and never trusted or compared
    // client-side. Real credential validation happens entirely server-side, in whatever
    // IAuthService implementation eventually replaces the current placeholder.
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? RedirectUrl { get; set; }
        // True only for a protected root admin (User.IsProtected) — drives the "IsProtected" claim,
        // which gates access to admin-only-of-admins features like "إعدادات العرض" (never keyed off
        // a specific hardcoded username, so any number of protected admins can be seeded).
        public bool IsProtected { get; set; }
    }
}
