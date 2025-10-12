using System;
using System.Threading.Tasks;
using CourseClient.Data;
using CourseClient.Services;
using CourseClient.Views;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main()
    {

        var dbContext = new AppDbContext();

        var registrationService = new UserRegistrationService();
        var authService = new AuthService();
        var adminService = new AdminService(dbContext);
        var teacherService = new TeacherService(dbContext);

        var mainView = new ConsoleMainView(registrationService, authService, adminService, teacherService);
        await mainView.RunAsync();
    }
}

