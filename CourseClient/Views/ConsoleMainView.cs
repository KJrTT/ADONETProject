using System;
using System.Threading.Tasks;
using CourseClient.Services;

namespace CourseClient.Views
{
    public class ConsoleMainView
    {
        private readonly IUserRegistrationService _registrationService;
        private readonly IAuthService _authService;

        public ConsoleMainView(IUserRegistrationService registrationService, IAuthService authService)
        {
            _registrationService = registrationService;
            _authService = authService;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1) Регистрация");
                Console.WriteLine("2) Вход");
                Console.WriteLine("0) Выход");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        var regView = new ConsoleRegistrationView(_registrationService);
                        await regView.RunAsync();
                        break;
                    case "2":
                        var loginView = new ConsoleLoginView(_authService);
                        await loginView.RunAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда");
                        break;
                }
            }
        }

        private async Task RouteByRoleAsync(LoginResult login)
        {
            switch (login.RoleName)
            {
                case "Admin":
                    // Реализация интерфейса чёт типо такого await RunAdminMenuAsync(login.UserId);
                    break;
                case "Teacher":
                    // Реализация интерфейса
                    break;
                case "Student":
                    // Реализация интерфейса
                    break;
                default:
                    Console.WriteLine("Неизвестная роль");
                    break;
            }
        }

    }
}


