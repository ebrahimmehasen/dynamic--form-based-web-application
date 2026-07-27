using System;

namespace StudentRegistry.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // The seeded root admin (username "Mohamed") only. Its username/password/role can never be
        // edited or deleted through the Admin UI — enforced in UserManagementService, not just the
        // client — regardless of who is logged in, including itself.
        public bool IsProtected { get; set; } = false;
    }
}
