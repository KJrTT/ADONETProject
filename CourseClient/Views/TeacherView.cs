using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseClient.Models;
using CourseClient.Services;

namespace CourseClient.Views
{
    internal class TeacherView
    {
        private readonly int _teacherId;
        private readonly IAuthService _authService;
        private readonly ITeacherService _teacherService;

        public TeacherView(int teacherId, IAuthService authService, ITeacherService teacherService)
        {
            _teacherId = teacherId;
            _authService = authService;
            _teacherService = teacherService;
        }

        public async Task<bool> RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n=== ПРЕПОДАВАТЕЛЬ ===");
                Console.WriteLine("1) Мои курсы");
                Console.WriteLine("2) Посмотреть записанных на курс");
                Console.WriteLine("3) Удалить мой курс");
                Console.WriteLine("0) Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ShowMyCoursesAsync();
                        break;
                    case "2":
                        await ShowEnrolledUsersAsync();
                        break;
                    case "3":
                        await DeleteOwnCourseAsync();
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

        private async Task ShowMyCoursesAsync()
        {
            var courses = await _teacherService.GetMyCoursesAsync(_teacherId);
            if (courses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("У вас пока нет созданных курсов.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n— Ваши курсы —");
            Console.ResetColor();
            foreach (var c in courses)
            {
                Console.WriteLine($"ID: {c.CourseId} | {c.name_course} | {c.data_start:yyyy-MM-dd} → {c.data_end:yyyy-MM-dd} | {c.price:C}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task ShowEnrolledUsersAsync()
        {
            Console.Write("Введите ID курса: ");
            if (!int.TryParse(Console.ReadLine(), out int courseId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID курса");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var users = await _teacherService.GetEnrolledUsersAsync(_teacherId, courseId);
            if (users.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Либо курс не ваш, либо на курс пока никто не записан.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n— Записанные пользователи —");
            Console.ResetColor();
            foreach (var u in users)
            {
                Console.WriteLine($"ID: {u.UserId} | {u.last_name} {u.first_name} | {u.user_email}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task DeleteOwnCourseAsync()
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

            var result = await _teacherService.DeleteOwnCourseAsync(_teacherId, courseId);
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Курс удален");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Не удалось удалить курс (возможно, это не ваш курс)");
                Console.ResetColor();
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}


