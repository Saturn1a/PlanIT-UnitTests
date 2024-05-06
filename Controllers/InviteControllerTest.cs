using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Controllers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Services.Interfaces;
using System.Security.Claims;

namespace PlanITAPI_UnitTests.Controllers;

public class InviteControllerTest
{
    private readonly Mock<IInviteService> _mockService;
    private readonly Mock<ILogger<InvitesController>> _mockLogger;
    private readonly InvitesController _controller;

    public InviteControllerTest()
    {
        _mockService = new Mock<IInviteService>();
        _mockLogger = new Mock<ILogger<InvitesController>>();
        _controller = new InvitesController(_mockService.Object, _mockLogger.Object);

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
    public async Task AddInviteAsync_ReturnsBadRequest_WhenRegistrationFails()
    {
        var newInvite = new InviteDTO(1,1,"Jane Doe", "janedoe@mail.com", true); 
        _mockService.Setup(x => x.CreateAsync(It.IsAny<int>(), newInvite)).ReturnsAsync((InviteDTO?)null);

        var result = await _controller.AddInviteAsync(newInvite);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetInvitesByIdAsync_ReturnsNotFound_WhenInviteDoesNotExist()
    {
        _mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((InviteDTO?)null);

        var result = await _controller.GetInvitesByIdASync(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task AddInviteAsync_ReturnsOk_WhenRegistrationIsSuccessful()
    {
        var newInvite = new InviteDTO(1, 1, "Jane Doe", "janedoe@mail.com", true); 
        _mockService.Setup(x => x.CreateAsync(It.IsAny<int>(), newInvite)).ReturnsAsync(new InviteDTO(1, 1, "Jane Doe", "janedoe@mail.com", true)); 

        var result = await _controller.AddInviteAsync(newInvite);

        Assert.IsType<OkObjectResult>(result.Result);
    }


    [Fact]
    public async Task DeleteInviteAsync_ReturnsOk_WhenDeletionIsSuccessful()
    {
        _mockService.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new InviteDTO(1, 1, "Jane Doe", "janedoe@mail.com", true));

        var result = await _controller.DeleteInviteAsync(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }







}
