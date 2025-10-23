using System.Threading.Tasks;
using CourseClient.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CourseClient.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string email, string password);
        Task LogoutAsync(int userId);
    }

    public sealed class LoginResult
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public sealed class AuthService : IAuthService
    {
        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            using var db = new AppDbContext();

            
            var user = await db.Users
                .Include(u => u.LevelAccess)
                .SingleOrDefaultAsync(u => u.user_email == email);

            if (user == null)
            {
                return new LoginResult { Success = false };
            }

            
            string hashedPassword = HashPassword(password);
            if (user.password != hashedPassword)
            {
                return new LoginResult { Success = false };
            }

            return new LoginResult
            {
                Success = true,
                UserId = user.UserId,
                RoleId = user.level_access_id,
                RoleName = user.LevelAccess?.Title ?? string.Empty
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

        public Task LogoutAsync(int userId)
        {
            return Task.CompletedTask;
        }
    }
}
