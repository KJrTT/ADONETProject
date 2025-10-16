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
        // ��������� Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // ������������� ����������� ��������
            _cancellationTokenSource.Cancel();
        };

        try
        {
            var registrationService = new UserRegistrationService();
            var authService = new AuthService();

            var mainView = new ConsoleMainView(registrationService, authService);

            // ���� RunAsync �� ��������� CancellationToken, �������� ��� ����������
            await mainView.RunAsync();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("���������� ��������� �������������.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"��������� ������: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("������� ����� ������� ��� ������...");
            Console.ReadKey();
        }
    }
}