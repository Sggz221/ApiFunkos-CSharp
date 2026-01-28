using cSharpApiFunko.Models;
using cSharpApiFunko.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace TestFunkos.Services;

[TestFixture]
public class JwtTokenExtractorTest
{
    private Mock<ILogger<JwtTokenExtractor>> _loggerMock;
    private JwtTokenExtractor _extractor;
    private JwtService _jwtService;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<JwtTokenExtractor>>();
        _extractor = new JwtTokenExtractor(_loggerMock.Object);

        // Setup JWT service to generate valid tokens for testing
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForTestingPurposesWithMinimum256Bits1234567890");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TiendaApi");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TiendaApi");
        configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

        var loggerJwtMock = new Mock<ILogger<JwtService>>();
        _jwtService = new JwtService(configMock.Object, loggerJwtMock.Object);
    }

    // ===== ExtractUserId Tests =====

    [Test]
    public void ExtractUserId_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 123,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.ExtractUserId(token);

        // Assert
        Assert.That(result, Is.EqualTo(123L));
    }

    [Test]
    public void ExtractUserId_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.format";

        // Act
        var result = _extractor.ExtractUserId(invalidToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractUserId_ShouldReturnNull_WhenTokenIsEmpty()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var result = _extractor.ExtractUserId(emptyToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== ExtractRole Tests =====

    [Test]
    public void ExtractRole_ShouldReturnRole_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "adminuser",
            Email = "admin@example.com",
            Role = "ADMIN"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.ExtractRole(token);

        // Assert
        Assert.That(result, Is.EqualTo("ADMIN"));
    }

    [Test]
    public void ExtractRole_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token";

        // Act
        var result = _extractor.ExtractRole(invalidToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== ExtractEmail Tests =====

    [Test]
    public void ExtractEmail_ShouldReturnEmail_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "user",
            Email = "user@example.com",
            Role = "USER"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.ExtractEmail(token);

        // Assert
        Assert.That(result, Is.EqualTo("user@example.com"));
    }

    [Test]
    public void ExtractEmail_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "bad.token";

        // Act
        var result = _extractor.ExtractEmail(invalidToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== IsAdmin Tests =====

    [Test]
    public void IsAdmin_ShouldReturnTrue_WhenRoleIsAdmin()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "adminuser",
            Email = "admin@example.com",
            Role = "ADMIN"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.IsAdmin(token);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAdmin_ShouldReturnTrue_WhenRoleIsAdminLowerCase()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "adminuser",
            Email = "admin@example.com",
            Role = "admin"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.IsAdmin(token);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAdmin_ShouldReturnFalse_WhenRoleIsUser()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "regularuser",
            Email = "user@example.com",
            Role = "USER"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.IsAdmin(token);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsAdmin_ShouldReturnFalse_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token";

        // Act
        var result = _extractor.IsAdmin(invalidToken);

        // Assert
        Assert.That(result, Is.False);
    }

    // ===== ExtractUserInfo Tests =====

    [Test]
    public void ExtractUserInfo_ShouldReturnAllInfo_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 999,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "ADMIN"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var (userId, isAdmin, role) = _extractor.ExtractUserInfo(token);

        // Assert
        Assert.That(userId, Is.EqualTo(999L));
        Assert.That(isAdmin, Is.True);
        Assert.That(role, Is.EqualTo("ADMIN"));
    }

    [Test]
    public void ExtractUserInfo_ShouldReturnNullValues_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid";

        // Act
        var (userId, isAdmin, role) = _extractor.ExtractUserInfo(invalidToken);

        // Assert
        Assert.That(userId, Is.Null);
        Assert.That(isAdmin, Is.False);
        Assert.That(role, Is.Null);
    }

    // ===== ExtractClaims Tests =====

    [Test]
    public void ExtractClaims_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.ExtractClaims(token);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Identity, Is.Not.Null);
        Assert.That(result.Identity!.IsAuthenticated, Is.True);
        Assert.That(result.Claims.Any(), Is.True);
    }

    [Test]
    public void ExtractClaims_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid";

        // Act
        var result = _extractor.ExtractClaims(invalidToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== IsValidTokenFormat Tests =====

    [Test]
    public void IsValidTokenFormat_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "user",
            Email = "user@example.com",
            Role = "USER"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var result = _extractor.IsValidTokenFormat(token);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidTokenFormat_ShouldReturnFalse_WhenTokenIsEmpty()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var result = _extractor.IsValidTokenFormat(emptyToken);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidTokenFormat_ShouldReturnFalse_WhenTokenHasInvalidFormat()
    {
        // Arrange
        var invalidToken = "not.a.valid.jwt.token";

        // Act
        var result = _extractor.IsValidTokenFormat(invalidToken);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidTokenFormat_ShouldReturnFalse_WhenTokenHasOnlyTwoParts()
    {
        // Arrange
        var twoPartToken = "header.payload";

        // Act
        var result = _extractor.IsValidTokenFormat(twoPartToken);

        // Assert
        Assert.That(result, Is.False);
    }
}
