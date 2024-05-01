using Microsoft.EntityFrameworkCore;
using PlanIT.API.Data;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories;
using PlanIT.API.Utilities;


namespace PlanITAPI_UnitTests.Repositories;

public class TodoRepositoryTests : IDisposable
{
    private readonly PlanITDbContext _dbContext;
    private readonly TodoRepository _todoRepository;
    private readonly PaginationUtility _paginationUtility;

    public TodoRepositoryTests()
    {
        // Initialize a unique database for each test instance
        _dbContext = CreateDbContext(Guid.NewGuid().ToString());
        _paginationUtility = new PaginationUtility(_dbContext);
        _todoRepository = new TodoRepository(_dbContext, _paginationUtility);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
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
    public async Task AddAsync_AddsTodoToDatabase()
    {
        var newTodo = new ToDo { Name = "Test Todo" };

        var result = await _todoRepository.AddAsync(newTodo);

        Assert.NotNull(result);
        Assert.Equal("Test Todo", result.Name);
        Assert.True(result.Id > 0);
    }


    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        // Adding multiple todos
        _dbContext.Todos.AddRange(
            new ToDo { Name = "Todo 1" },
            new ToDo { Name = "Todo 2" },
            new ToDo { Name = "Todo 3" }
        );
        await _dbContext.SaveChangesAsync();

        var todos = await _todoRepository.GetAllAsync(1, 2);

        Assert.NotNull(todos);
        Assert.Equal(2, todos.Count);  // Assuming pagination correctly returns 2 items per page
    }


    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectTodo()
    {
        var todo = new ToDo { Name = "Test Todo" };
        await _dbContext.Todos.AddAsync(todo);
        await _dbContext.SaveChangesAsync();

        var foundTodo = await _todoRepository.GetByIdAsync(todo.Id);

        Assert.NotNull(foundTodo);
        Assert.Equal("Test Todo", foundTodo.Name);
    }


    [Fact]
    public async Task UpdateAsync_UpdatesTodoDetails()
    {
        var todo = new ToDo { Name = "Original Name" };
        await _dbContext.Todos.AddAsync(todo);
        await _dbContext.SaveChangesAsync();

        var updatedTodo = new ToDo { Name = "Updated Name" };
        var updatedResult = await _todoRepository.UpdateAsync(todo.Id, updatedTodo);

        Assert.NotNull(updatedResult);
        Assert.Equal("Updated Name", updatedResult.Name);
    }


    [Fact]
    public async Task DeleteAsync_DeletesTodo()
    {
        var todo = new ToDo { Name = "Test Todo" };
        await _dbContext.Todos.AddAsync(todo);
        await _dbContext.SaveChangesAsync();

        var deletedTodo = await _todoRepository.DeleteAsync(todo.Id);
        var result = await _dbContext.Todos.FindAsync(todo.Id);

        Assert.NotNull(deletedTodo);
        Assert.Null(result);  // The ToDo should no longer exist in the database
    }
}