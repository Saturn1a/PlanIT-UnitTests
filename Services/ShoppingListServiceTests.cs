using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Mappers.Interface;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories.Interfaces;
using PlanIT.API.Services;
using PlanIT.API.Utilities;

namespace PlanITAPI_UnitTests.Services;

public class ShoppingListServiceTests
{
    private readonly Mock<IRepository<ShoppingList>> _mockRepo = new Mock<IRepository<ShoppingList>>();
    private readonly Mock<IMapper<ShoppingList, ShoppingListDTO>> _mockMapper = new Mock<IMapper<ShoppingList, ShoppingListDTO>>();
    private readonly Mock<ILogger<LoggerService>> _mockLogger = new Mock<ILogger<LoggerService>>();
    private readonly LoggerService _loggerService;
    private readonly ShoppingListService _service;

    public ShoppingListServiceTests()
    {
        _loggerService = new LoggerService(_mockLogger.Object);
        _service = new ShoppingListService(_mockRepo.Object, _mockMapper.Object, _loggerService);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedShoppingList_AndLogsAppropriately()
    {
        // Arrange
        int userId = 1, shoppingListId = 1;
        var shoppingListDTO = new ShoppingListDTO(shoppingListId, userId, "Updated Shopping List");
        var existingShoppingList = new ShoppingList { Id = shoppingListId, UserId = userId, Name = "Shopping List" };

        _mockRepo.Setup(r => r.GetByIdAsync(shoppingListId)).ReturnsAsync(existingShoppingList);
        _mockRepo.Setup(r => r.UpdateAsync(shoppingListId, It.IsAny<ShoppingList>())).ReturnsAsync(existingShoppingList);
        _mockMapper.Setup(m => m.MapToModel(It.IsAny<ShoppingListDTO>())).Returns(existingShoppingList);
        _mockMapper.Setup(m => m.MapToDTO(It.IsAny<ShoppingList>())).Returns(shoppingListDTO);

        // Act
        var result = await _service.UpdateAsync(userId, shoppingListId, shoppingListDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shoppingListDTO.Name, result.Name);

        // Check for correct log message
        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Updating shoppinglist item with ID {shoppingListId} for user {userId}.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }


    [Fact]
    public async Task GetByIdAsync_UnauthorizedUser_ThrowsUnauthorizedException_AndLogsAppropriately()
    {
        // Arrange
        int authorizedUserId = 1, unauthorizedUserId = 2, shoppingListId = 1;
        var shoppingList = new ShoppingList { Id = shoppingListId, UserId = authorizedUserId, Name = "Private Shopping List" };

        _mockRepo.Setup(r => r.GetByIdAsync(shoppingListId)).ReturnsAsync(shoppingList);

        // Act 
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetByIdAsync(unauthorizedUserId, shoppingListId));

       // Assert
        Assert.Equal("Access denied for shopping list ID 1.", ex.Message);

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Unauthorized attempt to access shopping list with ID {shoppingListId} by user ID {unauthorizedUserId}.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving shoppinglist item with ID {shoppingListId} for user {unauthorizedUserId}.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

        // Ensure no success log is written
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("retrieved")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_UnauthorizedUser_ThrowsUnauthorizedException_AndLogsAppropriately()
    {
        // Arrange
        int authorizedUserId = 1, unauthorizedUserId = 2, shoppingListId = 1;
        var shoppingList = new ShoppingList { Id = shoppingListId, UserId = authorizedUserId, Name = "Private Shopping List" };

        _mockRepo.Setup(r => r.GetByIdAsync(shoppingListId)).ReturnsAsync(shoppingList);

        // Act 
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteAsync(unauthorizedUserId, shoppingListId));

        // Assert
        Assert.Equal("Access denied for shopping list ID 1.", exception.Message);

        // Verify for correct log messages
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthorized attempt to access shopping list with ID 1 by user ID 2")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting shoppinglist item with ID 1 for user 2")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

        
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
    }













}
