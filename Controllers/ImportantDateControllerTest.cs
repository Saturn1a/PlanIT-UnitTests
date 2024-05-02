using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Controllers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Services.Interfaces;
using System.Security.Claims;

namespace PlanITAPI_UnitTests.Controllers;

public class ImportantDateControllerTest
{
    private readonly Mock<IService<ImportantDateDTO>> _mockService;
    private readonly Mock<ILogger<ImportantDatesController>> _mockLogger;
    private readonly ImportantDatesController _controller;

    public ImportantDateControllerTest()
    {
        _mockService = new Mock<IService<ImportantDateDTO>>();
        _mockLogger = new Mock<ILogger<ImportantDatesController>>();
        _controller = new ImportantDatesController(_mockService.Object, _mockLogger.Object);

        // Mock user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),  
            new Claim(ClaimTypes.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Set HttpContext directly on ControllerContext
        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

        // Mimic Middleware logic: Ensure UserId is added as string to HttpContext.Items
        _controller.ControllerContext.HttpContext.Items["UserId"] = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [Fact]
    public async Task GetImportantDatesByIdAsync_ReturnsOkObject_WhenDateIsFound()
    {
        // Arrange
        var importantDate = new ImportantDateDTO(id: 1, userId: 1, name: "Meeting", date: DateOnly.FromDateTime(DateTime.Today));
        _mockService.Setup(x => x.GetByIdAsync(1, 1))  // Assuming GetByIdAsync takes userId and dateId
                    .ReturnsAsync(importantDate);

        // Act
        var result = await _controller.GetImportantDatesByIdAsync(1);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ImportantDateDTO>(actionResult.Value);
        Assert.Equal("Meeting", returnValue.Name);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task AddImportantDateAsync_ReturnsOkObject_WhenDateIsRegisteredSuccessfully()
    {
        // Arrange
        var newImportantDate = new ImportantDateDTO(id: 1, userId: 1, name: "Party", date: DateOnly.FromDateTime(DateTime.Today));
        _mockService.Setup(x => x.CreateAsync(1, newImportantDate)).ReturnsAsync(newImportantDate);

        // Act
        var result = await _controller.AddImportantDateAsync(newImportantDate);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ImportantDateDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<ImportantDateDTO>(okResult.Value);
        Assert.Equal("Party", returnValue.Name);
    }

    [Fact]
    public async Task UpdateImportantDateAsync_ReturnsOkObject_WhenDateIsUpdatedSuccessfully()
    {
        // Arrange
        var updatedImportantDate = new ImportantDateDTO(id: 1, userId: 1, name: "Updated Meeting", date: DateOnly.FromDateTime(DateTime.Today));
        _mockService.Setup(x => x.UpdateAsync(1, 1, updatedImportantDate)).ReturnsAsync(updatedImportantDate);

        // Act
        var result = await _controller.updateImportantDateAsync(1, updatedImportantDate);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ImportantDateDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<ImportantDateDTO>(okResult.Value);
        Assert.Equal("Updated Meeting", returnValue.Name);
    }

    [Fact]
    public async Task DeleteImportantDateAsync_ReturnsOkObject_WhenDateIsDeletedSuccessfully()
    {
        // Arrange
        var deletedImportantDate = new ImportantDateDTO(id: 1, userId: 1, name: "Deleted Birthday Party", date: DateOnly.FromDateTime(DateTime.Today));
        _mockService.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(deletedImportantDate);

        // Act
        var result = await _controller.DeleteImportantDateAsync(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ImportantDateDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<ImportantDateDTO>(okResult.Value);
        Assert.Equal("Deleted Birthday Party", returnValue.Name);
    }



}




