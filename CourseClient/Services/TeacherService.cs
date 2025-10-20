using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseClient.Services
{
    public interface ITeacherService
    {
        Task<List<Course>> GetMyCoursesAsync(int teacherId);
        Task<List<User>> GetEnrolledUsersAsync(int teacherId, int courseId);
        Task<bool> DeleteOwnCourseAsync(int teacherId, int courseId);
        Task<(bool Success, string ErrorMessage)> CreateCourseAsync(int teacherId, string name, DateTime startDate, DateTime endDate, decimal price);
    }

    public sealed class TeacherService : ITeacherService
    {


        public async Task<List<Course>> GetMyCoursesAsync(int teacherId)
        {
            using var db = new AppDbContext();
            return await db.Courses
                .AsNoTracking()
                .Where(c => c.UserID == teacherId)
                .OrderBy(c => c.name_course)
                .ToListAsync();
        }

        public async Task<List<User>> GetEnrolledUsersAsync(int teacherId, int courseId)
        {
            using var db = new AppDbContext();
            var ownsCourse = await db.Courses
                .AsNoTracking()
                .AnyAsync(c => c.CourseId == courseId && c.UserID == teacherId);

            if (!ownsCourse)
                return new List<User>();

            return await db.UserCourses
                .AsNoTracking()
                .Where(uc => uc.CourseId == courseId)
                .Select(uc => uc.User)
                .OrderBy(u => u.last_name)
                .ThenBy(u => u.first_name)
                .ToListAsync();
        }

        public async Task<bool> DeleteOwnCourseAsync(int teacherId, int courseId)
        {
            using var db = new AppDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var course = await db.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserID == teacherId);

                if (course == null)
                    return false;

                var userCourses = await db.UserCourses
                    .Where(uc => uc.CourseId == courseId)
                    .ToListAsync();

                if (userCourses.Any())
                {
                    db.UserCourses.RemoveRange(userCourses);
                }

                db.Courses.Remove(course);

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

        public async Task<(bool Success, string ErrorMessage)> CreateCourseAsync(int teacherId, string name, DateTime startDate, DateTime endDate, decimal price)
        {
            // Валидация названия курса
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Название курса не может быть пустым");

            var trimmedName = name.Trim();
            if (trimmedName.Length < 3)
                return (false, "Название курса должно содержать минимум 3 символа");

            if (trimmedName.Length > 100)
                return (false, "Название курса не может превышать 100 символов");

            // Валидация дат
            var today = DateTime.Today;
            if (startDate < today)
                return (false, "Дата начала курса не может быть в прошлом");

            if (endDate < startDate)
                return (false, "Дата окончания курса не может быть раньше даты начала");

            var maxEndDate = startDate.AddYears(3);
            if (endDate > maxEndDate)
                return (false, "Курс не может длиться более 3 лет");

            // Валидация цены
            if (price < 0)
                return (false, "Цена курса не может быть отрицательной");

            if (price > 1000000)
                return (false, "Цена курса не может превышать 1,000,000");

            try
            {
                using var db = new AppDbContext();
                var course = new Course
                {
                    name_course = trimmedName,
                    data_start = startDate,
                    data_end = endDate,
                    price = price,
                    UserID = teacherId
                };

                await db.Courses.AddAsync(course);
                await db.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при сохранении курса: {ex.Message}");
            }
        }
    }
}


