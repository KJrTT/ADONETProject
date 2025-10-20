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

        #region Вспомогательные методы валидации

        private static bool ValidateCourseName(string name, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Название курса не может быть пустым";
                return false;
            }

            var trimmedName = name.Trim();
            if (trimmedName.Length < 3)
            {
                errorMessage = "Название курса должно содержать минимум 3 символа";
                return false;
            }

            if (trimmedName.Length > 100)
            {
                errorMessage = "Название курса не может превышать 100 символов";
                return false;
            }

            return true;
        }

        private static bool ValidateDate(DateTime date, bool isStartDate, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (isStartDate && date < DateTime.Today)
            {
                errorMessage = "Дата начала курса не может быть в прошлом";
                return false;
            }

            return true;
        }

        private static bool ValidateDateRange(DateTime startDate, DateTime endDate, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (endDate < startDate)
            {
                errorMessage = "Дата окончания курса не может быть раньше даты начала";
                return false;
            }

            var maxEndDate = startDate.AddYears(3);
            if (endDate > maxEndDate)
            {
                errorMessage = "Курс не может длиться более 3 лет";
                return false;
            }

            return true;
        }

        private static bool ValidatePrice(decimal price, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (price < 0)
            {
                errorMessage = "Цена курса не может быть отрицательной";
                return false;
            }

            if (price > 1000000)
            {
                errorMessage = "Цена курса не может превышать 1,000,000";
                return false;
            }

            return true;
        }

        private static bool ValidateCourseId(string input, List<Course> availableCourses, out int courseId, out string errorMessage)
        {
            courseId = 0;
            errorMessage = string.Empty;

            if (!int.TryParse(input, out courseId))
            {
                errorMessage = "Некорректный формат ID. Введите число";
                return false;
            }

            if (courseId <= 0)
            {
                errorMessage = "ID курса должен быть положительным числом";
                return false;
            }

            var courseIdToCheck = courseId;
            var courseExists = availableCourses.Any(c => c.CourseId == courseIdToCheck);
            if (!courseExists)
            {
                errorMessage = "Курс с таким ID не найден среди ваших курсов";
                return false;
            }

            return true;
        }

        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void DisplayWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void DisplayInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        #endregion

        public async Task<bool> RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n=== ПРЕПОДАВАТЕЛЬ ===");
                Console.WriteLine("1) Добавить курс");
                Console.WriteLine("2) Мои курсы");
                Console.WriteLine("3) Посмотреть записанных на курс");
                Console.WriteLine("4) Удалить мой курс");
                Console.WriteLine("0) Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await CreateCourseAsync();
                        break;
                    case "2":
                        await ShowMyCoursesAsync();
                        break;
                    case "3":
                        await ShowEnrolledUsersAsync();
                        break;
                    case "4":
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
                DisplayWarning("У вас пока нет созданных курсов.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            DisplayInfo("\n— Ваши курсы —");
            foreach (var c in courses)
            {
                Console.WriteLine($"ID: {c.CourseId} | {c.name_course} | {c.data_start:yyyy-MM-dd} → {c.data_end:yyyy-MM-dd} | {c.price:C}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task CreateCourseAsync()
        {
            Console.WriteLine("\n=== СОЗДАНИЕ КУРСА ===");
            
            // Ввод названия курса
            string name;
            do
            {
                Console.Write("Название курса (3-100 символов): ");
                name = Console.ReadLine() ?? string.Empty;
                
                if (!ValidateCourseName(name, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!ValidateCourseName(name, out _));

            // Ввод даты начала
            DateTime startDate;
            string startDateInput;
            do
            {
                Console.Write("Дата начала (yyyy-MM-dd, не раньше сегодня): ");
                startDateInput = Console.ReadLine();
                
                if (!DateTime.TryParse(startDateInput, out startDate))
                {
                    DisplayError("Некорректный формат даты. Используйте формат yyyy-MM-dd");
                }
                else if (!ValidateDate(startDate, true, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!DateTime.TryParse(startDateInput, out startDate) || !ValidateDate(startDate, true, out _));

            // Ввод даты окончания
            DateTime endDate;
            string endDateInput;
            do
            {
                Console.Write("Дата окончания (yyyy-MM-dd, максимум 3 года от начала): ");
                endDateInput = Console.ReadLine();
                
                if (!DateTime.TryParse(endDateInput, out endDate))
                {
                    DisplayError("Некорректный формат даты. Используйте формат yyyy-MM-dd");
                }
                else if (!ValidateDateRange(startDate, endDate, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!DateTime.TryParse(endDateInput, out endDate) || !ValidateDateRange(startDate, endDate, out _));

            // Ввод цены
            decimal price;
            string priceInput;
            do
            {
                Console.Write("Цена курса (0 - 1,000,000): ");
                priceInput = Console.ReadLine();
                
                if (!decimal.TryParse(priceInput, out price))
                {
                    DisplayError("Некорректный формат цены. Введите число");
                }
                else if (!ValidatePrice(price, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!decimal.TryParse(priceInput, out price) || !ValidatePrice(price, out _));

            // Создание курса
            var result = await _teacherService.CreateCourseAsync(_teacherId, name, startDate, endDate, price);
            if (result.Success)
            {
                DisplaySuccess("Курс успешно создан!");
            }
            else
            {
                DisplayError($"Ошибка: {result.ErrorMessage}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task ShowEnrolledUsersAsync()
        {
            Console.WriteLine("\n=== ПРОСМОТР ЗАПИСАННЫХ ПОЛЬЗОВАТЕЛЕЙ ===");
            
            // Сначала показываем доступные курсы
            var courses = await _teacherService.GetMyCoursesAsync(_teacherId);
            if (courses.Count == 0)
            {
                DisplayWarning("У вас пока нет созданных курсов.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            DisplayInfo("\n— Ваши курсы —");
            foreach (var c in courses)
            {
                Console.WriteLine($"ID: {c.CourseId} | {c.name_course} | {c.data_start:yyyy-MM-dd} → {c.data_end:yyyy-MM-dd}");
            }

            // Ввод ID курса с валидацией
            int courseId;
            string courseIdInput;
            do
            {
                Console.Write("\nВведите ID курса: ");
                courseIdInput = Console.ReadLine();
                
                if (!ValidateCourseId(courseIdInput, courses, out courseId, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!ValidateCourseId(courseIdInput, courses, out courseId, out _));

            var users = await _teacherService.GetEnrolledUsersAsync(_teacherId, courseId);
            if (users.Count == 0)
            {
                DisplayWarning("На курс пока никто не записан.");
            }
            else
            {
                DisplayInfo($"\n— Записанные пользователи (всего: {users.Count}) —");
                foreach (var u in users)
                {
                    Console.WriteLine($"ID: {u.UserId} | {u.last_name} {u.first_name} | {u.user_email}");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private async Task DeleteOwnCourseAsync()
        {
            Console.WriteLine("\n=== УДАЛЕНИЕ КУРСА ===");
            
            // Сначала показываем доступные курсы
            var courses = await _teacherService.GetMyCoursesAsync(_teacherId);
            if (courses.Count == 0)
            {
                DisplayWarning("У вас пока нет созданных курсов.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            DisplayInfo("\n— Ваши курсы —");
            foreach (var c in courses)
            {
                Console.WriteLine($"ID: {c.CourseId} | {c.name_course} | {c.data_start:yyyy-MM-dd} → {c.data_end:yyyy-MM-dd} | {c.price:C}");
            }

            // Ввод ID курса с валидацией
            int courseId;
            string courseIdInput;
            do
            {
                Console.Write("\nВведите ID курса для удаления: ");
                courseIdInput = Console.ReadLine();
                
                if (!ValidateCourseId(courseIdInput, courses, out courseId, out string errorMessage))
                {
                    DisplayError(errorMessage);
                }
            } while (!ValidateCourseId(courseIdInput, courses, out courseId, out _));

            // Подтверждение удаления
            var courseToDelete = courses.First(c => c.CourseId == courseId);
            DisplayWarning($"\nВы действительно хотите удалить курс:");
            Console.WriteLine($"\"{courseToDelete.name_course}\" (ID: {courseToDelete.CourseId})?");
            Console.WriteLine("Это действие нельзя отменить!");
            Console.Write("Введите 'ДА' для подтверждения или любую другую строку для отмены: ");
            
            var confirmation = Console.ReadLine();
            if (confirmation?.ToUpper() != "ДА")
            {
                DisplayWarning("Удаление отменено");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }

            var result = await _teacherService.DeleteOwnCourseAsync(_teacherId, courseId);
            if (result)
            {
                DisplaySuccess("Курс успешно удален");
            }
            else
            {
                DisplayError("Не удалось удалить курс. Попробуйте позже");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}


