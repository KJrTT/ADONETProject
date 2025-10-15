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

                Console.Write("Фамилия: ");
                string lastName = Console.ReadLine() ?? string.Empty;

                Console.Write("Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                Console.Write("Длина пароля (по умолчанию длина пароля: 12): ");
                if (!int.TryParse(Console.ReadLine(), out int passwordLength) || passwordLength < 8 && passwordLength < 50)
                    passwordLength = 12;
                Console.WriteLine("\nРегистрируем...");

                bool hasErrors = false;

                if (!ValidFirstName.ValidFirstNamefunction(firstName))
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Имя заполнено некорректно");
                    Console.ResetColor();
                    hasErrors = true;
                }
                if (!ValidLastName.ValidLastNamefunction(lastName))
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Фамилия заполнена некорректно");
                    Console.ResetColor();
                    hasErrors = true;
                }
                if (!IsValidEmail.ValidEmail(email))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Почта некорректна");
                    Console.ResetColor();
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    Console.WriteLine("Исправьте ошибки и попробуйте снова");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue; 
                }

                try
                {
                    string password = GeneratePassword(passwordLength);
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

        private static string GeneratePassword(int length)
        {
            if (length < 8) length = 12;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }
            return builder.ToString();
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

                    // Дополнительная проверка на опасные последовательности
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
            public static bool ValidLastNamefunction( string last_name)
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
    }
}


