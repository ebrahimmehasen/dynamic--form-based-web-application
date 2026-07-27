using System;

namespace StudentRegistry.Application.DTOs
{
    // Never carries PasswordHash — the admin list/edit views must never expose or imply any
    // password material, hashed or otherwise.
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsProtected { get; set; }
    }

    public class UserCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
