using System;
using System.Data.Common;
using System.Threading.Tasks;
using CourseClient.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace CourseClient.Test
{

    public class AuthRegistrationTests
    {

        private readonly string _testEmail;
        private readonly string _testPassword;

        public AuthRegistrationTests()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var timestamp = DateTime.Now.Ticks;
            _testEmail = $"test{timestamp}@test.com";
            _testPassword = $"Password{timestamp}";
        }

        [Fact]
        public async Task TestRegistrationUsers()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var service = new UserRegistrationService();

            Console.WriteLine($"\u001b[33mТестирование регистрации с почтой {_testEmail}\u001b[0m");

            var result = await service.RegisterClientAsync("Иван", "Иванов", _testEmail, _testPassword);

            Console.WriteLine($"\u001b[36mРезультат регистрации: UserID => {result.ClientId}, Status => {result.Status}\u001b[0m");
            Assert.NotNull(result);
            Console.WriteLine($"\u001b[32mРезультат не NULL\u001b[0m");
            Assert.True(result.ClientId > 0);
            Console.WriteLine($"\u001b[32mUserID => валидный: {result.ClientId}\u001b[0m");
            Assert.Equal("Registered", result.Status);
            Console.WriteLine($"\u001b[32mStatus => 'Registered'\u001b[0m");
            Assert.Equal(_testPassword, result.GeneratedPassword);
            Console.WriteLine($"\u001b[32mСгенерированные пароли совпадают\u001b[0m");
            Console.WriteLine($"\u001b[32m Все тесты пройдены\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }

        [Fact]
        public async Task TestLoginSuccess()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var authservice = new AuthService();
            var registrationservice = new UserRegistrationService();
            Console.WriteLine($"\u001b[33mРегистрация пользователя для теста логина: {_testEmail}\u001b[0m");
            await registrationservice.RegisterClientAsync("Петр", "Петров", _testEmail, _testPassword);
            Console.WriteLine($"\u001b[36mПопытка логина с email: {_testEmail}\u001b[0m");
            var result = await authservice.LoginAsync(_testEmail, _testPassword);
            Console.WriteLine($"\u001b[36mРезультат логина: Success => {result.Success}, UserId => {result.UserId}, Role => {result.RoleName}\u001b[0m");
            Assert.True(result.Success);
            Console.WriteLine($"\u001b[32mSuccess => true\u001b[0m");
            Assert.True(result.UserId > 0);
            Console.WriteLine($"\u001b[32mUserId => валидный: {result.UserId}\u001b[0m");
            Assert.False(string.IsNullOrEmpty(result.RoleName));
            Console.WriteLine($"\u001b[32mRoleName => не пустой: {result.RoleName}\u001b[0m");
            Console.WriteLine($"\u001b[92mТест на вход пройден\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }

        [Fact]
        public async Task TestLoginnotSuccessEmail()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var authservice = new AuthService();
            var registrationservice = new UserRegistrationService();
            var notsuccessemail = "gr@gmail.com";
            Console.WriteLine($"\u001b[33mПопытка логина с несуществующим email: {notsuccessemail}\u001b[0m");

            var result = await authservice.LoginAsync(notsuccessemail, _testPassword);
            Console.WriteLine($"\u001b[36mРезультат логина с несуществующим email: Success => {result.Success}, UserId => {result.UserId}, Role => {result.RoleName}\u001b[0m");

            Assert.False(result.Success);
            Console.WriteLine($"\u001b[32mSuccess => false (ожидаемый результат для несуществующего email)\u001b[0m");
            Assert.True(result.UserId <= 0);
            Console.WriteLine($"\u001b[32mUserId => невалидный: {result.UserId}\u001b[0m");
            Assert.True(string.IsNullOrEmpty(result.RoleName));
            Console.WriteLine($"\u001b[32mRoleName => пустой\u001b[0m");
            Console.WriteLine($"\u001b[92mВсе несуществующие утверждения электронной почты пройдены\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }

        [Fact]
        public async Task TestRegistationnotSuccess()
        {
            var authservice = new AuthService();
            var registrationservice = new UserRegistrationService();

            Console.WriteLine($"\u001b[33m Попытка зарегистрировать аккаунт с незаполненными данными (first_name) и (last_name)\u001b[0m");

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => registrationservice.RegisterClientAsync(null, null, _testEmail, _testPassword));

            Console.WriteLine($"\u001b[32mПолучено ожидаемое исключение: {exception.GetType().Name}\u001b[0m");
            Console.WriteLine($"\u001b[32mСообщение => {exception.InnerException?.Message ?? exception.Message} \u001b[0m");
            Console.WriteLine($"\u001b[32mПроверим, является ли данная ошибка - ошибкой на NULL => {exception.InnerException?.Message ?? exception.Message} \u001b[0m");

            Assert.Contains("NULL", exception.InnerException?.Message ?? exception.Message);
            Console.WriteLine($"\u001b[32m✓ Исключение связано с NULL значениями\u001b[0m");

            Console.WriteLine($"\u001b[92mТест на null значения пройден\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }

    }

    public class AdminRegistrationTests
    {
        [Fact]
        public async Task TestUpdateUserRoleAsyncSuccess()
        {
            // Arrange
            var adminService = new AdminService();
            var registrationService = new UserRegistrationService();

            Console.WriteLine("Попытка обновить роль пользователя...");

            // Act
            var result = await adminService.UpdateUserRoleAsync(1004, 4);

            // Assert
            if (result)
            {
                Console.WriteLine("✅ Роль пользователя успешно обновлена");
                Console.WriteLine($"ID пользователя: 1004");
                Console.WriteLine($"Новая роль: 4");
            }
            else
            {
                Console.WriteLine("❌ Не удалось обновить роль пользователя");
                Console.WriteLine($"Ошибка: {result}");
            }

            Console.WriteLine("=============================");
        }

        [Fact]
        public async Task TestDelCourseSuccess()
        {
            // Arrange
            var service = new AdminService();
            int existingCourseId = 10;

            Console.WriteLine("🧪 Тестирование удаления курса");
            Console.WriteLine("=================================");
            Console.WriteLine($"📚 ID курса для удаления: {existingCourseId}");

            // Act
            Console.WriteLine("🔄 Выполнение удаления...");
            bool result = await service.DelCourse(existingCourseId);

            // Assert
            Console.WriteLine("📊 Результат выполнения:");

            if (result)
            {
                Console.WriteLine("✅ УДАЛЕНИЕ УСПЕШНО");
                Console.WriteLine($"💡 Курс с ID {existingCourseId} был удален из системы");
            }
            else
            {
                Console.WriteLine("❌ УДАЛЕНИЕ НЕ УДАЛОСЬ");
                Console.WriteLine($"⚠️ Курс с ID {existingCourseId} не был найден или произошла ошибка");
                Console.WriteLine($"{result}");
            }

            Console.WriteLine("=================================");

            Assert.True(result, "Курс должен быть успешно удален");
        }
    }


    public class TeacherRegistrationTests
    {
        [Fact]
        public async Task TestTeacherCreateCourseSuccess()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var teacherService = new TeacherService();
            var registrationservice = new UserRegistrationService();

            Console.WriteLine($"\u001b[33mРегистрация пользователя как преподавателя для создания курса: {_testEmail}\u001b[0m");
            var reg = await registrationservice.RegisterClientAsync("Сергей", "Сергеев", _testEmail, _testPassword);

            var courseName = "Основы C#";
            var startDate = DateTime.Today.AddDays(1);
            var endDate = startDate.AddDays(14);
            var price = 1999.99m;

            Console.WriteLine($"\u001b[36mПопытка создать курс: '{courseName}', даты {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}, цена {price}\u001b[0m");
            var created = await teacherService.CreateCourseAsync(reg.ClientId, courseName, startDate, endDate, price);

            Console.WriteLine($"\u001b[36mРезультат создания курса: {created}\u001b[0m");
            Assert.True(created);
            Console.WriteLine("\u001b[32mКурс успешно создан (ожидалось true)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }

        [Fact]
        public async Task TestTeacherCreateCourseFail_InvalidName()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var teacherService = new TeacherService();

            var invalidName = "  ";
            var startDate = DateTime.Today.AddDays(1);
            var endDate = startDate.AddDays(7);
            var price = 1000m;

            Console.WriteLine($"\u001b[33mПопытка создать курс с некорректным именем: '{invalidName}'\u001b[0m");
            var created = await teacherService.CreateCourseAsync(1, invalidName, startDate, endDate, price);

            Console.WriteLine($"\u001b[36mРезультат создания курса с некорректными данными: {created}\u001b[0m");
            Assert.False(created);
            Console.WriteLine("\u001b[32mСоздание курса ожидаемо неуспешно (ожидалось false)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }
    }
}

