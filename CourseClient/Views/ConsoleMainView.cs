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
        public ConsoleMainView(IUserRegistrationService registrationService, IAuthService authService)
        {
            _registrationService = registrationService;
            _authService = authService;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Неизвестная команда");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private async Task RouteByRoleAsync(LoginResult login)
        {
            switch (login.RoleName)
            {
                case "Admin":
                    var adminService = new AdminService();
                    var adminView = new AdminView(login.UserId, _authService,adminService);
                    await adminView.RunAsync();
                    break;
                case "Teacher":
                    var teacherService = new TeacherService();
                    var teacherview = new TeacherView(login.UserId, _authService,teacherService);
                    await teacherview.RunAsync();
                    break;
                case "Student":
                    var studentService = new StudentService();
                    var studentView = new StudentView(login.UserId, _authService, studentService);
                    await studentView.RunAsync(login);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Неизвестная роль");
                    Console.ResetColor();
                    break;
            }
        }

    }
}



