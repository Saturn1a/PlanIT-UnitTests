using Microsoft.EntityFrameworkCore;
using PlanIT.API.Data;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories;
using PlanIT.API.Utilities;


namespace PlanITAPI_UnitTests.Repositories;

public class UserRepositoryTests
{
    private PlanITDbContext CreateDbContext(string dbName)
    {
        // Oppretter alternativer for in-memory database
        var options = new DbContextOptionsBuilder<PlanITDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        // Returnerer en ny database kontekst instans
        return new PlanITDbContext(options);
    }


    [Fact]
    public async Task AddAsync_AddsUserToDatabase()
    {
        // Arrange
        var dbContext = CreateDbContext("AddUserTestDb");
        var paginationUtility = new PaginationUtility(dbContext);
        var userRepository = new UserRepository(dbContext, paginationUtility);
        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };

        // Act
        var addedUser = await userRepository.AddAsync(newUser);

        // Assert
        Assert.NotNull(addedUser);
        Assert.Equal(newUser.Name, addedUser.Name);
        Assert.Equal(newUser.Email, addedUser.Email);

        // Rensker opp in-memory databasen
        await dbContext.Database.EnsureDeletedAsync();
    }


    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var dbContext = CreateDbContext("GetByIdTestDb");
        var paginationUtility = new PaginationUtility(dbContext);
        var userRepository = new UserRepository(dbContext, paginationUtility);

        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await dbContext.Users.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        // Act
        var fetchedUser = await userRepository.GetByIdAsync(newUser.Id);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(newUser.Id, fetchedUser.Id);
        Assert.Equal(newUser.Name, fetchedUser.Name);
        Assert.Equal(newUser.Email, fetchedUser.Email);
    }


    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var dbContext = CreateDbContext("GetUserByEmailTestDb");
        var paginationUtility = new PaginationUtility(dbContext);
        var userRepository = new UserRepository(dbContext, paginationUtility);

        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await dbContext.Users.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        // Act
        var fetchedUser = await userRepository.GetUserByEmailAsync(newUser.Email);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(newUser.Email, fetchedUser.Email);
        Assert.Equal(newUser.Name, fetchedUser.Name);
    }



    [Fact]
    public async Task DeleteAsync_DeletesUser_WhenUserExists()
    {
        // Arrange
        var dbContext = CreateDbContext("DeleteUserTestDb");
        var paginationUtility = new PaginationUtility(dbContext);
        var userRepository = new UserRepository(dbContext, paginationUtility);

        var newUser = new User { Name = "Kari Normann", Email = "kari@normann.com" };
        await dbContext.Users.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        // Act
        var deletedUser = await userRepository.DeleteAsync(newUser.Id);
        var fetchedUser = await dbContext.Users.FindAsync(newUser.Id);

        // Assert
        Assert.NotNull(deletedUser);
        Assert.Null(fetchedUser);
    }

}
