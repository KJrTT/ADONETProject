using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseClient.Services
{
    public interface IAdminService
    {
        Task<bool> UpdateUserRoleAsync(int userId, int newRole);
        Task<bool> DelCourse(int courseId);
    }

    public sealed class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRole)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return false;

            user.level_access_id = newRole; // Или user.LevelAccess, в зависимости от вашей модели
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DelCourse(int courseId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Удаляем связи пользователей с курсом
                var userCourses = await _context.UserCourses
                    .Where(uc => uc.CourseId == courseId)
                    .ToListAsync();

                if (userCourses.Any())
                {
                    _context.UserCourses.RemoveRange(userCourses);
                }

                // 2. Удаляем сам курс
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

                if (course == null)
                    return false;

                _context.Courses.Remove(course);

                // 3. Сохраняем изменения
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}