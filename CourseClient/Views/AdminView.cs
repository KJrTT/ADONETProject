using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Services;
using Microsoft.EntityFrameworkCore;


namespace CourseClient.Views
{
    internal class AdminView
    {
        private readonly int _adminId;
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService; // Добавлена зависимость

        public AdminView(int adminId, IAuthService authService, IAdminService adminService)
        {
            _adminId = adminId;
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }

        public async Task<bool> RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n=== АДМИНИСТРАТОР ==="); // Исправлен заголовок
                Console.WriteLine("1) Изменить роль пользователю");
                Console.WriteLine("2) Удалить курс");
                Console.WriteLine("0) Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ChangeUserRoleAsync();
                        break;
                    case "2":
                        await DeleteCourseAsync();
                        break;
                    case "0":
                        Console.WriteLine("Выход из аккаунта...");
                        return false;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Неизвестная команда");
                        Console.ResetColor();
                        break;
                }
            }
        }
        private async Task ChangeUserRoleAsync()
        {
            using var context = new AppDbContext();

            // Простой вывод пользователей
            var users = await context.Users
                .Include(u => u.LevelAccess)
                .ToListAsync();

            Console.WriteLine("\nСписок пользователей:");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.UserId}, Имя: {user.first_name}, Email: {user.user_email}, Роль: {user.LevelAccess?.Title}");
            }

            Console.Write("\nВведите ID пользователя: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Некорректный ID пользователя");
                return;
            }

            // Простой вывод ролей
            var roles = await context.LevelAccess.ToListAsync();
            Console.WriteLine("\nСписок ролей:");
            foreach (var role in roles)
            {
                Console.WriteLine($"ID: {role.AccessId}, Роль: {role.Title}");
            }

            Console.Write("Введите новую роль (ID): ");
            if (!int.TryParse(Console.ReadLine(), out int newRoleId))
            {
                Console.WriteLine("Некорректный ID роли");
                return;
            }

            // Изменение роли
            var userToUpdate = await context.Users.FindAsync(userId);
            if (userToUpdate != null)
            {
                userToUpdate.level_access_id = newRoleId;
                await context.SaveChangesAsync();
                Console.WriteLine("Роль пользователя успешно изменена!");
            }
            else
            {
                Console.WriteLine("Пользователь не найден");
            }
        }

        private async Task DeleteCourseAsync()
        {
            using var context = new AppDbContext();

            // Простой вывод курсов
            var courses = await context.Courses.ToListAsync();
            Console.WriteLine("\nСписок курсов:");
            foreach (var course in courses)
            {
                Console.WriteLine($"ID: {course.CourseId}, Название: {course.name_course}, Преподаватель ID: {course.UserID}");
            }

            Console.Write("\nВведите ID курса для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int courseId))
            {
                Console.WriteLine("Некорректный ID курса");
                return;
            }

            // Удаление курса
            var courseToDelete = await context.Courses.FindAsync(courseId);
            if (courseToDelete != null)
            {
                context.Courses.Remove(courseToDelete);
                await context.SaveChangesAsync();
                Console.WriteLine("Курс успешно удален!");
            }
            else
            {
                Console.WriteLine("Курс не найден");
            }
        }
    }
}
