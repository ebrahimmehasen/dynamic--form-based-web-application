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
    }
}
