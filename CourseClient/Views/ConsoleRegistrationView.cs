using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CourseClient.Services;

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
                Console.WriteLine("\n- Введите данные для регистрации -");

                Console.Write("Имя: ");
                string firstName = Console.ReadLine() ?? string.Empty;

                Console.Write("Фамилия: ");
                string lastName = Console.ReadLine() ?? string.Empty;

                Console.Write("Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                Console.Write("Длина пароля (по умолчанию 12): ");
                if (!int.TryParse(Console.ReadLine(), out int passwordLength) || passwordLength < 8)
                    passwordLength = 12;

                Console.WriteLine("\nРегистрируем...");

                bool hasErrors = false;

                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Имя не может быть пустым");
                    Console.ResetColor();
                    hasErrors = true;
                }

                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Фамилия не может быть пустой");
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
                }
                catch (InvalidOperationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ОШИБКА: {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ ОШИБКА: {ex.Message}");
                    Console.ResetColor();
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
                    string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}


