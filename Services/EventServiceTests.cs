using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Mappers.Interface;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories.Interfaces;
using PlanIT.API.Services;
using PlanIT.API.Utilities;

namespace PlanITAPI_UnitTests.Services;

public class EventServiceTests
{
    private readonly Mock<IRepository<Event>> _mockRepo = new Mock<IRepository<Event>>();
    private readonly Mock<IMapper<Event, EventDTO>> _mockMapper = new Mock<IMapper<Event, EventDTO>>();
    private readonly Mock<ILogger<LoggerService>> _mockLogger = new Mock<ILogger<LoggerService>>();
    private readonly LoggerService _loggerService;
    private readonly EventService _service;

    public EventServiceTests()
    {
        
        _loggerService = new LoggerService(_mockLogger.Object);
        _service = new EventService(_mockMapper.Object, _mockRepo.Object, _loggerService);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedEvent()
    {
        // Arrange
        int userId = 1;
        var newEventDTO = new EventDTO(0, userId, "new Event", new DateOnly(2024, 5, 1), new TimeOnly(10, 0), "Location");
        var newEvent = new Event { UserId = userId, Name = "New Event" };

        _mockMapper.Setup(m => m.MapToModel(It.IsAny<EventDTO>())).Returns(newEvent);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Event>())).ReturnsAsync(newEvent);
        _mockMapper.Setup(m => m.MapToDTO(It.IsAny<Event>())).Returns(newEventDTO);

        var service = new EventService(_mockMapper.Object, _mockRepo.Object, _loggerService);

        // Act
        var result = await service.CreateAsync(userId, newEventDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newEventDTO, result);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting to create a new event.")),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("event with ID")),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
        Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEvents_AndLogsDebugMessage()
    {
        // Arrange
        var userId = 1;
        var pageNr = 1;
        var pageSize = 10;
        var eventsFromRepository = new List<Event>
    {
        new Event { Id = 1, UserId = userId, Name = "Event 1" },
        new Event { Id = 2, UserId = userId, Name = "Event 2" },
        new Event { Id = 3, UserId = userId + 1, Name = "Event 3" }
    };

        var expectedDtos = eventsFromRepository
            .Where(e => e.UserId == userId)
            .Select(e => new EventDTO(e.Id, e.UserId, e.Name, new DateOnly(2024, 5, 1), new TimeOnly(10, 0), "Location"))
            .ToList();

        _mockRepo.Setup(r => r.GetAllAsync(pageNr, pageSize)).ReturnsAsync(eventsFromRepository);
        _mockMapper.Setup(m => m.MapToDTO(It.IsAny<Event>())).Returns((Event e) => new EventDTO(e.Id, e.UserId, e.Name, new DateOnly(2024, 5, 1), new TimeOnly(10, 0), "Location"));

        var service = new EventService(_mockMapper.Object, _mockRepo.Object, _loggerService);

        // Act
        var result = await service.GetAllAsync(userId, pageNr, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); 
        Assert.All(result, dto => Assert.Equal(userId, dto.UserId));

        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<object>(o => o.ToString()!.Contains($"Retrieving all events for user {userId}.")),
            null,
            (Func<object, Exception?, string>)It.IsAny<object>()),
        Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_ValidData_ReturnsDeletedEvent()
    {
        // Arrange
        int userId = 1, eventId = 1;
        var eventToDelete = new Event { Id = eventId, UserId = userId, Name = "Test Event" };
        var eventDTO = new EventDTO(eventId, userId, "Test Event", DateOnly.FromDateTime(DateTime.Now), TimeOnly.FromDateTime(DateTime.Now), "Location");

        _mockRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(eventToDelete);
        _mockRepo.Setup(r => r.DeleteAsync(eventId)).ReturnsAsync(eventToDelete);  
        _mockMapper.Setup(m => m.MapToDTO(It.IsAny<Event>())).Returns(eventDTO);

        _mockLogger.Setup(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains($"Deleting dinner with ID {eventId} for user {userId}.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()  
        ));

        _mockLogger.Setup(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"event with ID {eventId} deleted successfully.")),  
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>() 
        ));

        // Act
        var result = await _service.DeleteAsync(userId, eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventDTO, result);  

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<object>(o => o.ToString()!.Contains($"Deleting dinner with ID {eventId} for user {userId}.")),
            null,
            (Func<object, Exception?, string>)It.IsAny<object>()
        ), Times.Once);

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<object>(o => o.ToString()!.Contains($"event with ID {eventId} deleted successfully.")),
            null,
            (Func<object, Exception?, string>)It.IsAny<object>()
        ), Times.Once);

        _mockRepo.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }


}
