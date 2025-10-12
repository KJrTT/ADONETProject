using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CourseClient.Services
{
    public interface IStudentService
    {
        Task<List<object>> GetMyCoursesAsync(int userId);
        Task<List<Course>> GetAvailableCoursesAsync(int userId);
        Task<List<object>> GetFreeCoursesAsync();
        Task<bool> EnrollToCourseAsync(int userId, int courseId);
        Task<bool> UnenrollFromCourseAsync(int userId, int courseId);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<bool> DeleteAccountAsync(int userId);
        Task<bool> ReplenishBalanceAsync(int userId, decimal amount);
        Task<decimal> GetBalanceAsync(int userId);
    }

    internal class StudentService : IStudentService
    {
        public async Task<List<object>> GetMyCoursesAsync(int userId)
        {
            using var db = new AppDbContext();

            var myCourses = await db.UserCourses.Where(uc => uc.UserId == userId).Select(uc => new
                {
                    uc.Course.CourseId,
                    uc.Course.name_course,
                    uc.Course.price,
                    uc.Course.data_start,
                    uc.Course.data_end
                }).ToListAsync();

            return myCourses.Cast<object>().ToList();
        }

        public async Task<List<Course>> GetAvailableCoursesAsync(int userId)
        {
            using var db = new AppDbContext();

            var subscribedCourses = await db.UserCourses.Where(uc => uc.UserId == userId).Select(uc => uc.CourseId).ToListAsync();
            var availableCourses = await db.Courses.Where(c => !subscribedCourses.Contains(c.CourseId)).ToListAsync();

            return availableCourses;
        }

        public async Task<List<object>> GetFreeCoursesAsync()
        {
            using var db = new AppDbContext();

            var freeCourses = await db.Courses.Where(c => c.price == 0).Select(c => new
                {
                    c.CourseId,
                    c.data_end,
                    c.data_start,
                    c.name_course
                }).ToListAsync();

            return freeCourses.Cast<object>().ToList();
        }

        public async Task<bool> EnrollToCourseAsync(int userId, int courseId)
        {
            using var db = new AppDbContext();

            var course = await db.Courses.Where(c => c.CourseId == courseId).Select(c => new { c.price, c.name_course }).FirstOrDefaultAsync();

            if (course == null)
            {
                return false;
            }

            var isAlreadyEnrolled = await db.UserCourses.Where(uc => uc.UserId == userId && uc.CourseId == courseId).AnyAsync();

            if (isAlreadyEnrolled)
            {
                return false;
            }

            if (course.price > 0)
            {
                var userBalance = await db.Users.Where(u => u.UserId == userId).Select(u => u.Wallet.balance).FirstOrDefaultAsync();

                if (userBalance < course.price)
                {
                    return false;
                }

                var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.User.UserId == userId);

                if (wallet != null)
                {
                    wallet.balance -= course.price;
                }
            }

            var userCourse = new UserCourse
            {
                UserId = userId,
                CourseId = courseId
            };

            db.UserCourses.Add(userCourse);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnenrollFromCourseAsync(int userId, int courseId)
        {
            using var db = new AppDbContext();

            var userCourse = await db.UserCourses.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (userCourse == null)
            {
                return false;
            }

            db.UserCourses.Remove(userCourse);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
            {
                return false;
            }

            string hashedPassword = HashPassword(newPassword);

            using var db = new AppDbContext();

            var updatedRows = await db.Users.Where(u => u.UserId == userId).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.password, hashedPassword));

            return updatedRows == 1;
        }

        public async Task<bool> DeleteAccountAsync(int userId)
        {
            using var db = new AppDbContext();

            var checkuser = await db.UserCourses.Where(uc => uc.UserId == userId).AnyAsync();

            if (checkuser == true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Вы не можете удалить пользователя, так как у вас есть активная подписка на курс(ы)");
                Console.ResetColor();
                return false;
            }
            var deletedRows = await db.Users.Where(u => u.UserId == userId).ExecuteDeleteAsync();

            return deletedRows == 1;
        }

        public async Task<bool> ReplenishBalanceAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            using var db = new AppDbContext();

            var walletId = await db.Users.Where(u => u.UserId == userId).Select(u => u.wallet_id).FirstOrDefaultAsync();

            var currentBalance = await db.Wallets.Where(w => w.WalletId == walletId).Select(w => w.balance).FirstOrDefaultAsync();

            var updatedRows = await db.Wallets.Where(w => w.WalletId == walletId).ExecuteUpdateAsync(setters => setters.SetProperty(w => w.balance, currentBalance + amount));

            return updatedRows == 1;
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            using var db = new AppDbContext();

            var walletId = await db.Users.Where(u => u.UserId == userId).Select(u => u.wallet_id).FirstOrDefaultAsync();

            var balance = await db.Wallets.Where(w => w.WalletId == walletId).Select(w => w.balance).FirstOrDefaultAsync();

            return balance;
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
}
