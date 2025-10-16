using System;
using System.Threading;
using System.Threading.Tasks;
using CourseClient.Services;
using CourseClient.Views;
using Microsoft.Extensions.Logging;

class Program
{
    private static CancellationTokenSource _cancellationTokenSource = new();

    static async Task Main()
    {
        // Обработка Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Предотвращаем немедленное закрытие
            _cancellationTokenSource.Cancel();
        };

        try
        {
            var registrationService = new UserRegistrationService();
            var authService = new AuthService();

            var mainView = new ConsoleMainView(registrationService, authService);

            // Если RunAsync не принимает CancellationToken, вызываем без параметров
            await mainView.RunAsync();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Приложение завершено пользователем.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}