using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Tourism.E2ETests;
    public class CityTourControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CityTourControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task City_Details_ShowsAvailableTours()
        {
            // Arrange
            int cityId;
            string cityName;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
                var city = dbContext.Cities.First();
                cityId = city.CityId;
                cityName = city.Name;
            }

            // Act
            var response = await _client.GetAsync($"/City/Details/{cityId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(cityName, content);
        }

        [Fact]
        public async Task Tour_Index_FiltersByDateRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(7);

            // Act
            var response = await _client.GetAsync($"/Tour/Index?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            // Перевіряємо, що сторінка завантажилась успішно
            Assert.Contains("Tour", content);
        }

        [Fact]
        public async Task Tour_Index_FiltersByCategory()
        {
            // Arrange
            int categoryId;
            string categoryName;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
                var category = dbContext.Categories.First();
                categoryId = category.CategoryId;
                categoryName = category.Name;
            }

            // Act
            var response = await _client.GetAsync($"/Tour/Index?categoryId={categoryId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(categoryName, content);
        }

        [Fact]
        public async Task Tour_Index_FiltersByPrice()
        {
            // Arrange
            int? targetPrice;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
                targetPrice = dbContext.Tours.First().Price;
            }

            // Act
            var response = await _client.GetAsync($"/Tour/Index?price={targetPrice}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(targetPrice.ToString(), content);
        }

        [Fact]
        public async Task Tour_Details_ShowsCorrectAvailableTickets()
        {
            // Arrange
            int tourId;
            int? availableTickets;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
                var tour = dbContext.Tours.First();
                tourId = tour.TourId;
                availableTickets = tour.AvaibleTickets;
            }

            // Act
            var response = await _client.GetAsync($"/Tour/Details/{tourId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(availableTickets.ToString(), content);
        }

        [Fact]
        public async Task Tour_Details_ReturnsNotFoundForInvalidId()
        {
            // Act
            var response = await _client.GetAsync("/Tour/Details/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }