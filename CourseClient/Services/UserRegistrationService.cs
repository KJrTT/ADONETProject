using System;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

            string hashedPassword = HashPassword(password);

            var wallet = new Wallet { balance = 0 };

            var defaultLevel = await db.LevelAccess
                .OrderBy(la => la.AccessId)
                .FirstAsync();

            var user = new User
            {
                first_name = firstName,
                last_name = lastName,
                user_email = email,
                password = hashedPassword, 
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

        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
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