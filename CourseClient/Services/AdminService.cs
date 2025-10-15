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
        Task<bool> UpdateUserRoleAsync(int userId, int newRoleId);
        Task<bool> DelCourse(int courseId);
    }

    public sealed class AdminService : IAdminService
    {
        

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId) 
        {
            using var db = new AppDbContext();
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return false;

            user.level_access_id = newRoleId; // Или user.LevelAccess, в зависимости от вашей модели
            await db.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DelCourse(int courseId)
        {
            using var db = new AppDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 1. Удаляем связи пользователей с курсом
                var userCourses = await db.UserCourses
                    .Where(uc => uc.CourseId == courseId)
                    .ToListAsync();

                if (userCourses.Any())
                {
                    db.UserCourses.RemoveRange(userCourses);
                }

                // 2. Удаляем сам курс
                var course = await db.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

                if (course == null)
                    return false;

                db.Courses.Remove(course);

                // 3. Сохраняем изменения
                await db.SaveChangesAsync();
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
