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
    public async Task AuthenticateUserAsync_IncorrectPassword_ReturnsNull()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var authService = new AuthenticationService(userRepositoryMock.Object, configurationMock.Object, loggerMock.Object);
        var email = "per@hansen.com";
        var incorrectPassword = "Prhansen#";

        var user = new User
        {
            Id = 1,
            Email = email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("P1rhansen#"),
            Salt = "$2a$11$H.YLW/LIwUy1/UicyUvn2."
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
            ["Jwt:Secret"] = "a-very-long-secret-key-to-meet-the-bit-requirement",
            ["Jwt:Issuer"] = "testIssuer",
            ["Jwt:Audience"] = "testAudience",
            ["Jwt:ExpiryInMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var authService = new AuthenticationService(userRepositoryMock.Object, configuration, loggerMock.Object);
        var user = new User { Id = 1, Email = "user@example.com" };

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
            ["Jwt:Secret"] = "a-very-long-secret-key-here-to-meet-the-bit-requirement",
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
        Assert.Equal(3, parts.Length); // JWT tokens har 3 deler, separert med punktum
    }


    [Fact]
    public async Task GenerateJwtTokenAsync_ValidUser_IncludesExpectedClaims()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "a-very-long-secret-key-here-to-meet-the-bit-requirement",
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
        var payload = parts[1]; // JWT's payload er i andre delen av token
        var claims = DecodePayload(payload); 
        Assert.Contains("\"email\":\"kari@normann.com\"", claims);
    }

    // Hjelpemetode for å dekode JWT payload fra base64 til JSON
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

        // Simulerer manglende konfigurasjon ved å ikke sette nødvendige verdier
        configurationMock.SetupGet(x => x["Jwt:Secret"]).Returns(string.Empty);
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
        // Forventer at en InvalidOperationException kastes grunnet manglende konfigurasjon
        await Assert.ThrowsAsync<InvalidOperationException>(() => authService.GenerateJwtTokenAsync(user));
    }

}