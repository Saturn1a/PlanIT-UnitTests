using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Models.Entities;
using PlanIT.API.Repositories.Interfaces;
using PlanIT.API.Services.AuthenticationService;
using System.Text;


namespace PlanITAPI_UnitTests.Services;


public class AuthenticationServiceTests
{

    [Fact]
    public async Task AuthenticateUserAsync_CorrectPassword_ReturnsUser()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var authService = new AuthenticationService(userRepositoryMock.Object, configurationMock.Object, loggerMock.Object);
        var email = "kari@normann.com";
        var correctPassword = "K1rinormann#";

        var user = new User
        {
            Id = 1,
            Email = email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword)
        };

        userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                          .ReturnsAsync(user);

        // Act
        var result = await authService.AuthenticateUserAsync(email, correctPassword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }



    [Fact]
    public async Task AuthenticateUserAsync_WithIncorrectPassword_ShouldReturnNull()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var authService = new AuthenticationService(userRepositoryMock.Object, configurationMock.Object, loggerMock.Object);
        var email = "kari@normann.com";
        var incorrectPassword = "Krijormann#";

        var user = new User
        {
            Id = 1,
            Email = email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("K1rinormann#"),
        };

        userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                          .ReturnsAsync(user);

        // Act
        var result = await authService.AuthenticateUserAsync(email, incorrectPassword);

        // Assert
        Assert.Null(result);
    }



    [Fact]
    public async Task GenerateJwtTokenAsync_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["JwtSecret"] = "a-very-long-secret-key-to-meet-the-bit-requirement",
            ["Jwt:Issuer"] = "testIssuer",
            ["Jwt:Audience"] = "testAudience",
            ["Jwt:ExpiryInMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var authService = new AuthenticationService(userRepositoryMock.Object, configuration, loggerMock.Object);
        var user = new User { Id = 1, Email = "kari@normann.com" };

        // Act
        var token = await authService.GenerateJwtTokenAsync(user);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
    }



    [Fact]
    public async Task GenerateJwtTokenAsync_ValidUser_ReturnsWellStructuredToken()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["JwtSecret"] = "a-very-long-secret-key-here-to-meet-the-bit-requirement",
            ["Jwt:Issuer"] = "testIssuer",
            ["Jwt:Audience"] = "testAudience",
            ["Jwt:ExpiryInMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var authService = new AuthenticationService(userRepositoryMock.Object, configuration, loggerMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "per@hansen.com"
        };

        // Act
        var token = await authService.GenerateJwtTokenAsync(user);

        // Assert
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length); // JWT tokens have 3 parts, separated by periods.
    }


    [Fact]
    public async Task GenerateJwtTokenAsync_ValidUser_IncludesExpectedClaims()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["JwtSecret"] = "a-very-long-secret-key-here-to-meet-the-bit-requirement",
            ["Jwt:Issuer"] = "testIssuer",
            ["Jwt:Audience"] = "testAudience",
            ["Jwt:ExpiryInMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var authService = new AuthenticationService(userRepositoryMock.Object, configuration, loggerMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "kari@normann.com"
        };

        // Act
        var token = await authService.GenerateJwtTokenAsync(user);

        // Assert
        var parts = token.Split('.');
        var payload = parts[1]; // JWT's payload is in the second part of the token
        var claims = DecodePayload(payload);
        Assert.Contains("\"email\":\"kari@normann.com\"", claims);
    }


    // Helper method to decode JWT payload from base64 to JSON
    private string DecodePayload(string payload)
    {
        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);
        return json;
    }



    [Fact]
    public async Task GenerateJwtTokenAsync_MissingConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        // Simulates missing configuration by not setting necessary values
        configurationMock.SetupGet(x => x["JwtSecret"]).Returns(string.Empty);
        configurationMock.SetupGet(x => x["Jwt:Issuer"]).Returns(string.Empty);
        configurationMock.SetupGet(x => x["Jwt:Audience"]).Returns(string.Empty);
        configurationMock.SetupGet(x => x["Jwt:ExpiryInMinutes"]).Returns(string.Empty);

        var authService = new AuthenticationService(userRepositoryMock.Object, configurationMock.Object, loggerMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "kari@normann.com"
        };

        // Act & Assert
        // Expects an InvalidOperationException to be thrown due to missing configuration
        await Assert.ThrowsAsync<InvalidOperationException>(() => authService.GenerateJwtTokenAsync(user));
    }
}