using System;
using System.Data.SqlClient;
using CourseClient.Data;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main()
    {
        try
        {
            using AppDbContext db = new AppDbContext();


            // Проверка подключения
            bool canConnect = db.Database.CanConnect();

            if (canConnect)
            {
                Console.WriteLine("✅ Подключение к базе данных успешно!");
                Console.WriteLine($"Имя базы данных: {db.Database.GetDbConnection().Database}");
                Console.WriteLine($"Сервер: {db.Database.GetDbConnection().DataSource}");
            }
            else
            {
                Console.WriteLine("❌ Не удалось подключиться к базе данных");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
        }

        Console.ReadKey();
    }

    //static string connectionString = "Server=A1208339\\SQLEXPRESS;Database=SaleCourses;Trusted_Connection=true;TrustServerCertificate=true";

    //static void Main()
    //{
    //    CheckConnection();
    //}

    //static void CheckConnection()
    //{
    //    try
    //    {
    //        using var connection = new SqlConnection(connectionString);
    //        connection.Open();
    //        Console.WriteLine("✅ Подключение к базе данных успешно!");
    //        Console.WriteLine($"Server: {connection.DataSource}");
    //        Console.WriteLine($"Database: {connection.Database}");
    //        Console.WriteLine($"State: {connection.State}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
    //    }
    //}
}

