using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Models;
using CourseClient.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;



namespace CourseClient.Views
{
    internal class StudentView
    {
        private readonly int _userId;
        private readonly IAuthService _authService;

        public StudentView(IAuthService authService)
        {
            _authService = authService;
        }

        public StudentView(int userId, IAuthService authService)
        {
            _userId = userId;
            _authService = authService;
        }
        public async Task<bool> RunAsync(LoginResult login)
        {
            while (true)
            {
                Console.WriteLine("\n=== СТУДЕНТ ===");
                Console.WriteLine("1) Мои курсы");
                Console.WriteLine("2) Доступные курсы");
                Console.WriteLine("3) Бесплатные курсы");
                Console.WriteLine("4) Записаться на курс");
                Console.WriteLine("5) Отписаться от курса");
                Console.WriteLine("6) Обновить пароль");
                Console.WriteLine("7) Удалить аккаунт");
                Console.WriteLine("0) Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ShowMyCoursesAsync();
                        break;
                    case "2":
                        await ShowAvailableCoursesAsync();
                        break;
                    case "3":
                        await ShowFreeCoursesAsync();
                        break;
                    case "4":
                        Console.Write("Введите ID курса для записи: ");
                        if (int.TryParse(Console.ReadLine(), out var courseIdToEnroll))
                            await EnrollToCourseAsync(courseIdToEnroll);
                        break;
                    case "5":
                        Console.Write("Введите ID курса для отписки: ");
                        if (int.TryParse(Console.ReadLine(), out var courseIdToUnenroll))
                            await UnenrollFromCourseAsync(courseIdToUnenroll);
                        break;
                    case "6":
                        Console.Write("Новый пароль: ");
                        var newPassword = Console.ReadLine() ?? string.Empty;
                        var update = await UpdatePasswordAsync(newPassword);
                        break;
                    case "7":
                        Console.Write("Точно удалить аккаунт? (y/n): ");
                        if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "y")
                        {
                            var deleted = await DeleteAccountAsync();
                            if (deleted)
                            {
                                Console.WriteLine("Аккаунт удалён.");
                                return false;
                            }
                        }
                        break;
                    case "0":
                        Console.WriteLine("Выход из аккаунта...");
                        return false;
                    default:
                        Console.WriteLine("Неизвестная команда");
                        break;
                }
            }
        }

        private async Task ShowMyCoursesAsync()
        {
            using var db = new AppDbContext();

            var myCourse = await db.UserCourses.Where(uc => uc.UserId == _userId).Select(uc => new
            {
                uc.Course.CourseId,
                uc.Course.name_course,
                uc.Course.price,
                uc.Course.data_start,
                uc.Course.data_end

            }).ToListAsync();
           
            if (myCourse.Count == 0)
            {
                Console.WriteLine("У вас нет записей на курсы.");
                return;
            }



            Console.WriteLine("\nМои курсы:");
            foreach (var c in myCourse)
            {
                Console.WriteLine($"[{c.CourseId}] {c.name_course} | Цена: {c.price} | {c.data_start:d} - {c.data_end:d}");
            }
        }

        private Task ShowAvailableCoursesAsync()
        {
            // LINQ: курсы, на которые пользователь НЕ подписан.
            // Courses.Where(c => !UserCourses.Any(uc => uc.UserId == _userId && uc.CourseId == c.CourseId))
            return Task.CompletedTask;
        }

        private async Task ShowFreeCoursesAsync()
        {
            using var db = new AppDbContext();

            var freeCourse = await db.Courses.Where(c => c.price == 0).Select(c => new
            {
                c.CourseId,
                c.data_end,
                c.data_start,
                c.name_course

            }).ToListAsync();

            Console.WriteLine("Бесплатные курсы: ");

            foreach (var c in freeCourse)
            {
                Console.WriteLine($"[Название курса -> {c.name_course}] \n Дата начала -> {c.data_start} \n Окончание курса -> {c.data_end}");
            }

        }

        private Task EnrollToCourseAsync(int courseId)
        {
            // LINQ: проверить существование курса и отсутствие подписки, добавить UserCourse, SaveChangesAsync
            return Task.CompletedTask;
        }

        private Task UnenrollFromCourseAsync(int courseId)
        {
            // LINQ: найти UserCourse по (_userId, courseId), удалить, SaveChangesAsync
            return Task.CompletedTask;
        }

        private async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("Пустое значение");
                return false;
            }
            if (newPassword.Length < 8)
            {
                Console.WriteLine("Пароль должен быть больше 8 символов");
                return false;
            }

            
            string hashedPassword = HashPassword(newPassword);

            using var db = new AppDbContext();

            var updateps = await db.Users
                .Where(u => u.UserId == _userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.password, hashedPassword));

            if (updateps == 1)
            {
                Console.WriteLine("Пароль обновлён");
                return true;
            }
            else
            {
                Console.WriteLine("Ошибка обновления пароля");
                return false;
            }
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

        private async Task<bool> DeleteAccountAsync()
        {
            using var db = new AppDbContext();

            var delClient = await db.Users.Where(c => c.UserId == _userId).ExecuteDeleteAsync();

            if (delClient == 1)
            {
                return true;
            }

            return (false);
        }
    }
}

    
