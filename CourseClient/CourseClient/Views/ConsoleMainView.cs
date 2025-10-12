using System;
using System.Threading.Tasks;
using CourseClient.Services;

namespace CourseClient.Views
{
    public class ConsoleMainView
    {
        private readonly IUserRegistrationService _registrationService;
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly ITeacherService _teacherService;

        public ConsoleMainView(IUserRegistrationService registrationService, IAuthService authService, IAdminService adminService, ITeacherService teacherService)
        {
            _registrationService = registrationService;
            _authService = authService;
            _adminService = adminService; // Инициализируем
            _teacherService = teacherService;
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
                        if (loginView.IsLoggedIn)
                        {
                            await RouteByRoleAsync(loginView.LoginResult);
                        }
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
                    var adminView = new AdminView(login.UserId, _authService, _adminService);
                    await adminView.RunAsync();
                    break;
                case "Teacher":
                    var teacherView = new TeacherView(login.UserId, _authService, _teacherService);
                    await teacherView.RunAsync();
                    break;
                case "Student":
                    var studentView = new StudentView(login.UserId, _authService);
                    await studentView.RunAsync(login);
                    break;
                case "Baned":
                    Console.WriteLine("Ваш аккаунт забанен!!!");
                    break;
                default:
                    Console.WriteLine("Неизвестная роль");
                    break;
            }
        }

    }
}



