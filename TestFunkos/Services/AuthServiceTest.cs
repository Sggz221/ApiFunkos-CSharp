using cSharpApiFunko.Errors;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto.Usuarios;
using cSharpApiFunko.Repositories.Usuarios;
using cSharpApiFunko.Services.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunkos.Services;

[TestFixture]
public class AuthServiceTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private AuthService _service;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object
        );
    }

    // ===== SignUpAsync Tests =====

    [Test]
    public async Task SignUpAsync_ShouldRegisterUser_WhenValidData()
    {
        // Arrange
        var dto = new UserRequestDto(
            "newuser",
            "newuser@example.com",
            "password123"
        );

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("newuser"))
            .ReturnsAsync((Usuario)null!);
        _userRepositoryMock.Setup(r => r.FindByEmailAsync("newuser@example.com"))
            .ReturnsAsync((Usuario)null!);
        _userRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Usuario>()))
            .Returns("fake.jwt.token");

        // Act
        var result = await _service.SignUpAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo("fake.jwt.token"));
        Assert.That(result.Value.User.UserName, Is.EqualTo("newuser"));
        Assert.That(result.Value.User.Email, Is.EqualTo("newuser@example.com"));
        Assert.That(result.Value.User.Role, Is.EqualTo(UserRoles.USER));
        
        _userRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Usuario>()), Times.Once);
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<Usuario>()), Times.Once);
    }

    [Test]
    public async Task SignUpAsync_ShouldReturnError_WhenUsernameAlreadyExists()
    {
        // Arrange
        var dto = new UserRequestDto(
            "existinguser",
            "new@example.com",
            "password123"
        );

        var existingUser = new Usuario
        {
            Id = 1,
            UserName = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hashed"
        };

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("existinguser"))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((Usuario)null!);

        // Act
        var result = await _service.SignUpAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("username ya en uso"));
        _userRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task SignUpAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new UserRequestDto(
            "newuser",
            "existing@example.com",
            "password123"
        );

        var existingUser = new Usuario
        {
            Id = 1,
            UserName = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hashed"
        };

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("newuser"))
            .ReturnsAsync((Usuario)null!);
        _userRepositoryMock.Setup(r => r.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.SignUpAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("email ya en uso"));
        _userRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task SignUpAsync_ShouldHashPassword_BeforeSaving()
    {
        // Arrange
        var dto = new UserRequestDto(
            "secureuser",
            "secure@example.com",
            "plainpassword"
        );

        Usuario? savedUser = null;
        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("secureuser"))
            .ReturnsAsync((Usuario)null!);
        _userRepositoryMock.Setup(r => r.FindByEmailAsync("secure@example.com"))
            .ReturnsAsync((Usuario)null!);
        _userRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => savedUser = u)
            .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Usuario>()))
            .Returns("fake.jwt.token");

        // Act
        var result = await _service.SignUpAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.PasswordHash, Is.Not.EqualTo("plainpassword"));
        Assert.That(BCrypt.Net.BCrypt.Verify("plainpassword", savedUser.PasswordHash), Is.True);
    }

    // ===== SignInAsync Tests =====

    [Test]
    public async Task SignInAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var dto = new LogInDto(
            "validuser",
            "correctpassword"
        );

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword", workFactor: 11);
        var user = new Usuario
        {
            Id = 1,
            UserName = "validuser",
            Email = "valid@example.com",
            PasswordHash = passwordHash,
            Role = UserRoles.USER
        };

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("validuser"))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("valid.jwt.token");

        // Act
        var result = await _service.SignInAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo("valid.jwt.token"));
        Assert.That(result.Value.User.UserName, Is.EqualTo("validuser"));
        _jwtServiceMock.Verify(j => j.GenerateToken(user), Times.Once);
    }

    [Test]
    public async Task SignInAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var dto = new LogInDto(
            "nonexistent",
            "somepassword"
        );

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("nonexistent"))
            .ReturnsAsync((Usuario)null!);

        // Act
        var result = await _service.SignInAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("Credenciales inválidas"));
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task SignInAsync_ShouldReturnError_WhenPasswordIsInvalid()
    {
        // Arrange
        var dto = new LogInDto(
            "validuser",
            "wrongpassword"
        );

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword", workFactor: 11);
        var user = new Usuario
        {
            Id = 1,
            UserName = "validuser",
            Email = "valid@example.com",
            PasswordHash = passwordHash,
            Role = UserRoles.USER
        };

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("validuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.SignInAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("Credenciales inválidas"));
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task SignInAsync_ShouldReturnUserRole_InResponse()
    {
        // Arrange
        var dto = new LogInDto(
            "adminuser",
            "adminpass"
        );

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("adminpass", workFactor: 11);
        var user = new Usuario
        {
            Id = 1,
            UserName = "adminuser",
            Email = "admin@example.com",
            PasswordHash = passwordHash,
            Role = UserRoles.ADMIN
        };

        _userRepositoryMock.Setup(r => r.FindByUsernameAsync("adminuser"))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("admin.jwt.token");

        // Act
        var result = await _service.SignInAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.User.Role, Is.EqualTo(UserRoles.ADMIN));
    }
}
