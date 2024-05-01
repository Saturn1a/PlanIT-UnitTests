using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Controllers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Services.Interfaces;
using System.Security.Claims;

namespace PlanITAPI_UnitTests.Controllers;

public class ShoppingListControllerTest
{
    private readonly Mock<IService<ShoppingListDTO>> _mockService;
    private readonly Mock<ILogger<ShoppingListsController>> _mockLogger;
    private readonly ShoppingListsController _controller;

    public ShoppingListControllerTest()
    {
        _mockService = new Mock<IService<ShoppingListDTO>>();
        _mockLogger = new Mock<ILogger<ShoppingListsController>>();
        _controller = new ShoppingListsController(_mockLogger.Object, _mockService.Object);

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
    public async Task AddShoppingListAsync_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var newShoppingList = new ShoppingListDTO(1, 1, "milk"); 
        _mockService.Setup(x => x.CreateAsync(It.IsAny<int>(), newShoppingList)).ReturnsAsync((ShoppingListDTO?)null);

        // Act
        var result = await _controller.AddShoppingListAsync(newShoppingList);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetShoppingListByIdAsync_ReturnsNotFound_WhenListDoesNotExist()
    {
        // Arrange
        _mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((ShoppingListDTO?)null);

        // Act
        var result = await _controller.GetShoppingListByIdAsync(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ShoppingListDTO>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }



    [Fact]
    public async Task UpdateShoppingListAsync_ReturnsNotFound_WhenListDoesNotExist()
    {
        // Arrange
        var updatedShoppingList = new ShoppingListDTO(1,1, "milk"); // Minimal details assumed
        _mockService.Setup(x => x.UpdateAsync(1, 1, updatedShoppingList)).ReturnsAsync((ShoppingListDTO?)null);  

        // Act
        var result = await _controller.UpdateShoppingListAsync(1, updatedShoppingList);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ShoppingListDTO>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task DeleteShoppingListAsync_ReturnsNotFound_WhenListDoesNotExist()
    {
        // Arrange
        _mockService.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync((ShoppingListDTO?)null);  

        // Act
        var result = await _controller.DeleteShoppingListAsync(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ShoppingListDTO>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }





}
