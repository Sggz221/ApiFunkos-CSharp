using cSharpApiFunko.Errors;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto.Categorias;
using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Services.Categorias;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;

namespace TestFunkos.Services;

[TestFixture]
public class CategoryServiceTest
{
    private Mock<ICategoryRepository> _repositoryMock;
    private Mock<IDistributedCache> _cacheMock;
    private CategoryService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _cacheMock = new Mock<IDistributedCache>();

        _service = new CategoryService(
            _repositoryMock.Object,
            _cacheMock.Object
        );
    }

    // ===== GetByIdAsync Tests =====

    [Test]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenIdExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Marvel") { Id = categoryId };

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[])null!);
        _repositoryMock.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Marvel"));
        Assert.That(result.Value.Id, Is.EqualTo(categoryId.ToString()));
        _repositoryMock.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnError_WhenIdDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[])null!);
        _repositoryMock.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain($"No se encontró la categoría con id: {categoryId}"));
    }

    // ===== GetAllAsync Tests =====

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new("Marvel") { Id = Guid.NewGuid() },
            new("DC") { Id = Guid.NewGuid() },
            new("Anime") { Id = Guid.NewGuid() }
        };

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Nombre, Is.EqualTo("Marvel"));
        Assert.That(result[1].Nombre, Is.EqualTo("DC"));
        Assert.That(result[2].Nombre, Is.EqualTo("Anime"));
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    // ===== CreateAsync Tests =====

    [Test]
    public async Task CreateAsync_ShouldCreateCategory_WhenNameIsUnique()
    {
        // Arrange
        var dto = new CategoryRequestDto { Nombre = "NewCategory" };
        var savedCategory = new Category("NewCategory") { Id = Guid.NewGuid() };

        _repositoryMock.Setup(r => r.GetByNameAsync("NewCategory"))
            .ReturnsAsync((Category)null!);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync(savedCategory);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("NewCategory"));
        _repositoryMock.Verify(r => r.GetByNameAsync("NewCategory"), Times.Once);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnError_WhenNameAlreadyExists()
    {
        // Arrange
        var dto = new CategoryRequestDto { Nombre = "ExistingCategory" };
        var existingCategory = new Category("ExistingCategory") { Id = Guid.NewGuid() };

        _repositoryMock.Setup(r => r.GetByNameAsync("ExistingCategory"))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("La categoría: ExistingCategory ya existe"));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
    }

    // ===== UpdateAsync Tests =====

    [Test]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenValidData()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CategoryRequestDto { Nombre = "UpdatedName" };
        var updatedCategory = new Category("UpdatedName") { Id = categoryId };

        _repositoryMock.Setup(r => r.GetByNameAsync("UpdatedName"))
            .ReturnsAsync((Category)null!);
        _repositoryMock.Setup(r => r.UpdateAsync(categoryId, It.IsAny<Category>()))
            .ReturnsAsync(updatedCategory);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(categoryId, dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("UpdatedName"));
        _repositoryMock.Verify(r => r.UpdateAsync(categoryId, It.IsAny<Category>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnError_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CategoryRequestDto { Nombre = "TestName" };

        _repositoryMock.Setup(r => r.GetByNameAsync("TestName"))
            .ReturnsAsync((Category)null!);
        _repositoryMock.Setup(r => r.UpdateAsync(categoryId, It.IsAny<Category>()))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.UpdateAsync(categoryId, dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain($"No se encontró la categoría con id: {categoryId}"));
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnError_WhenNameConflictsWithAnotherCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var anotherCategoryId = Guid.NewGuid();
        var dto = new CategoryRequestDto { Nombre = "ConflictName" };
        var existingCategory = new Category("ConflictName") { Id = anotherCategoryId };

        _repositoryMock.Setup(r => r.GetByNameAsync("ConflictName"))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _service.UpdateAsync(categoryId, dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("Ya existe otra categoría con el nombre: ConflictName"));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Category>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_ShouldAllowUpdate_WhenNameMatchesSameCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CategoryRequestDto { Nombre = "SameName" };
        var existingCategory = new Category("SameName") { Id = categoryId };
        var updatedCategory = new Category("SameName") { Id = categoryId };

        _repositoryMock.Setup(r => r.GetByNameAsync("SameName"))
            .ReturnsAsync(existingCategory);
        _repositoryMock.Setup(r => r.UpdateAsync(categoryId, It.IsAny<Category>()))
            .ReturnsAsync(updatedCategory);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(categoryId, dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("SameName"));
        _repositoryMock.Verify(r => r.UpdateAsync(categoryId, It.IsAny<Category>()), Times.Once);
    }

    // ===== DeleteAsync Tests =====

    [Test]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("ToDelete") { Id = categoryId };

        _repositoryMock.Setup(r => r.DeleteAsync(categoryId))
            .ReturnsAsync(category);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("ToDelete"));
        _repositoryMock.Verify(r => r.DeleteAsync(categoryId), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnError_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteAsync(categoryId))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain($"No se encontró la categoría con id: {categoryId}"));
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }
}
