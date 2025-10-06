using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

class Program
{
    static string connectionString = "Server=A1208339\\SQLEXPRESS;Database=SaleCourses;Trusted_Connection=true;TrustServerCertificate=true";

    static void Main()
    {
        CheckConnection();
    }

    static void CheckConnection()
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("✅ Подключение к базе данных успешно!");
            Console.WriteLine($"Server: {connection.DataSource}");
            Console.WriteLine($"Database: {connection.Database}");
            Console.WriteLine($"State: {connection.State}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
        }
    }
}