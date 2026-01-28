using cSharpApiFunko.Email;
using cSharpApiFunko.Errors;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Notifications;
using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Services.Funkos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestFunkos.Services;

[TestFixture]
public class FunkoServiceTest
{
    private Mock<IFunkoRepository> _funkoRepositoryMock;
    private Mock<ICategoryRepository> _categoryRepositoryMock;
    private Mock<IDistributedCache> _cacheMock;
    private Mock<ILogger<FunkoService>> _loggerMock;
    private Mock<IEmailService> _emailServiceMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<IHubContext<FunkoHub>> _hubContextMock;
    private FunkoService _service;

    [SetUp]
    public void Setup()
    {
        _funkoRepositoryMock = new Mock<IFunkoRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<FunkoService>>();
        _emailServiceMock = new Mock<IEmailService>();
        _configurationMock = new Mock<IConfiguration>();
        _hubContextMock = new Mock<IHubContext<FunkoHub>>();

        // Setup default configuration
        _configurationMock.Setup(c => c["Smtp:AdminEmail"]).Returns("admin@test.com");

        // Setup default hub context
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        _service = new FunkoService(
            _funkoRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            _emailServiceMock.Object,
            _configurationMock.Object,
            _hubContextMock.Object
        );
    }

    // ===== GetByIdAsync Tests =====

    [Test]
    public async Task GetByIdAsync_ShouldReturnFunko_WhenIdExists()
    {
        // Arrange
        var category = new Category("Marvel");
        var funko = new Funko
        {
            Id = 1,
            Nombre = "Iron Man",
            Precio = 50,
            Categoria = category,
            Image = "ironman.png"
        };

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[])null!);
        _funkoRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(funko);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Iron Man"));
        Assert.That(result.Value.Id, Is.EqualTo(1));
        _funkoRepositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnError_WhenIdDoesNotExist()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[])null!);
        _funkoRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Funko)null!);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("No se encontro funko con ID: 999"));
    }

    // ===== GetAllAsync Tests =====

    [Test]
    public async Task GetAllAsync_ShouldReturnPagedResponse()
    {
        // Arrange
        var category = new Category("DC");
        var funkos = new List<Funko>
        {
            new() { Id = 1, Nombre = "Batman", Precio = 45, Categoria = category, Image = "batman.png" },
            new() { Id = 2, Nombre = "Superman", Precio = 50, Categoria = category, Image = "superman.png" }
        };

        var filter = new FilterDto(null, null, null, 1, 10, "id", "asc");
        _funkoRepositoryMock.Setup(r => r.GetAllAsync(filter))
            .ReturnsAsync((funkos, 2));

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalCount, Is.EqualTo(2));
        var itemsList = result.Value.Items.ToList();
        Assert.That(itemsList.Count, Is.EqualTo(2));
        Assert.That(itemsList[0].Nombre, Is.EqualTo("Batman"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnEmptyPage_WhenNoFunkos()
    {
        // Arrange
        var filter = new FilterDto(null, null, null, 1, 10, "id", "asc");
        _funkoRepositoryMock.Setup(r => r.GetAllAsync(filter))
            .ReturnsAsync((new List<Funko>(), 0));

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalCount, Is.EqualTo(0));
        Assert.That(result.Value.Items, Is.Empty);
    }

    // ===== SaveAsync Tests =====

    [Test]
    public async Task SaveAsync_ShouldCreateFunko_WhenValidData()
    {
        // Arrange
        var category = new Category("Anime");
        var dto = new FunkoRequestDto("Goku", "Anime", 30, "goku.png");
        var savedFunko = new Funko
        {
            Id = 1,
            Nombre = "Goku",
            Precio = 30,
            Categoria = category,
            CategoriaId = category.Id,
            Image = "goku.png"
        };

        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("Anime"))
            .ReturnsAsync(category);
        _funkoRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<Funko>()))
            .ReturnsAsync(savedFunko);

        // Act
        var result = await _service.SaveAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Goku"));
        _categoryRepositoryMock.Verify(r => r.GetByNameAsync("Anime"), Times.Exactly(2));
        _funkoRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Funko>()), Times.Once);
    }

    [Test]
    public async Task SaveAsync_ShouldReturnError_WhenCategoryNotFound()
    {
        // Arrange
        var dto = new FunkoRequestDto("Test", "NonExistentCategory", 10, null);
        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("NonExistentCategory"))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.SaveAsync(dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("La categoria no es valida"));
        _funkoRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Funko>()), Times.Never);
    }

    // ===== UpdateAsync Tests =====

    [Test]
    public async Task UpdateAsync_ShouldUpdateFunko_WhenValidData()
    {
        // Arrange
        var category = new Category("Movies");
        var dto = new FunkoRequestDto("Harry Potter Updated", "Movies", 55, "hp.png");
        var updatedFunko = new Funko
        {
            Id = 1,
            Nombre = "Harry Potter Updated",
            Precio = 55,
            Categoria = category,
            CategoriaId = category.Id,
            Image = "hp.png"
        };

        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("Movies"))
            .ReturnsAsync(category);
        _funkoRepositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Funko>()))
            .ReturnsAsync(updatedFunko);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Harry Potter Updated"));
        _funkoRepositoryMock.Verify(r => r.UpdateAsync(1, It.IsAny<Funko>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnError_WhenFunkoNotFound()
    {
        // Arrange
        var category = new Category("Test");
        var dto = new FunkoRequestDto("Test", "Test", 10, null);

        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("Test"))
            .ReturnsAsync(category);
        _funkoRepositoryMock.Setup(r => r.UpdateAsync(999, It.IsAny<Funko>()))
            .ReturnsAsync((Funko)null!);

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("No se encontro funko con id: 999"));
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnError_WhenCategoryNotFound()
    {
        // Arrange
        var dto = new FunkoRequestDto("Test", "InvalidCategory", 10, null);
        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("InvalidCategory"))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("La categoria no es valida"));
    }

    // ===== PatchAsync Tests =====

    [Test]
    public async Task PatchAsync_ShouldUpdateFunko_WhenValidData()
    {
        // Arrange
        var category = new Category("Horror");
        var existingFunko = new Funko
        {
            Id = 1,
            Nombre = "OldName",
            Precio = 20,
            Categoria = category,
            Image = "old.png"
        };
        var patchDto = new FunkoPatchRequestDto
        {
            Nombre = "NewName",
            Precio = 25
        };

        _funkoRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingFunko);
        _funkoRepositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Funko>()))
            .ReturnsAsync(existingFunko);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PatchAsync(1, patchDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("NewName"));
        Assert.That(result.Value.Precio, Is.EqualTo(25));
    }

    [Test]
    public async Task PatchAsync_ShouldReturnError_WhenFunkoNotFound()
    {
        // Arrange
        var patchDto = new FunkoPatchRequestDto { Nombre = "Test" };
        _funkoRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Funko)null!);

        // Act
        var result = await _service.PatchAsync(999, patchDto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("Funko 999 no encontrado"));
    }

    [Test]
    public async Task PatchAsync_ShouldReturnError_WhenCategoryNotFound()
    {
        // Arrange
        var category = new Category("Test");
        var existingFunko = new Funko
        {
            Id = 1,
            Nombre = "Test",
            Precio = 20,
            Categoria = category,
            Image = "test.png"
        };
        var patchDto = new FunkoPatchRequestDto { Categoria = "NonExistent" };

        _funkoRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingFunko);
        _categoryRepositoryMock.Setup(r => r.GetByNameAsync("NonExistent"))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.PatchAsync(1, patchDto);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("La categoría: NonExistent no existe"));
    }

    // ===== DeleteAsync Tests =====

    [Test]
    public async Task DeleteAsync_ShouldDeleteFunko_WhenFunkoExists()
    {
        // Arrange
        var category = new Category("Music");
        var funko = new Funko
        {
            Id = 1,
            Nombre = "Freddie Mercury",
            Precio = 100,
            Categoria = category,
            Image = "freddie.png"
        };

        _funkoRepositoryMock.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(funko);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Freddie Mercury"));
        _funkoRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnError_WhenFunkoNotFound()
    {
        // Arrange
        _funkoRepositoryMock.Setup(r => r.DeleteAsync(999))
            .ReturnsAsync((Funko)null!);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Mensaje, Does.Contain("No se encontro funko con ID: 999"));
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }
}
