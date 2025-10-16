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
        Task<bool> CreateCourseAsync(int teacherId, string name, DateTime startDate, DateTime endDate, decimal price);
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

        public async Task<bool> CreateCourseAsync(int teacherId, string name, DateTime startDate, DateTime endDate, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (endDate < startDate)
                return false;

            if (price < 0)
                return false;

            using var db = new AppDbContext();
            var course = new Course
            {
                name_course = name.Trim(),
                data_start = startDate,
                data_end = endDate,
                price = price,
                UserID = teacherId
            };

            await db.Courses.AddAsync(course);
            await db.SaveChangesAsync();
            return true;
        }
    }
}


