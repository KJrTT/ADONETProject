using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CourseClient.Services;
using Microsoft.IdentityModel.Tokens;

namespace CourseClient.Views
{
    public class ConsoleRegistrationView
    {
        private readonly IUserRegistrationService _registrationService;

        public ConsoleRegistrationView(IUserRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                
                Console.Clear();
                Console.WriteLine("\n- Введите данные для регистрации -");

                Console.Write("Имя: ");
                string firstName = Console.ReadLine() ?? string.Empty;
                
                while (!ValidFirstName.ValidFirstNamefunction(firstName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Имя заполнено некорректно!\n");
                    Console.ResetColor();
                    Console.Write("Пожалуйста, введите имя еще раз: ");
                    firstName = Console.ReadLine() ?? string.Empty;
                }
                
                Console.Write("Фамилия: ");
                string lastName = Console.ReadLine() ?? string.Empty;

                while (!ValidLastName.ValidLastNamefunction(lastName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Фамилия заполнена некорректно!\n");
                    Console.ResetColor();
                    Console.Write("Пожалуйста, введите фамилию еще раз: ");
                    lastName = Console.ReadLine() ?? string.Empty;
                }

                Console.Write("Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                while (!IsValidEmail.ValidEmail(email))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Email некорректен!\n");
                    Console.ResetColor();
                    Console.Write("Пожалуйста, введите email еще раз: ");
                    email = Console.ReadLine() ?? string.Empty;
                }
                bool hasErrors = false;
                string password = string.Empty;
                string Confirmationpassword = string.Empty;

                Console.Write("Введите пароль: ");
                password = Console.ReadLine() ?? string.Empty;
                var passwordValidation = await ValidPassword(password);

                if (!passwordValidation.isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Пароль заполнен некорректно: {passwordValidation.message}");
                    Console.ResetColor();
                    hasErrors = true;
                }
                else
                {
                    Console.Write("\nПовторите пароль: ");
                    Confirmationpassword = Console.ReadLine() ?? string.Empty;

                    if (password != Confirmationpassword)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Пароли не совпадают!");
                        Console.ResetColor();
                        hasErrors = true;
                    }
                }

                if (hasErrors)
                {
                    Console.WriteLine("Исправьте ошибки и попробуйте снова");
                    Console.WriteLine("Хотите повторить регистрацию?");
                    Console.Write("Введите Y, если да и N, если нет:  ");
                    if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "y")
                        continue;
                    break;
                }

                Console.WriteLine("\nРегистрируем...");
                try
                { 
                    var result = await _registrationService.RegisterClientAsync(firstName, lastName, email, password);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ УСПЕХ: {result.Status}");
                    Console.ResetColor();
                    Console.WriteLine($"ID клиента: {result.ClientId}");
                    Console.WriteLine($"ID кошелька: {result.WalletId}");
                    Console.WriteLine($"Пароль: {result.GeneratedPassword}");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
                catch (InvalidOperationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ОШИБКА: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ ОШИБКА: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }

                Console.Write("\nПродолжить? (y/n): ");
                if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() != "y")
                    break;
            }
        }
        public class IsValidEmail
        {
            public static bool ValidEmail(string email)
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }

                try
                {
                    string pattern = @"^[a-zA-Z0-9.!#$&'*+/=?^_`{|}~-]+" +
                @"@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?" +
                @"(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

                    var isValid = Regex.IsMatch(email, pattern);
                    if (isValid && email.Contains("%00"))
                        return false;

                    return isValid;
                }
                catch
                {
                    return false;
                }
            }
        }
        public class ValidFirstName
        {
            public static bool ValidFirstNamefunction(string first_name)
            {
                if (string.IsNullOrEmpty(first_name))
                {
                    return false;
                }
                try
                {
                    string pattern = @"^[a-zA-Zа-яА-ЯёЁ'\-\s]{2,50}$";
                    return Regex.IsMatch(first_name, pattern);
                }
                catch
                {
                    return false;
                }
            }
        }
        public class ValidLastName
        {
            public static bool ValidLastNamefunction(string last_name)
            {
                if (string.IsNullOrEmpty(last_name))
                {
                    return false;
                }
                try
                {
                    string pattern = @"^[a-zA-Zа-яА-ЯёЁ'\-\s]{2,50}$";
                    return Regex.IsMatch(last_name, pattern);
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<(bool isValid, string message)> ValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "Пароль не может быть пустым");
            }

            try
            {
                if (password.Length < 8)
                {
                    return (false, "Пароль должен содержать минимум 8 символов");
                }

                string upperCasePattern = @"[A-Z]";
                if (!Regex.IsMatch(password, upperCasePattern))
                {
                    return (false, "Пароль должен содержать хотя бы одну заглавную букву");
                }

                string lowerCasePattern = @"[a-z]";
                if (!Regex.IsMatch(password, lowerCasePattern))
                {
                    return (false, "Пароль должен содержать хотя бы одну строчную букву");
                }

                string digitPattern = @"[0-9]";
                if (!Regex.IsMatch(password, digitPattern))
                {
                    return (false, "Пароль должен содержать хотя бы одну цифру");
                }

                string specialCharPattern = @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]";
                if (!Regex.IsMatch(password, specialCharPattern))
                {
                    return (false, "Пароль должен содержать хотя бы один специальный символ");
                }

                if (password.Contains(" "))
                {
                    return (false, "Пароль не должен содержать пробелы");
                }

                string[] commonPasswords = { "password", "123456", "qwerty", "admin" };
                if (commonPasswords.Contains(password.ToLower()))
                {
                    return (false, "Пароль слишком простой и распространенный");
                }

                bool passwordExists = await _registrationService.Checkpassword(password);
                if (passwordExists)
                {
                    return (false, "Этот пароль уже используется другим пользователем. Выберите другой пароль.");
                }

                return (true, "Пароль корректен");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при проверке пароля: {ex.Message}");
            }
        }
    }
}


