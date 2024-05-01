using Microsoft.EntityFrameworkCore;
using PlanIT.API.Data;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories;
using PlanIT.API.Utilities;


namespace PlanITAPI_UnitTests.Repositories;


public class UserRepositoryTests : IDisposable
{
    private readonly PlanITDbContext _dbContext;
    private readonly UserRepository _userRepository;
    private readonly PaginationUtility _paginationUtility;

    public UserRepositoryTests()
    {
        // Initialize a unique database for each test instance
        _dbContext = CreateDbContext(Guid.NewGuid().ToString());
        _paginationUtility = new PaginationUtility(_dbContext);
        _userRepository = new UserRepository(_dbContext, _paginationUtility);
    }

    
    public void Dispose()
    {
        // Ensure cleanup of the database
        _dbContext.Database.EnsureDeletedAsync().Wait(); 
        _dbContext.Dispose();
    }

    private PlanITDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PlanITDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PlanITDbContext(options);
    }


    [Fact]
    public async Task AddAsync_AddsUserToDatabase()
    {
        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        var addedUser = await _userRepository.AddAsync(newUser);
        Assert.NotNull(addedUser);
        Assert.Equal(newUser.Name, addedUser.Name);
        Assert.Equal(newUser.Email, addedUser.Email);
    }


    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        var fetchedUser = await _userRepository.GetByIdAsync(newUser.Id);
        Assert.NotNull(fetchedUser);
        Assert.Equal(newUser.Id, fetchedUser.Id);
        Assert.Equal(newUser.Name, fetchedUser.Name);
        Assert.Equal(newUser.Email, fetchedUser.Email);
    }


    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
    {
        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        var fetchedUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
        Assert.NotNull(fetchedUser);
        Assert.Equal(newUser.Email, fetchedUser.Email);
        Assert.Equal(newUser.Name, fetchedUser.Name);
    }


    [Fact]
    public async Task UpdateAsync_UpdatesUserDetails_WhenUserExists()
    {
        var originalUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await _dbContext.Users.AddAsync(originalUser);
        await _dbContext.SaveChangesAsync();

        var updatedUser = new User { Id = originalUser.Id, Name = "Updated Name", Email = "updated@normann.com" };
        var resultUser = await _userRepository.UpdateAsync(originalUser.Id, updatedUser);

        Assert.NotNull(resultUser);
        Assert.Equal(updatedUser.Name, resultUser.Name);
        Assert.Equal(updatedUser.Email, resultUser.Email);

        var persistedUser = await _dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(persistedUser);
        Assert.Equal(updatedUser.Name, persistedUser.Name);
        Assert.Equal(updatedUser.Email, persistedUser.Email);
    }


    [Fact]
    public async Task UpdateAsync_DoesNotChangeUserCountOrAffectOtherUsers()
    {
        // Arrange
        var originalUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        var otherUser = new User { Name = "Ola Nordmann", Email = "ola@nordmann.com" };

        await _dbContext.Users.AddRangeAsync(new List<User> { originalUser, otherUser });
        await _dbContext.SaveChangesAsync();

        var updatedUser = new User { Id = originalUser.Id, Name = "Updated Name", Email = "updated@normann.com" };
        var initialCount = await _dbContext.Users.CountAsync();

        // Act
        await _userRepository.UpdateAsync(originalUser.Id, updatedUser);
        var newCount = await _dbContext.Users.CountAsync();
        var unchangedUser = await _dbContext.Users.FindAsync(otherUser.Id);

        // Assert
        Assert.Equal(initialCount, newCount);  // Check that user count remains the same
        Assert.NotNull(unchangedUser);
        Assert.Equal("Ola Nordmann", unchangedUser.Name);  // Confirm other user is unchanged
        Assert.Equal("ola@nordmann.com", unchangedUser.Email);
    }


    [Fact]
    public async Task DeleteAsync_DeletesUser_WhenUserExists()
    {
        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        var deletedUser = await _userRepository.DeleteAsync(newUser.Id);
        var fetchedUser = await _dbContext.Users.FindAsync(newUser.Id);

        Assert.NotNull(deletedUser);
        Assert.Null(fetchedUser);
    }


    [Fact]
    public async Task DeleteAsync_DeletesUser_AndDecreasesCountByOne()
    {
        var deleteUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        var keepUser = new User { Name = "Ola Nordmann", Email = "ola@nordmann.com" };

        await _dbContext.Users.AddRangeAsync(new List<User> { deleteUser, keepUser });
        await _dbContext.SaveChangesAsync();

        var initialCount = await _dbContext.Users.CountAsync();
        await _userRepository.DeleteAsync(deleteUser.Id);
        var newCount = await _dbContext.Users.CountAsync();
        var remainingUser = await _dbContext.Users.FindAsync(keepUser.Id);

        Assert.Equal(initialCount - 1, newCount);
        Assert.NotNull(remainingUser);
        Assert.Equal("Ola Nordmann", remainingUser.Name);
    }
}