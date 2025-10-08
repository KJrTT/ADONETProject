using System;
using System.Threading.Tasks;
using CourseClient.Services;
using CourseClient.Views;

class Program
{
    static async Task Main()
    {
        var registrationService = new UserRegistrationService();
        var authService = new AuthService();

        var mainView = new ConsoleMainView(registrationService, authService);
        await mainView.RunAsync();
    }
}

