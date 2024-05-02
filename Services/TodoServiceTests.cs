using Moq;
using PlanIT.API.Models.Entities;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Repositories.Interfaces;
using PlanIT.API.Mappers.Interface;
using PlanIT.API.Utilities;
using Microsoft.Extensions.Logging; 
using PlanIT.API.Services;

namespace PlanITAPI_UnitTests.Services;

public class TodoServiceTests
{
    private readonly Mock<IRepository<ToDo>> _mockRepo = new Mock<IRepository<ToDo>>();
    private readonly Mock<IMapper<ToDo, ToDoDTO>> _mockMapper = new Mock<IMapper<ToDo, ToDoDTO>>();
    private readonly Mock<ILogger<LoggerService>> _mockLogger = new Mock<ILogger<LoggerService>>();
    private readonly LoggerService _loggerService;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        
        _loggerService = new LoggerService(_mockLogger.Object);
        _service = new TodoService(_mockRepo.Object, _mockMapper.Object, _loggerService);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedTodo()
    {
        // Arrange
        var userId = 1;
        var dto = new ToDoDTO(0, userId, "Test ToDo");
        var todo = new ToDo { UserId = userId, Name = "Test ToDo" };

        _mockMapper.Setup(m => m.MapToModel(It.IsAny<ToDoDTO>())).Returns(todo);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<ToDo>())).ReturnsAsync(todo);
        _mockMapper.Setup(m => m.MapToDTO(It.IsAny<ToDo>())).Returns(dto);

        // Act
        var result = await _service.CreateAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto, result);

        // confirms that creating logging and sucess is logged once and error is not logged 
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting to create a new todo.")),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("todo with ID")),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create todo.")),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Never);

        _mockRepo.Verify(r => r.AddAsync(It.Is<ToDo>(t => t.UserId == userId && t.Name == dto.Name)), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTodosForUser_AndLogsDebugMessage()
    {
        // Arrange
        var userId = 1;
        var pageNr = 1;
        var pageSize = 10;
        var todosFromRepository = new List<ToDo>
    {
        new ToDo { Id = 1, UserId = userId, Name = "ToDo 1" },
        new ToDo { Id = 2, UserId = userId, Name = "ToDo 2" },
        new ToDo { Id = 3, UserId = userId + 1, Name = "ToDo 3" } 
    };

        var expectedDtos = todosFromRepository
            .Where(todo => todo.UserId == userId)
            .Select(todoEntity => new ToDoDTO(todoEntity.Id, todoEntity.UserId, todoEntity.Name))
            .ToList();

        _mockRepo.Setup(r => r.GetAllAsync(pageNr, pageSize)).ReturnsAsync(todosFromRepository);
        _mockMapper
            .Setup(m => m.MapToDTO(It.IsAny<ToDo>()))
            .Returns((ToDo todo) => new ToDoDTO(todo.Id, todo.UserId, todo.Name));

        // Act
        var result = await _service.GetAllAsync(userId, pageNr, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only todos for the given user ID should be returned
        Assert.All(result, dto => Assert.Equal(userId, dto.UserId)); // Ensure all returned todos have the correct user ID

        // Verify that the debug message was logged with the correct user ID
        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<object>(o => o.ToString()!.Contains($"Retrieving all todos for user {userId}.")),
            null,
            (Func<object, Exception?, string>)It.IsAny<object>()),
        Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_LogsAppropriateMessages()
    {
        // Arrange
        int userId = 1, todoId = 1;
        var todo = new ToDo { Id = todoId, UserId = userId, Name = "Test ToDo" };
        var expectedDto = new ToDoDTO(todoId, userId, "Test ToDo");

        var mockRepo = new Mock<IRepository<ToDo>>();
        var mockMapper = new Mock<IMapper<ToDo, ToDoDTO>>();
        var mockLogger = new Mock<ILogger<LoggerService>>();

        
        mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
        mockMapper.Setup(m => m.MapToDTO(todo)).Returns(expectedDto);

        var loggerService = new LoggerService(mockLogger.Object);

        var service = new TodoService(mockRepo.Object, mockMapper.Object, loggerService);

        // Act
        var result = await service.GetByIdAsync(userId, todoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto, result);

        // Confirms that retriving and sucess is logged once and error is not logged. 
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving todo with ID {todoId} for user {userId}")),
            null,
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"todo with ID {todoId} retrieved successfully")),
            null,
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

       
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Never);
    }


    // checks that unauthorized user is not allowed to get by ID. 
    [Fact]
    public async Task GetByIdAsync_LogsUnauthorizedAndThrows_WhenUserIsNotOwner()
    {
        // Arrange
        int userId = 1, todoId = 1, wrongUserId = 2;
        var todo = new ToDo { Id = todoId, UserId = wrongUserId, Name = "Test ToDo" };

        var mockRepo = new Mock<IRepository<ToDo>>();
        var mockMapper = new Mock<IMapper<ToDo, ToDoDTO>>();
        var mockLogger = new Mock<ILogger<LoggerService>>();

        mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);

        var loggerService = new LoggerService(mockLogger.Object);

        var service = new TodoService(mockRepo.Object, mockMapper.Object, loggerService);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByIdAsync(userId, todoId));

        // Assert


        // Unauthorized should be logged once, sucess and Retrieving should never be logged 
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Unauthorized attempt to access todo with ID {todoId} by user ID {userId}")),
            null,
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving todo with ID {todoId} for user {userId}")),
            null,
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"todo with ID {todoId} retrieved successfully")),
            null,
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Never);

        
        Assert.Equal("Access denied for todo ID 1.", exception.Message);
    }




}
