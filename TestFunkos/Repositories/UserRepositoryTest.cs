using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using cSharpApiFunko.Repositories.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunkos.Repositories;

[TestFixture]
public class UserRepositoryTest
{
    private Context _context;
    private UserRepository _repository;
    private Mock<ILogger<UserRepository>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var contextLoggerMock = new Mock<ILogger<Context>>();
        _context = new Context(options, contextLoggerMock.Object);
        _loggerMock = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(_context, _loggerMock.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    // ===== GetAllAsync Tests =====
    
    [Test]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new Usuario { UserName = "user1", Email = "user1@test.com", PasswordHash = "hash1", Role = UserRoles.USER };
        var user2 = new Usuario { UserName = "admin1", Email = "admin1@test.com", PasswordHash = "hash2", Role = UserRoles.ADMIN };
        await _context.Usuarios.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result, Does.Contain(user1));
        Assert.That(result, Does.Contain(user2));
    }
    
    [Test]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsers()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    // ===== GetByIdAsync Tests =====
    
    [Test]
    public async Task GetByIdAsync_ShouldReturnUser_WhenIdExists()
    {
        // Arrange
        var user = new Usuario { UserName = "testuser", Email = "test@test.com", PasswordHash = "hash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("testuser"));
        Assert.That(result.Id, Is.EqualTo(user.Id));
    }
    
    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== FindByUsernameAsync Tests =====
    
    [Test]
    public async Task FindByUsernameAsync_ShouldReturnUser_WhenUsernameExists()
    {
        // Arrange
        var user = new Usuario { UserName = "john_doe", Email = "john@test.com", PasswordHash = "hash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByUsernameAsync("john_doe");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("john_doe"));
        Assert.That(result.Email, Is.EqualTo("john@test.com"));
    }
    
    [Test]
    public async Task FindByUsernameAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        // Act
        var result = await _repository.FindByUsernameAsync("nonexistent");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByUsernameAsync_ShouldReturnNull_WhenUsernameIsNull()
    {
        // Act
        var result = await _repository.FindByUsernameAsync(null!);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByUsernameAsync_ShouldReturnNull_WhenUsernameIsEmpty()
    {
        // Act
        var result = await _repository.FindByUsernameAsync("");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByUsernameAsync_ShouldReturnNull_WhenUsernameIsWhitespace()
    {
        // Act
        var result = await _repository.FindByUsernameAsync("   ");

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== FindByEmailAsync Tests =====
    
    [Test]
    public async Task FindByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var user = new Usuario { UserName = "user1", Email = "user1@example.com", PasswordHash = "hash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByEmailAsync("user1@example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("user1@example.com"));
        Assert.That(result.UserName, Is.EqualTo("user1"));
    }
    
    [Test]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Act
        var result = await _repository.FindByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        // Act
        var result = await _repository.FindByEmailAsync(null!);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenEmailIsEmpty()
    {
        // Act
        var result = await _repository.FindByEmailAsync("");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        // Act
        var result = await _repository.FindByEmailAsync("   ");

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== SaveAsync Tests =====
    
    [Test]
    public async Task SaveAsync_ShouldAddNewUser()
    {
        // Arrange
        var newUser = new Usuario 
        { 
            UserName = "newuser", 
            Email = "newuser@test.com", 
            PasswordHash = "securehash", 
            Role = UserRoles.USER 
        };

        // Act
        var result = await _repository.SaveAsync(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.UserName, Is.EqualTo("newuser"));
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.UpdatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(_context.Usuarios.Count(), Is.EqualTo(1));
    }
    
    [Test]
    public async Task SaveAsync_ShouldSetTimestamps()
    {
        // Arrange
        var newUser = new Usuario 
        { 
            UserName = "timetest", 
            Email = "time@test.com", 
            PasswordHash = "hash", 
            Role = UserRoles.ADMIN 
        };
        var beforeSave = DateTime.UtcNow;

        // Act
        var result = await _repository.SaveAsync(newUser);

        // Assert
        Assert.That(result.CreatedAt, Is.GreaterThanOrEqualTo(beforeSave));
        Assert.That(result.UpdatedAt, Is.GreaterThanOrEqualTo(beforeSave));
        Assert.That(result.CreatedAt, Is.EqualTo(result.UpdatedAt).Within(TimeSpan.FromSeconds(1)));
    }

    // ===== UpdateAsync Tests =====
    
    [Test]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var user = new Usuario { UserName = "oldname", Email = "old@test.com", PasswordHash = "oldhash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        var updatedUser = new Usuario 
        { 
            UserName = "newname", 
            Email = "new@test.com", 
            PasswordHash = "newhash", 
            Role = UserRoles.ADMIN 
        };

        // Act
        var result = await _repository.UpdateAsync(user.Id, updatedUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("newname"));
        Assert.That(result.Email, Is.EqualTo("new@test.com"));
        Assert.That(result.PasswordHash, Is.EqualTo("newhash"));
        Assert.That(result.Role, Is.EqualTo(UserRoles.ADMIN));
    }
    
    [Test]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = new Usuario { UserName = "user", Email = "user@test.com", PasswordHash = "hash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var originalUpdatedAt = user.UpdatedAt;
        await Task.Delay(10); // Small delay to ensure timestamp difference

        var updatedUser = new Usuario 
        { 
            UserName = "updatedname", 
            Email = "updated@test.com", 
            PasswordHash = "updatedhash", 
            Role = UserRoles.USER 
        };

        // Act
        var result = await _repository.UpdateAsync(user.Id, updatedUser);

        // Assert
        Assert.That(result!.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }
    
    [Test]
    public async Task UpdateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var updatedUser = new Usuario 
        { 
            UserName = "doesnotmatter", 
            Email = "doesnotmatter@test.com", 
            PasswordHash = "hash", 
            Role = UserRoles.USER 
        };

        // Act
        var result = await _repository.UpdateAsync(999, updatedUser);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ===== DeleteAsync Tests =====
    
    [Test]
    public async Task DeleteAsync_ShouldRemoveUser_WhenUserExists()
    {
        // Arrange
        var user = new Usuario { UserName = "todelete", Email = "delete@test.com", PasswordHash = "hash", Role = UserRoles.USER };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("todelete"));
        Assert.That(_context.Usuarios.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task DeleteAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.That(result, Is.Null);
        Assert.That(_context.Usuarios.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task DeleteAsync_ShouldNotAffectOtherUsers()
    {
        // Arrange
        var user1 = new Usuario { UserName = "user1", Email = "user1@test.com", PasswordHash = "hash1", Role = UserRoles.USER };
        var user2 = new Usuario { UserName = "user2", Email = "user2@test.com", PasswordHash = "hash2", Role = UserRoles.USER };
        await _context.Usuarios.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(user1.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_context.Usuarios.Count(), Is.EqualTo(1));
        
        var remainingUser = await _context.Usuarios.FirstOrDefaultAsync();
        Assert.That(remainingUser!.UserName, Is.EqualTo("user2"));
    }
}
