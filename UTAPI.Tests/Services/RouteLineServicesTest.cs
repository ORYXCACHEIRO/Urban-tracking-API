using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Requests.RouteLine;
using UTAPI.Services;
using UTAPI.Utils;
using Xunit;

public class RouteLineServicesTests
{
    private readonly DataContext _context;
    private readonly Mock<ILogger<RouteLineServices>> _loggerMock;
    private readonly IMapper _mapper;

    public RouteLineServicesTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostRouteLine, RouteLine>();
            cfg.CreateMap<RouteLine, ListRouteLine>();
        });

        _mapper = config.CreateMapper();
        _loggerMock = new Mock<ILogger<RouteLineServices>>();

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var route = new Route
        {
            Id = Guid.NewGuid(),
            Name = "Test Route",
            EntityId = Guid.NewGuid()
        };

        var entityDriver = new EntityDriver
        {
            UserId = Guid.NewGuid(),
            EntityId = route.EntityId
        };

        _context.Route.Add(route);
        _context.EntityDriver.Add(entityDriver);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateRouteLineAsync_RouteNotFound_ThrowsException()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var request = new PostRouteLine
        {
            RouteId = Guid.NewGuid(),
            FirstLat = 10.0,
            FirstLong = 20.0,
            SecondLat = 30.0,
            SecondLong = 40.0,
            Direction = 1,
            LineColor = "Blue"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateRouteLineAsync(Guid.NewGuid(), "3", request));
        Assert.Contains("An error occurred while creating the route line", ex.Message);
    }

    [Fact]
    public async Task CreateRouteLineAsync_UnauthorizedAccess_ThrowsException()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var route = _context.Route.First();
        var request = new PostRouteLine
        {
            RouteId = route.Id,
            FirstLat = 10.0,
            FirstLong = 20.0,
            SecondLat = 30.0,
            SecondLong = 40.0,
            Direction = 1,
            LineColor = "Blue"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UTAPI.Utils.UnauthorizedAccessException>(() => service.CreateRouteLineAsync(Guid.NewGuid(), "2", request));
        Assert.Equal("User is not authorized to create RouteLine for this Route.", ex.Message);
    }

    [Fact]
    public async Task DeleteRouteLineAsync_RouteLineNotFound_ThrowsException()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var invalidRouteLineId = Guid.NewGuid();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteRouteLineAsync(Guid.NewGuid(), "3", invalidRouteLineId));
        Assert.Contains("An error occurred while deleting the route line", ex.Message);
    }

    [Fact]
    public async Task GetByRouteIdAsync_NoRouteLines_ReturnsEmptyList()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var nonExistentRouteId = Guid.NewGuid();
        var filterQuery = new UTAPI.Types.FilterQuery(); // Ensure FilterQuery is not null

        // Act
        var result = await service.GetByRouteIdAsync(nonExistentRouteId, filterQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRouteLinesAsync_NoRouteLines_ReturnsEmptyList()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var filterQuery = new UTAPI.Types.FilterQuery(); // Ensure FilterQuery is not null

        // Act
        var result = await service.GetRouteLinesAsync(filterQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteRouteLineAsync_UnauthorizedAccess_ThrowsException()
    {
        // Arrange
        var service = new RouteLineServices(_context, _loggerMock.Object, _mapper);
        var route = _context.Route.First();
        var routeLine = new RouteLine
        {
            Id = Guid.NewGuid(),
            RouteId = route.Id,
            LineColor = "Blue"
        };
        _context.RouteLine.Add(routeLine);
        _context.SaveChanges();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UTAPI.Utils.UnauthorizedAccessException>(() =>
            service.DeleteRouteLineAsync(Guid.NewGuid(), "2", routeLine.Id));
        Assert.Equal("User is not authorized to delete this RouteLine.", ex.Message);
    }
}