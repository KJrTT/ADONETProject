using System;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseClient.Services
{
    public interface IUserRegistrationService
    {
        Task<RegistrationResult> RegisterClientAsync(string firstName, string lastName, string email, string password);
    }

    public sealed class UserRegistrationService : IUserRegistrationService
    {
        public async Task<RegistrationResult> RegisterClientAsync(string firstName, string lastName, string email, string password)
        {
            using var db = new AppDbContext();

            var exists = await db.Users.AnyAsync(u => u.user_email == email);
            if (exists)
                throw new InvalidOperationException("Email already exists");

            var wallet = new Wallet { balance = 0 };

            var defaultLevel = await db.LevelAccess
                .OrderBy(la => la.AccessId)
                .FirstAsync();

            var user = new User
            {
                first_name = firstName,
                last_name = lastName,
                user_email = email,
                password = password,
                LevelAccess = defaultLevel
            };

            user.Wallet = wallet;
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new RegistrationResult
            {
                ClientId = user.UserId,
                WalletId = user.wallet_id,
                Status = "Registered",
                GeneratedPassword = password
            };
        }
    }

    public sealed class RegistrationResult
    {
        public int ClientId { get; set; }
        public int WalletId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string GeneratedPassword { get; set; } = string.Empty;
    }
}


