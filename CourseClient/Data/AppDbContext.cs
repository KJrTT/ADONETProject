using System;
using CourseClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CourseClient.Data
{
    public class AppDbContext : DbContext
    {


        static string connectionString = "Server=192.168.9.203\\sqlexpress;Database=ErofeevKirill;User Id=student1;Password=123456;Encrypt=false;";

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<LevelAccess> LevelAccess { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(connectionString)
                    .LogTo(message => { },
                          new[] { DbLoggerCategory.Database.Command.Name },
                          LogLevel.None) 
                    .LogTo(Console.WriteLine, LogLevel.Error); 
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LevelAccess
            modelBuilder.Entity<LevelAccess>(entity =>
            {
                entity.HasKey(la => la.AccessId);
                entity.Property(la => la.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.HasIndex(la => la.Title).IsUnique();
            });

            // Wallet
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.WalletId);
                entity.Property(w => w.balance)
                      .IsRequired()
                      .HasPrecision(10, 2)
                      .HasDefaultValue(0);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.first_name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(u => u.last_name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(u => u.password)
                      .IsRequired()
                      .HasMaxLength(300);

                entity.Property(u => u.user_email)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.HasIndex(u => u.user_email).IsUnique();

                // Связи
                entity.HasOne(u => u.LevelAccess)
                      .WithMany(la => la.Users)
                      .HasForeignKey(u => u.level_access_id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Wallet)
                      .WithOne(w => w.User)
                      .HasForeignKey<User>(u => u.wallet_id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.CourseId);

                entity.Property(c => c.name_course)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(c => c.price)
                      .HasPrecision(10, 2);

                // Связь с User (преподаватель)
                entity.HasOne(c => c.User)
                      .WithMany(u => u.CreatedCourses)
                      .HasForeignKey(c => c.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // UserCourse (связь многие-ко-многим)
            modelBuilder.Entity<UserCourse>(entity =>
            {
                entity.HasKey(uc => uc.UserCourseId);

                // Составной уникальный ключ
                entity.HasIndex(uc => new { uc.CourseId, uc.UserId }).IsUnique();

                // Связи
                entity.HasOne(uc => uc.Course)
                      .WithMany(c => c.UserCourses)
                      .HasForeignKey(uc => uc.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uc => uc.User)
                      .WithMany(u => u.UserCourses)
                      .HasForeignKey(uc => uc.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            //SeedData(modelBuilder);
        }

        //private void SeedData(ModelBuilder modelBuilder)
        //{
        //    // LevelAccess
        //    modelBuilder.Entity<LevelAccess>().HasData(
        //        new LevelAccess { AccessId = 1, Title = "Admin" },
        //        new LevelAccess { AccessId = 2, Title = "Teacher" },
        //        new LevelAccess { AccessId = 3, Title = "Student" }
        //    );

        //    // Wallets
        //    modelBuilder.Entity<Wallet>().HasData(
        //        new Wallet { WalletId = 1, Balance = 1000.00m },
        //        new Wallet { WalletId = 2, Balance = 500.00m },
        //        new Wallet { WalletId = 3, Balance = 300.00m }
        //    );

        //    // Users
        //    modelBuilder.Entity<User>().HasData(
        //        new User
        //        {
        //            UserId = 1,
        //            FirstName = "Admin",
        //            LastName = "User",
        //            Password = "admin123",
        //            UserEmail = "admin@school.com",
        //            LevelAccessId = 1,
        //            WalletId = 1
        //        },
        //        new User
        //        {
        //            UserId = 2,
        //            FirstName = "John",
        //            LastName = "Teacher",
        //            Password = "teacher123",
        //            UserEmail = "john@school.com",
        //            LevelAccessId = 2,
        //            WalletId = 2
        //        },
        //        new User
        //        {
        //            UserId = 3,
        //            FirstName = "Alice",
        //            LastName = "Student",
        //            Password = "student123",
        //            UserEmail = "alice@school.com",
        //            LevelAccessId = 3,
        //            WalletId = 3
        //        }
        //    );

        //    // Courses
        //    modelBuilder.Entity<Course>().HasData(
        //        new Course
        //        {
        //            CourseId = 1,
        //            NameCourse = "C# Programming",
        //            DataStart = new DateTime(2024, 1, 15),
        //            DataEnd = new DateTime(2024, 6, 15),
        //            Price = 299.99m,
        //            UserId = 2
        //        },
        //        new Course
        //        {
        //            CourseId = 2,
        //            NameCourse = "Web Development",
        //            DataStart = new DateTime(2024, 2, 1),
        //            DataEnd = new DateTime(2024, 7, 1),
        //            Price = 399.99m,
        //            UserId = 2
        //        }
        //    );

        //    // UserCourses
        //    modelBuilder.Entity<UserCourse>().HasData(
        //        new UserCourse { Id = 1, CourseId = 1, UserId = 3 },
        //        new UserCourse { Id = 2, CourseId = 2, UserId = 3 }
        //    );
        //}
    }
}
