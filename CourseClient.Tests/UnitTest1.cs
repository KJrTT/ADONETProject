using System;
using System.Data.Common;
using System.Threading.Tasks;
using CourseClient.Data;
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
            long createdUserId = 0;

            try
            {
                Console.WriteLine($"\u001b[33mТестирование регистрации с почтой {_testEmail}\u001b[0m");

                var result = await service.RegisterClientAsync("Иван", "Иванов", _testEmail, _testPassword);
                createdUserId = result.ClientId;
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
            finally
            {

                if (createdUserId > 0)
                {
                    await DeleteTestUser(createdUserId);
                }
                Console.WriteLine();
                Console.WriteLine("=============================");
            }
        }
        [Fact]
        public async Task TestLoginSuccess()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var authservice = new AuthService();
            var registrationservice = new UserRegistrationService();
            long createdUserId = 0;
            try
            {
                Console.WriteLine($"\u001b[33mРегистрация пользователя для теста логина: {_testEmail}\u001b[0m");
                await registrationservice.RegisterClientAsync("Петр", "Петров", _testEmail, _testPassword);

                Console.WriteLine($"\u001b[36mПопытка логина с email: {_testEmail}\u001b[0m");
                var result = await authservice.LoginAsync(_testEmail, _testPassword);
                createdUserId = result.UserId;
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
            finally
            {

                if (createdUserId > 0)
                {
                    await DeleteTestUser(createdUserId);
                }
                Console.WriteLine();
                Console.WriteLine("=============================");
            }
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

        private async Task DeleteTestUser(long userId)
        {
            try
            {
                using var db = new AppDbContext();

                var courses = await db.Courses.Where(c => c.UserID == userId).ToListAsync();
                if (courses.Any())
                {
                    db.Courses.RemoveRange(courses);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"\u001b[90mУдалено связанных курсов: {courses.Count}\u001b[0m");
                }

                
                var deleteduser = await db.Users.Where(u => u.UserId == userId).ExecuteDeleteAsync();

                Console.WriteLine($"\u001b[90mУдален тестовый пользователь с ID: {userId}\u001b[0m");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[31mОшибка при удалении тестового пользователя: {ex.Message}\u001b[0m");
            }
        }

        [Fact]
        public async Task TestUpdateUserRoleAsyncSuccess()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var createdUserId = 0;
            var adminService = new AdminService();
            var registrationService = new UserRegistrationService();
            using var db = new AppDbContext();
            try
            {
                
                var reguser = await registrationService.RegisterClientAsync("Петр", "Петров", _testEmail, _testPassword);
                createdUserId = reguser.ClientId;
                var checkuserid = await db.Users.Where(u => u.user_email == _testEmail).Select(u => u.UserId).FirstOrDefaultAsync();
                Console.WriteLine("Попытка обновить роль пользователя...");


                var result = await adminService.UpdateUserRoleAsync(checkuserid, 4);


                if (result)
                {
                    Console.WriteLine("✅ Роль пользователя успешно обновлена");
                    Console.WriteLine($"ID пользователя: {checkuserid}");
                    Console.WriteLine($"Новая роль: 4");
                }
                else
                {
                    Console.WriteLine("❌ Не удалось обновить роль пользователя");
                    Console.WriteLine($"Ошибка: {result}");
                }

                Console.WriteLine("=============================");
            }
            finally 
            {
                if (createdUserId > 0)
                {
                    await DeleteTestUser(createdUserId);
                }
                Console.WriteLine();
                Console.WriteLine("=============================");
            }

        }

        [Fact]
        public async Task TestDelCourseSuccess()
        {
            var service = new AdminService();
            var createdUserId = 0;
            using var db = new AppDbContext(); 
            var registrationservice = new UserRegistrationService();
            var teacherService = new TeacherService();
            
            Console.WriteLine();
            Console.WriteLine("=============================");
            try
            {
                var reguser = await registrationservice.RegisterClientAsync("Петр", "Петров", _testEmail, _testPassword);
                createdUserId = reguser.ClientId;
                var checkuserid = await db.Users.Where(u => u.user_email == _testEmail).Select(u => u.UserId).FirstOrDefaultAsync();

                var courseName = "Основы C#";
                var startDate = DateTime.Today.AddDays(1);
                var endDate = startDate.AddDays(14);
                var price = 1999.99m;
                var created = await teacherService.CreateCourseAsync(checkuserid, courseName, startDate, endDate, price);
                var createdCourseId = await db.Courses.Where(c => c.UserID == checkuserid).Select(c => c.CourseId).FirstOrDefaultAsync();

                Console.WriteLine("🧪 Тестирование удаления курса");
                Console.WriteLine("=================================");
                Console.WriteLine($"📚 ID курса для удаления: {createdCourseId}");


                Console.WriteLine("🔄 Выполнение удаления...");
                bool result = await service.DelCourse(createdCourseId);


                Console.WriteLine("📊 Результат выполнения:");

                if (result)
                {
                    Console.WriteLine("✅ УДАЛЕНИЕ УСПЕШНО");
                    Console.WriteLine($"💡 Курс с ID {createdCourseId} был удален из системы");
                }
                else
                {
                    Console.WriteLine("❌ УДАЛЕНИЕ НЕ УДАЛОСЬ");
                    Console.WriteLine($"⚠️ Курс с ID {createdCourseId} не был найден или произошла ошибка");
                    Console.WriteLine($"{result}");
                }

                Console.WriteLine("=================================");

                Assert.True(result, "Курс должен быть успешно удален");
            }
            finally
            {
                if (createdUserId > 0)
                {
                    await DeleteTestUser(createdUserId);
                }
                Console.WriteLine();
                Console.WriteLine("=============================");
            }
        }


        [Fact]
        public async Task TestTeacherCreateCourseSuccess()
        {
            Console.WriteLine();
            Console.WriteLine("=============================");
            var createdUserId = 0;
            using var db = new AppDbContext(); 
            var teacherService = new TeacherService();
            var registrationservice = new UserRegistrationService();


            try
            {
                var reguser = await registrationservice.RegisterClientAsync("Петр", "Петров", _testEmail, _testPassword);
                createdUserId = reguser.ClientId;
                var checkuserid = await db.Users.Where(u => u.user_email == _testEmail).Select(u => u.UserId).FirstOrDefaultAsync();



                var courseName = "Основы C#";
                var startDate = DateTime.Today.AddDays(1);
                var endDate = startDate.AddDays(14);
                var price = 1999.99m;

                Console.WriteLine($"\u001b[36mПопытка создать курс: '{courseName}', даты {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}, цена {price}\u001b[0m");
                var created = await teacherService.CreateCourseAsync(checkuserid, courseName, startDate, endDate, price);

                Console.WriteLine($"\u001b[36mРезультат создания курса: {created}\u001b[0m");
                Assert.True(created.Success);
                Console.WriteLine("\u001b[32mКурс успешно создан (ожидалось true)\u001b[0m");
                Console.WriteLine();
                Console.WriteLine("=============================");
            }
            finally 
            {
                if (createdUserId > 0)
                {
                    await DeleteTestUser(createdUserId);
                }
                Console.WriteLine();
                Console.WriteLine("=============================");
            }
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
            Assert.False(created.Success);
            Console.WriteLine("\u001b[32mСоздание курса ожидаемо неуспешно (ожидалось false)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("=============================");
        }
    }
}
