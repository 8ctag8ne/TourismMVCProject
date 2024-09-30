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
public class TourismControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TourismControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Categories_Index_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/Category/Index");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task Category_Details_ReturnsNotFoundForInvalidId()
    {
        // Act
        var response = await _client.GetAsync("/Category/Details/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Tour_Index_FiltersCorrectlyBySearchString()
    {
        // Act
        var response = await _client.GetAsync("/Tour/Index?searchString=Test+Tour");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Tour", content);
    }

    [Fact]
    public async Task City_Index_ReturnsSuccessAndContainsTestCity()
    {
        // Act
        var response = await _client.GetAsync("/City/Index");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test City", content);
    }

    [Fact]
    public async Task Tour_Details_ReturnsCorrectTourInfo()
    {
        // Arrange
        int tourId;
        string expectedName;
        string expectedInfo;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
            var tour = dbContext.Tours.FirstOrDefault() 
                ?? throw new InvalidOperationException("No test tours found in database");
            
            tourId = tour.TourId;
            expectedName = tour.Name;
            expectedInfo = tour.Info;
        }

        // Act
        var response = await _client.GetAsync($"/Tour/Details/{tourId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains(expectedName, content);
        Assert.Contains(expectedInfo, content);
    }
}