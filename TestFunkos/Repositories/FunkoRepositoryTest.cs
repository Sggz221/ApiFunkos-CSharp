using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Repositories.Categorias;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunkos;

[TestFixture]
public class FunkoRepositoryTest
{
    private Context _context;
    private FunkoRepository _repository;
    private Mock<ILogger<FunkoRepository>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var contextLoggerMock = new Mock<ILogger<Context>>();
        _context = new Context(options, contextLoggerMock.Object);
        _loggerMock = new Mock<ILogger<FunkoRepository>>();
        _repository = new FunkoRepository(_context, _loggerMock.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnFilteredItems_WhenFilterIsApplied()
    {
        // Arrange
        var category = new Category("Marvel");
        await _context.Categories.AddAsync(category);
        
        var funko1 = new Funko { Nombre = "Iron Man", Precio = 50, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var funko2 = new Funko { Nombre = "Batman", Precio = 60, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _context.Funkos.AddRangeAsync(funko1, funko2);
        await _context.SaveChangesAsync();
        
        var filter = new FilterDto(Nombre: "Iron", Categoria: null, MaxPrecio: null);

        // Act
        var result = await _repository.GetAllAsync(filter);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Nombre, Is.EqualTo("Iron Man"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFunko_WhenIdExists()
    {
        // Arrange
        var category = new Category("DC");
        var funko = new Funko { Nombre = "Wonder Woman", Precio = 45, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _context.Funkos.AddAsync(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(funko.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("Wonder Woman"));
    }
    
    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldAddNewFunko()
    {
        // Arrange
        var category = new Category("Anime");
        await _context.Categories.AddAsync(category); // Ensure category is tracked
        await _context.SaveChangesAsync();

        var newFunko = new Funko { Nombre = "Goku", Precio = 30, CategoriaId = category.Id, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.SaveAsync(newFunko);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(_context.Funkos.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateFunko_WhenFunkoExists()
    {
        // Arrange
        var category = new Category("Movies");
        var funko = new Funko { Nombre = "Harry Potter", Precio = 40, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _context.Funkos.AddAsync(funko);
        await _context.SaveChangesAsync();

        var updatedFunko = new Funko { Nombre = "Harry Potter Updated", Precio = 55, CategoriaId = category.Id, Categoria = category };

        // Act
        var result = await _repository.UpdateAsync(funko.Id, updatedFunko);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("Harry Potter Updated"));
        Assert.That(result.Precio, Is.EqualTo(55));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveFunko_WhenFunkoExists()
    {
        // Arrange
        var category = new Category("Music");
        var funko = new Funko { Nombre = "Freddie Mercury", Precio = 100, Categoria = category, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _context.Funkos.AddAsync(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(funko.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_context.Funkos.Count(), Is.EqualTo(0));
    }
}
