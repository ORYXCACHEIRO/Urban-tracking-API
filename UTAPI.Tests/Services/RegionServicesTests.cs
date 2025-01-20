using Moq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Requests.Region;
using UTAPI.Services;
using Xunit;

namespace UTAPI.Tests
{
    public class RegionServicesTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<RegionServices>> _loggerMock;
        private readonly DataContext _context;

        public RegionServicesTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<RegionServices>>();
        }

        [Fact]
        public async Task CreateRegionAsync_WithValidData_CreatesRegion()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var request = new PostRegion { Name = "Test Region" };
            var region = new Region { Id = Guid.NewGuid(), Name = request.Name };
            var response = new OneRegion { Id = region.Id, Name = region.Name };

            _mapperMock.Setup(m => m.Map<Region>(request)).Returns(region);
            _mapperMock.Setup(m => m.Map<OneRegion>(region)).Returns(response);

            var result = await service.CreateRegionAsync(request);

            Assert.NotNull(result);
            Assert.Equal(region.Id, result.Id);
            Assert.Equal(request.Name, result.Name);
        }

        [Fact]
        public async Task CreateRegionAsync_WithDuplicateName_ThrowsException()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var existingRegion = new Region { Name = "Test Region" };
            await _context.Region.AddAsync(existingRegion);
            await _context.SaveChangesAsync();

            var request = new PostRegion { Name = "Test Region" };

            await Assert.ThrowsAsync<Exception>(() => service.CreateRegionAsync(request));
        }

        [Fact]
        public async Task DeleteRegionAsync_WithNoReferences_DeletesRegion()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var region = new Region { Id = Guid.NewGuid(), Name = "Test Region" };
            await _context.Region.AddAsync(region);
            await _context.SaveChangesAsync();

            await service.DeleteRegionAsync(region.Id);

            var deletedRegion = await _context.Region.FindAsync(region.Id);
            Assert.Null(deletedRegion);
        }

        [Fact]
        public async Task DeleteRegionAsync_WithRouteReferences_ThrowsException()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var region = new Region { Id = Guid.NewGuid(), Name = "Test Region" };
            await _context.Region.AddAsync(region);
            var route = new Route { Id = Guid.NewGuid(), RegionId = region.Id, Name = "Test Route" };
            await _context.Route.AddAsync(route);
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<Exception>(() => service.DeleteRegionAsync(region.Id));
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsRegion()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var region = new Region { Id = Guid.NewGuid(), Name = "Test Region" };
            await _context.Region.AddAsync(region);
            await _context.SaveChangesAsync();

            var expectedResponse = new OneRegion { Id = region.Id, Name = region.Name };
            _mapperMock.Setup(m => m.Map<OneRegion>(It.IsAny<Region>())).Returns(expectedResponse);

            var result = await service.GetByIdAsync(region.Id);

            Assert.NotNull(result);
            Assert.Equal(region.Id, result.Id);
            Assert.Equal(region.Name, result.Name);
        }

        [Fact]
        public async Task UpdateRegionAsync_WithValidData_UpdatesRegion()
        {
            var service = new RegionServices(_context, _loggerMock.Object, _mapperMock.Object);
            var region = new Region { Id = Guid.NewGuid(), Name = "Old Name" };
            await _context.Region.AddAsync(region);
            await _context.SaveChangesAsync();

            var request = new PatchRegion { Name = "New Name" };
            _mapperMock.Setup(m => m.Map<PatchRegion>(request)).Returns(request);

            await service.UpdateRegionAsync(region.Id, request);

            var updatedRegion = await _context.Region.FindAsync(region.Id);
            Assert.NotNull(updatedRegion);
            Assert.Equal("New Name", updatedRegion.Name);
        }
    }
}