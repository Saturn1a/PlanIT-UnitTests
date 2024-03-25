using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Controllers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories.Interfaces;
using PlanIT.API.Services.AuthenticationService;
using PlanIT.API.Services.Interfaces;


namespace PlanITAPI_UnitTests.Services;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task AuthenticateUserAsync_ValidCredentials_ReturnsUserAndToken_AndLogs()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        configurationMock.SetupGet(x => x["Jwt:Secret"]).Returns("secret_key_for_development_purpose_only");
        configurationMock.SetupGet(x => x["Jwt:Issuer"]).Returns("https://localhost:7019");
        configurationMock.SetupGet(x => x["Jwt:KeySizeInBits"]).Returns("256");
        configurationMock.SetupGet(x => x["Jwt:ExpiryInMinutes"]).Returns("60");

        var authService = new AuthenticationService(userRepositoryMock.Object, configurationMock.Object, loggerMock.Object);
        var email = "per@hansen.com";
        var password = "P1rhansen#";

        var user = new User
        {
            Id = 1,
            Email = email,
            HashedPassword = "$2a$11$H.YLW/LIwUy1/UicyUvn2.cjXuirIdmgcZzg68xGcnChNNPn59gsS",
            Salt = "$2a$11$H.YLW/LIwUy1/UicyUvn2."
        };

        userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                          .ReturnsAsync(user);

        // Act
        var result = await authService.AuthenticateUserAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
  

        // verifiser token generering
        var token = await authService.GenerateJwtTokenAsync(result);
        Assert.NotNull(token);
       

        // Verifiser logging
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), 
                null, 
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), 
            Times.Once);
    }


    [Fact]
    public async Task GetUsersAsync_ValidToken_ReturnsUsers()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var loggerMock = new Mock<ILogger<UsersController>>();
        var controller = new UsersController(userServiceMock.Object, loggerMock.Object);
        var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJwZXJAaGFuc2VuLmNvbSIsIm5iZiI6MTcxMTM3Njk5MywiZXhwIjoxNzExMzgwNTkzLCJpYXQiOjE3MTEzNzY5OTMsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjcwMTkifQ.NMzBV9lyqgKW8clFc0DCuxg3UFt6C7rgmmJtMdSecJU";

        // setter opp HTTP context med authorization header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer " + jwtToken;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // setter opp mock response fra user service
        var expectedUsers = new List<UserDTO> { new UserDTO(1, "Per Hansen", "per@hansen.com" ) };
        userServiceMock.Setup(service => service.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
                       .ReturnsAsync(expectedUsers);

        // Act
        var result = await controller.GetUsersAsync(1, 10);

        // Assert
        Assert.IsType<ActionResult<IEnumerable<UserDTO>>>(result);
        Assert.NotNull(result.Value);
        Assert.IsAssignableFrom<IEnumerable<UserDTO>>(result.Value);

        // Verifiserer logging
        loggerMock.Verify(
            logger => logger.LogInformation(
                "User {UserId} retrieved {UserCount} users",
                It.IsAny<int>(), expectedUsers.Count),
            Times.Once);
    }
}