using Xunit;
using Moq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Services;
using UTAPI.Interfaces;
using UTAPI.Requests.User;

namespace UTAPI.Tests
{
    public class UserServicesTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UserServices>> _loggerMock;
        private readonly Mock<IPasswordHelper> _passwordHelperMock;
        private readonly DataContext _context;

        public UserServicesTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserServices>>();
            _passwordHelperMock = new Mock<IPasswordHelper>();
        }

        [Fact]
        public async Task CreateDriverAsync_WithValidData_CreatesDriver()
        {
            // Arrange
            var userServices = new UserServices(_context, _loggerMock.Object, _mapperMock.Object, _passwordHelperMock.Object);
            var loggedInUserId = Guid.NewGuid();
            var entityId = Guid.NewGuid();

            // Set up entity admin
            await _context.User.AddAsync(new User { Id = loggedInUserId, Email = "admin@test.com", Role = "2", Name = "Admin", Password = "adminpassword" });
            await _context.EntityDriver.AddAsync(new EntityDriver { UserId = loggedInUserId, EntityId = entityId });
            await _context.SaveChangesAsync();

            var request = new PostUser { Email = "test1@test.com", Password = "password" };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, Name = "Teste", Password = "password" };
            var response = new OneUser { Id = user.Id, Email = user.Email };

            _mapperMock.Setup(m => m.Map<User>(request)).Returns(user);
            _mapperMock.Setup(m => m.Map<OneUser>(user)).Returns(response);
            _passwordHelperMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            // Act
            var result = await userServices.CreateDriverAsync(request, loggedInUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(request.Email, result.Email);
        }

        [Fact]
        public async Task CreateDriverAsync_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var userServices = new UserServices(_context, _loggerMock.Object, _mapperMock.Object, _passwordHelperMock.Object);
            var existingUser = new User { Email = "test@test.com", Name = "Existing User", Password = "password", Role = "1" };
            await _context.User.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var request = new PostUser { Email = "test@test.com" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                userServices.CreateDriverAsync(request, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteDriverAsync_WithValidData_DeletesDriver()
        {
            // Arrange
            var userServices = new UserServices(_context, _loggerMock.Object, _mapperMock.Object, _passwordHelperMock.Object);
            var entityId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var driverId = Guid.NewGuid();

            var admin = new User { Id = adminId, Role = "2", Email = "admin@test.com", Name = "Admin Name", Password = "adminpassword" };
            var driver = new User { Id = driverId, Role = "1", Email = "driver@test.com", Name = "Driver Name", Password = "driverpassword" };

            await _context.User.AddRangeAsync(admin, driver);
            await _context.EntityDriver.AddRangeAsync(
                new EntityDriver { UserId = adminId, EntityId = entityId },
                new EntityDriver { UserId = driverId, EntityId = entityId }
            );
            await _context.SaveChangesAsync();

            // Act
            await userServices.DeleteDriverAsync(driverId, adminId);

            // Assert
            var deletedDriver = await _context.User.FindAsync(driverId);
            Assert.Null(deletedDriver);
        }

        [Fact]
        public async Task GetDriverByIdAsync_WithValidData_ReturnsDriver()
        {
            // Arrange
            var userServices = new UserServices(_context, _loggerMock.Object, _mapperMock.Object, _passwordHelperMock.Object);
            var entityId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var driverId = Guid.NewGuid();

            var driver = new User { Id = driverId, Email = "driver@test.com", Name = "Driver Name", Password = "password", Role = "2" };
            await _context.User.AddAsync(driver);
            await _context.EntityDriver.AddRangeAsync(
                new EntityDriver { UserId = adminId, EntityId = entityId },
                new EntityDriver { UserId = driverId, EntityId = entityId }
            );
            await _context.SaveChangesAsync();

            var expectedResponse = new OneUser { Id = driverId, Email = "driver@test.com" };
            _mapperMock.Setup(m => m.Map<OneUser>(It.IsAny<User>())).Returns(expectedResponse);

            // Act
            var result = await userServices.GetDriverByIdAsync(driverId, adminId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(driverId, result.Id);
            Assert.Equal("driver@test.com", result.Email);
        }

        [Fact]
        public async Task UpdateDriverAsync_WithValidData_UpdatesDriver()
        {
            // Arrange
            var userServices = new UserServices(_context, _loggerMock.Object, _mapperMock.Object, _passwordHelperMock.Object);
            var entityId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var driverId = Guid.NewGuid();

            var driver = new User { Id = driverId, Name = "Old Name", Email = "old@test.com", Password = "oldpassword", Role = "1" };
            await _context.User.AddAsync(driver);
            await _context.EntityDriver.AddRangeAsync(
                new EntityDriver { UserId = adminId, EntityId = entityId },
                new EntityDriver { UserId = driverId, EntityId = entityId }
            );
            await _context.SaveChangesAsync();

            var request = new PatchUser { Name = "New Name", Email = "new@test.com" };
            _mapperMock.Setup(m => m.Map<PatchUser>(request)).Returns(request);

            // Act
            await userServices.UpdateDriverAsync(driverId, adminId, request);

            // Assert
            var updatedDriver = await _context.User.FindAsync(driverId);
            Assert.NotNull(updatedDriver);
            Assert.Equal("New Name", updatedDriver.Name);
            Assert.Equal("new@test.com", updatedDriver.Email);
        }
    }
}