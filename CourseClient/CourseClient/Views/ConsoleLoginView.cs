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
                Console.WriteLine("Неверный логин или пароль");
                return;
            }

            Console.WriteLine($"Вход выполнен. Роль: {result.RoleName}");
            
            LoginResult = result;

        }
        
    }
}



