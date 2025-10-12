using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseClient.Services;


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
            Console.Write("Введите ID пользователя: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID пользователя");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите новую роль (ID): ");
            if (!int.TryParse(Console.ReadLine(), out int newRoleId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID роли");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = await _adminService.UpdateUserRoleAsync(userId, newRoleId);
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Роль пользователя успешно изменена!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Не удалось изменить роль пользователя");
                Console.ResetColor();
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task DeleteCourseAsync()
        {
            Console.Write("Введите ID курса для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int courseId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID курса");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = await _adminService.DelCourse(courseId);
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Курс успешно удален!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Не удалось удалить курс");
                Console.ResetColor();
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}
