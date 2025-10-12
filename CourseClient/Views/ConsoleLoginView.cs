using System;
using System.Threading.Tasks;
using CourseClient.Models;
using CourseClient.Services;

namespace CourseClient.Views
{
    public class ConsoleLoginView
    {
        private readonly IAuthService _authService;
        public LoginResult LoginResult { get; private set; }
        public bool IsLoggedIn => LoginResult != null && LoginResult.Success;
        public ConsoleLoginView(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task RunAsync()
        {
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? string.Empty;

            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? string.Empty;

            var result = await _authService.LoginAsync(email, password);

            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Неверный логин или пароль");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }
            if (result.RoleName == "banned")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Аккаунт был забанен");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Вход выполнен. Роль: {result.RoleName}");
            Console.ResetColor();
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            
            LoginResult = result;

        }
        
    }
}



