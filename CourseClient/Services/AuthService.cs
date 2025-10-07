using System.Threading.Tasks;
using CourseClient.Data;
using Microsoft.EntityFrameworkCore;

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
                .SingleOrDefaultAsync(u => u.user_email == email && u.password == password);

            if (user == null)
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

        public Task LogoutAsync(int userId)
        {
            return Task.CompletedTask;
        }
    }
}


