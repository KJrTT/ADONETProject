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
using System.Diagnostics.Metrics;
using System.Data.Common;



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
                Console.WriteLine("\n========================================== Интерфейс Студента ========================================== \n");
                await ShowinfoUser(_userId);
                Console.WriteLine("1) Мои курсы");
                Console.WriteLine("2) Доступные курсы");
                Console.WriteLine("3) Бесплатные курсы");
                Console.WriteLine("4) Записаться на курс");
                Console.WriteLine("5) Отписаться от курса");
                Console.WriteLine("6) Редактирование пользователя");
                Console.WriteLine("7) Удалить аккаунт");
                Console.WriteLine("8) Пополнить баланс");
                Console.WriteLine("9) Просмотр баланса");
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
                            await EnrollToCourseAsync();
                        break;
                    case "5":
                            await UnenrollFromCourseAsync();
                        break;
                    case "6":
                            await UpdateUserinfoAsync(_userId); 
                        break;
                    case "7":
                        await DeleteAccountAsync();
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



        private async Task ShowinfoUser(int _userId)
        {
            try
            {
                Console.WriteLine("=========== Информация о пользователе ===========");
                var checkouser = await _studentService.ShowinfoUser(_userId);

                if (checkouser.Count == 0)
                {
                    Console.WriteLine("Произошла ошибка вывода информации о пользователе");
                }
                foreach (dynamic info in checkouser)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   Имя: {info.first_name}");
                    Console.WriteLine($"   Фамилия: {info.last_name}");
                    Console.WriteLine($"   Почта: {info.user_email}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   Баланс: {info.balance}");
                    Console.ResetColor();
                    Console.WriteLine("======================================= \n");
                }
            }
            catch (DbException dbex)
            {
                Console.WriteLine($"Ошибка БД -> {dbex.Message}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Внезапная ошибка -> {ex.Message}");
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

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===============================================");
            Console.WriteLine("                 МОИ КУРСЫ");
            Console.WriteLine("===============================================");
            Console.ResetColor();

            int counter = 1;
            foreach (dynamic c in myCourses)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nКУРС #{counter}");
                Console.ResetColor();


                Console.WriteLine($"   Id курса: {c.CourseId}");
                Console.WriteLine($"   Название: {c.name_course}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Цена: {c.price:F2}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"   Период: {c.data_start:dd.MM.yy} - {c.data_end:dd.MM.yy}");
                Console.ResetColor();

                
                DateTime today = DateTime.Today;
                if (today < c.data_start)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("   Статус: Ожидается начало");
                }
                else if (today > c.data_end)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("   Статус: Завершен");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("   Статус: В процессе");
                }
                Console.ResetColor();

                counter++;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n===============================================");
            Console.WriteLine($"Всего курсов: {myCourses.Count}");
            Console.WriteLine("===============================================");
            Console.ResetColor();

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task<List<Course>> ShowAvailableCoursesAsync()
        {
            var availableCourses = await _studentService.GetAvailableCoursesAsync(_userId);
            
            if (availableCourses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Нет доступных курсов для записи.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return availableCourses;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n   ДОСТУПНЫЕ КУРСЫ");
            Console.WriteLine(" ----------------------");
            Console.ResetColor();

            int counter = 1;
            foreach (var c in availableCourses)
            {
                Console.WriteLine($"\n{counter}. {c.name_course}");
                Console.WriteLine($"   ID: {c.CourseId}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Цена: {c.price:F2}");
                Console.ResetColor();
                Console.WriteLine($"   Дата: {c.data_start:dd.MM.yy} - {c.data_end:dd.MM.yy}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Статус: Не подписан");
                Console.ResetColor();

                counter++;
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"\nВсего доступных курсов: {availableCourses.Count}");
            Console.ResetColor();

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return availableCourses;
        }

        private async Task ShowFreeCoursesAsync()
        {
            var freeCourses = await _studentService.GetFreeCoursesAsync();

            if (freeCourses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Нет доступных бесплатных курсов.");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===============================================");
            Console.WriteLine("             БЕСПЛАТНЫЕ КУРСЫ");
            Console.WriteLine("===============================================");
            Console.ResetColor();

            int counter = 1;
            foreach (dynamic c in freeCourses)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nКУРС #{counter}");
                Console.ResetColor();

                Console.WriteLine($"   ID: {c.CourseId}");
                Console.WriteLine($"   Название: {c.name_course}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Дата начала: {c.data_start:dd.MM.yyyy}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"   Дата окончания: {c.data_end:dd.MM.yyyy}");
                Console.ResetColor();

                counter++;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n===============================================");
            Console.WriteLine($"Всего бесплатных курсов: {freeCourses.Count}");
            Console.WriteLine("===============================================");
            Console.ResetColor();

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task EnrollToCourseAsync()
        {
            var visualcourse = await ShowAvailableCoursesAsync();
            if (visualcourse.Count == 0)
            {
                return;
            }
            Console.WriteLine("\n===============================================");
            Console.Write("Введите ID курса для записи: ");
            if (int.TryParse(Console.ReadLine(), out var courseIdToEnroll))
            {
                var result = await _studentService.EnrollToCourseAsync(_userId, courseIdToEnroll);
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
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID курса");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        private async Task UnenrollFromCourseAsync()
        {
            await ShowMyCoursesAsync();

            Console.WriteLine("\n===============================================");
            Console.Write("Введите ID курса для отписки: ");
            if (int.TryParse(Console.ReadLine(), out var courseIdToUnenroll))
            {
                var result = await _studentService.UnenrollFromCourseAsync(_userId, courseIdToUnenroll);

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
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ID курса");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }  
        }

        private async Task<bool> UpdateUserinfoAsync(int userId)
        {
            Console.Clear();
            Console.WriteLine("\nЧто вы хотите обновить?");
            Console.WriteLine("1) Пароль");
            Console.WriteLine("2) Email");
            Console.Write("Ваш выбор: ");

            if (int.TryParse(Console.ReadLine(), out var editing))
            {
                switch (editing)
                {
                    case 1:
                        Console.Write("Введите новый пароль: ");
                        var newPassword = Console.ReadLine() ?? string.Empty;
                        var passwordResult = await _studentService.UpdatePasswordAsync(userId, newPassword);
                        if (!passwordResult)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Ошибка: некорректный пароль");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            return false;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Пароль успешно обновлен");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            return true;
                        }
                    case 2:
                        Console.Write("Введите новый email: ");
                        var newEmail = Console.ReadLine() ?? string.Empty;
                        var emailResult = await _studentService.UpdateEmailAsync(userId, newEmail);
                        if (!emailResult)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Ошибка: некорректный email адрес");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            return false;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Email успешно обновлен");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            return true;
                        }
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Неверный выбор");
                        Console.ResetColor();
                        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        return false;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ввод");
                Console.ResetColor();
                return false;
            }
        }

        
        private async Task<int> DeleteAccountAsync()
        {
            Console.Clear();
            Console.Write("Точно удалить аккаунт? (y/n): ");
            if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "y")
            {
                Console.Write("Для того чтобы удалить аккаунт, введите пароль: ");
                var password = Console.ReadLine() ?? string.Empty;
                var check = await _studentService.Checkpassword(password, _userId);
                if (!check)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Пароли не совпадают! Повторите попытку");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    return 0;
                }
                var deleted = await _studentService.DeleteAccountAsync(_userId);

                if (deleted)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Аккаунт удалён");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return 2;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Не удалось удалить аккаунт");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    return 0;
                }
            }
            Console.WriteLine("Удаление отменено");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return 0;
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

    
