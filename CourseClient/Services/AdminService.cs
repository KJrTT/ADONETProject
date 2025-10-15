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

            var userExists = await db.Users.AnyAsync(u => u.UserId == userId);

            if (!userExists)
                return false;

            try
            {
                var result = await db.Users.Where(u => u.UserId == userId).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.level_access_id, newRoleId));
                return result > 0;
            }
            catch (Exception ex) {
                Console.WriteLine($"Error updating user role: {ex.Message}");
                return false;
            }

            
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
