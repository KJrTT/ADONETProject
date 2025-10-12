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
    }

    public sealed class TeacherService : ITeacherService
    {
        private readonly AppDbContext _context;

        public TeacherService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetMyCoursesAsync(int teacherId)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.user_id == teacherId)
                .OrderBy(c => c.name_course)
                .ToListAsync();
        }

        public async Task<List<User>> GetEnrolledUsersAsync(int teacherId, int courseId)
        {
            var ownsCourse = await _context.Courses
                .AsNoTracking()
                .AnyAsync(c => c.CourseId == courseId && c.user_id == teacherId);

            if (!ownsCourse)
                return new List<User>();

            return await _context.UserCourses
                .AsNoTracking()
                .Where(uc => uc.CourseId == courseId)
                .Select(uc => uc.User)
                .OrderBy(u => u.last_name)
                .ThenBy(u => u.first_name)
                .ToListAsync();
        }

        public async Task<bool> DeleteOwnCourseAsync(int teacherId, int courseId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.user_id == teacherId);

                if (course == null)
                    return false;

                var userCourses = await _context.UserCourses
                    .Where(uc => uc.CourseId == courseId)
                    .ToListAsync();

                if (userCourses.Any())
                {
                    _context.UserCourses.RemoveRange(userCourses);
                }

                _context.Courses.Remove(course);

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


