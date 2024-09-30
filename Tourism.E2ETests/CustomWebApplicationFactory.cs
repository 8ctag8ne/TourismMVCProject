using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Tourism;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Tourism.E2ETests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Видаляємо реєстрацію реального контексту БД
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TourismDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Додаємо тестову in-memory базу даних
            services.AddDbContext<TourismDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestingDb");
            });

            // Створюємо і наповнюємо тестову БД
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            
            try
            {
                db.Database.EnsureCreated();
                SeedTestData(db, userManager);
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }

    private void SeedTestData(TourismDbContext context, UserManager<User> userManager)
    {
        // Додаємо тестові категорії
        var category = new Category
        {
            Name = "Test Category",
            Info = "Test Info"
        };
        context.Categories.Add(category);

        // Додаємо тестове місто
        var city = new City
        {
            Name = "Test City",
            Info = "Test City Info"
        };
        context.Cities.Add(city);
        context.SaveChanges();

        // Додаємо тестовий тур
        var tour = new Tour
        {
            Name = "Test Tour",
            Info = "Test Tour Info",
            CategoryId = category.CategoryId,
            CityId = city.CityId,
            Price = 100,
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(2),
            Capacity = 10,
            AvaibleTickets = 10
        };
        context.Tours.Add(tour);
        context.SaveChanges();
    }
}