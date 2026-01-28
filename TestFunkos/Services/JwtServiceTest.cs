using cSharpApiFunko.Models;
using cSharpApiFunko.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TestFunkos.Services;

[TestFixture]
public class JwtServiceTest
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILogger<JwtService>> _loggerMock;
    private JwtService _service;

    [SetUp]
    public void Setup()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<JwtService>>();

        // Setup default JWT configuration
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForTestingPurposesWithMinimum256Bits1234567890");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TiendaApi");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TiendaApi");
        _configurationMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

        _service = new JwtService(
            _configurationMock.Object,
            _loggerMock.Object
        );
    }

    // ===== GenerateToken Tests =====

    [Test]
    public void GenerateToken_ShouldGenerateValidToken_WhenUserIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);

        // Verify token structure
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value, Is.EqualTo("testuser"));
        Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value, Is.EqualTo("test@example.com"));
        Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value, Is.EqualTo("USER"));
        Assert.That(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, Is.EqualTo(user.Id.ToString()));
    }

    [Test]
    public void GenerateToken_ShouldIncludeAllRequiredClaims()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 2,
            UserName = "adminuser",
            Email = "admin@example.com",
            Role = "ADMIN"
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();
        Assert.That(claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub), Is.True);
        Assert.That(claims.Any(c => c.Type == JwtRegisteredClaimNames.Email), Is.True);
        Assert.That(claims.Any(c => c.Type == ClaimTypes.Role), Is.True);
        Assert.That(claims.Any(c => c.Type == ClaimTypes.NameIdentifier), Is.True);
        Assert.That(claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti), Is.True);
    }

    [Test]
    public void GenerateToken_ShouldThrowException_WhenJwtKeyNotConfigured()
    {
        // Arrange
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns((string)null!);

        var user = new Usuario
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.GenerateToken(user));
    }

    [Test]
    public void GenerateToken_ShouldUseDefaultValues_WhenOptionalConfigurationMissing()
    {
        // Arrange
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns((string)null!);
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns((string)null!);
        _configurationMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns((string)null!);

        var user = new Usuario
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.That(jwtToken.Issuer, Is.EqualTo("TiendaApi"));
        Assert.That(jwtToken.Audiences.First(), Is.EqualTo("TiendaApi"));
    }

    // ===== ValidateToken Tests =====

    [Test]
    public void ValidateToken_ShouldReturnUsername_WhenTokenIsValid()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "validuser",
            Email = "valid@example.com",
            Role = "USER"
        };

        var token = _service.GenerateToken(user);

        // Act
        var result = _service.ValidateToken(token);

        // Assert
        Assert.That(result, Is.EqualTo("validuser"));
    }

    [Test]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _service.ValidateToken(invalidToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsEmpty()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var result = _service.ValidateToken(emptyToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_ShouldReturnNull_WhenTokenHasWrongSignature()
    {
        // Arrange
        var user = new Usuario
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Role = "USER"
        };

        var token = _service.GenerateToken(user);

        // Change the key to simulate wrong signature
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("DifferentKeyThatWillCauseSignatureValidationToFail1234567890");

        // Create new service with different key
        var serviceWithDifferentKey = new JwtService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var result = serviceWithDifferentKey.ValidateToken(token);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_ShouldReturnNull_WhenJwtKeyNotConfigured()
    {
        // Arrange
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns((string)null!);
        var invalidService = new JwtService(_configurationMock.Object, _loggerMock.Object);

        var token = "some.token.here";

        // Act
        var result = invalidService.ValidateToken(token);

        // Assert - ValidateToken catches exceptions and returns null
        Assert.That(result, Is.Null);
    }
}
