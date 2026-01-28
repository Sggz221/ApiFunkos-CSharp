using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using FunkoApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunkos.Repositories;

[TestFixture]
public class CategoryRepositoryTest
{
    private Context _context;
    private CategoryRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var contextLoggerMock = new Mock<ILogger<Context>>();
        _context = new Context(options, contextLoggerMock.Object);
        _repository = new CategoryRepository(_context);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var category1 = new Category("Marvel");
        var category2 = new Category("DC");
        await _context.Categories.AddRangeAsync(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Contain(category1));
        Assert.That(result, Does.Contain(category2));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenIdExists()
    {
        // Arrange
        var category = new Category("Anime");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("Anime"));
        Assert.That(result.Id, Is.EqualTo(category.Id));
    }
    
    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByNameAsync_ShouldReturnCategory_WhenNameExists()
    {
        // Arrange
        var category = new Category("Horror");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Horror");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("Horror"));
    }
    
    [Test]
    public async Task GetByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var category = new Category("Movies");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("MOVIES");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("Movies"));
    }
    
    [Test]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNameDoesNotExist()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistent");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldAddNewCategory()
    {
        // Arrange
        var newCategory = new Category("Disney");

        // Act
        var result = await _repository.CreateAsync(newCategory);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo("Disney"));
        Assert.That(_context.Categories.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenCategoryExists()
    {
        // Arrange
        var category = new Category("OldName");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var updatedCategory = new Category("NewName");

        // Act
        var result = await _repository.UpdateAsync(category.Id, updatedCategory);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("NewName"));
        Assert.That(result.UpdatedAt, Is.GreaterThan(category.CreatedAt));
    }
    
    [Test]
    public async Task UpdateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        var updatedCategory = new Category("NewName");

        // Act
        var result = await _repository.UpdateAsync(Guid.NewGuid(), updatedCategory);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveCategory_WhenCategoryExists()
    {
        // Arrange
        var category = new Category("ToDelete");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(category.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Nombre, Is.EqualTo("ToDelete"));
        Assert.That(_context.Categories.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task DeleteAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }
}
