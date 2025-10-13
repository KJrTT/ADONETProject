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
using Microsoft.Identity.Client;



namespace CourseClient.Views
{
    internal class StudentView
    {
        private readonly int _userId;
        private readonly IAuthService _authService;
        private readonly IStudentService _studentService;

        public StudentView(IAuthService authService, IStudentService studentService)
        {
            _authService = authService;
            _studentService = studentService;
        }

        public StudentView(int userId, IAuthService authService, IStudentService studentService)
        {
            _userId = userId;
            _authService = authService;
            _studentService = studentService;
        }
        public async Task<bool> RunAsync(LoginResult login)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n=== СТУДЕНТ ===");
                Console.WriteLine("1) Мои курсы");
                Console.WriteLine("2) Доступные курсы");
                Console.WriteLine("3) Бесплатные курсы");
                Console.WriteLine("4) Записаться на курс");
                Console.WriteLine("5) Отписаться от курса");
                Console.WriteLine("6) Обновить пароль");
                Console.WriteLine("7) Удалить аккаунт");
                Console.WriteLine("8) Пополнить баланс");
                Console.WriteLine("9) Просмотр баланса");
                Console.WriteLine("10) Обновить почту (email) (Временно недоступно)");
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
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Некорректный ID курса");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                        }
                        break;
                    case "5":
                        Console.Write("Введите ID курса для отписки: ");
                        if (int.TryParse(Console.ReadLine(), out var courseIdToUnenroll))
                            await UnenrollFromCourseAsync(courseIdToUnenroll);
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Некорректный ID курса");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                        }
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
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Аккаунт удалён.");
                                Console.ResetColor();
                                return false;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Не удалось удалить аккаунт");
                                Console.ResetColor();
                                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                                Console.ReadKey();
                            }
                        }
                        break;
                    case "8":
                        Console.Write("Введите сумму для пополнения баланса: ");
                        if (decimal.TryParse(Console.ReadLine(), out var replenishmentofbalance))
                        {
                            await ReplenishmentbalanceAsync(replenishmentofbalance);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Некорректная сумма");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                        }
                        break;
                    case "9":
                        await CheckBalanceAsync();
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
            var myCourses = await _studentService.GetMyCoursesAsync(_userId);
           
            if (myCourses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("У вас нет записей на курсы.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\nМои курсы:");
            Console.ResetColor();
            foreach (dynamic c in myCourses)
            {
                Console.WriteLine($"[{c.CourseId}] {c.name_course} | Цена: {c.price} | {c.data_start:d} - {c.data_end:d}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task<List<Course>> ShowAvailableCoursesAsync()
        {
            var availableCourses = await _studentService.GetAvailableCoursesAsync(_userId);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\nДоступные курсы:");
            Console.ResetColor();
            foreach (var c in availableCourses)
            {
                Console.WriteLine($"[ID курса -> {c.CourseId} \n Название курса -> {c.name_course} \n Цена курса -> {c.price} \n Статус -> Не подписан]");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return availableCourses;
        }

        private async Task ShowFreeCoursesAsync()
        {
            var freeCourses = await _studentService.GetFreeCoursesAsync();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\nБесплатные курсы: ");
            Console.ResetColor();
            foreach (dynamic c in freeCourses)
            {
                Console.WriteLine($"[Название курса -> {c.name_course}] \n Дата начала -> {c.data_start} \n Окончание курса -> {c.data_end}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task EnrollToCourseAsync(int courseId)
        {
            var result = await _studentService.EnrollToCourseAsync(_userId, courseId);
            
            if (!result)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при записи на курс. Возможно, курс не найден или вы уже записаны на него, или недостаточно средств.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var balance = await _studentService.GetBalanceAsync(_userId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Вы подписались на курс! \n На вашем балансе осталось => {balance}");
            Console.ResetColor();
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task UnenrollFromCourseAsync(int courseId)
        {
            var result = await _studentService.UnenrollFromCourseAsync(_userId, courseId);
            
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Вы отписались от курса");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Такого курса нету");
                Console.ResetColor();
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Пустое значение");
                Console.ResetColor();
                return false;
            }
            if (newPassword.Length < 8)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Пароль должен быть больше 8 символов");
                Console.ResetColor();
                return false;
            }

            var result = await _studentService.UpdatePasswordAsync(_userId, newPassword);
            
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Пароль обновлён");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка обновления пароля");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return false;
            }
        }


        private async Task<bool> DeleteAccountAsync()
        {
            return await _studentService.DeleteAccountAsync(_userId);
        }

        private async Task<bool> ReplenishmentbalanceAsync(decimal replenishmentofbalance)
        {
            if (replenishmentofbalance <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Сумма пополнения должна быть больше 0");
                Console.ResetColor();
                return false;
            }
            
            var result = await _studentService.ReplenishBalanceAsync(_userId, replenishmentofbalance);
            
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Кошелек пополнен");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка пополнения");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return false;
            }
        }

        private async Task CheckBalanceAsync()
        {
            var balance = await _studentService.GetBalanceAsync(_userId);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Ваш баланс составляет: {balance}");
            Console.ResetColor();
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

    }
}

    
