using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Porpoise.Api.Controllers;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Xunit;

namespace Porpoise.Api.Tests.Controllers;

public class TenantControllerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<ILogger<TenantController>> _loggerMock;
    private readonly TenantController _controller;

    public TenantControllerTests()
    {
        _tenantRepoMock = new Mock<ITenantRepository>();
        _loggerMock = new Mock<ILogger<TenantController>>();
        _controller = new TenantController(_tenantRepoMock.Object, _loggerMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithTenants_WhenTenantsExist()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo Tenant", IsActive = true },
            new Tenant { TenantId = "2", TenantKey = "acme", Name = "Acme Corp", IsActive = true }
        };
        _tenantRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tenants);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTenants = okResult!.Value as IEnumerable<Tenant>;
        returnedTenants.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList_WhenNoTenants()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tenant>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTenants = okResult!.Value as IEnumerable<Tenant>;
        returnedTenants.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ReturnsOk_WhenTenantExists()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo Tenant" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetById("1");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTenant = okResult!.Value as Tenant;
        returnedTenant!.TenantId.Should().Be("1");
        returnedTenant.TenantKey.Should().Be("demo");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByIdAsync("999")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.GetById("999");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFound = result.Result as NotFoundObjectResult;
        notFound!.Value.Should().NotBeNull();
    }

    #endregion

    #region GetByKey Tests

    [Fact]
    public async Task GetByKey_ReturnsOk_WhenTenantExists()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo Tenant" };
        _tenantRepoMock.Setup(r => r.GetByKeyAsync("demo")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetByKey("demo");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTenant = okResult!.Value as Tenant;
        returnedTenant!.TenantKey.Should().Be("demo");
    }

    [Fact]
    public async Task GetByKey_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByKeyAsync("nonexistent")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.GetByKey("nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "NewTenant",
            Name = "New Tenant Corp"
        };
        _tenantRepoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((Tenant?)null);
        _tenantRepoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync("new-id");

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = result.Result as CreatedAtActionResult;
        created!.ActionName.Should().Be(nameof(TenantController.GetById));
        var tenant = created.Value as Tenant;
        tenant!.TenantKey.Should().Be("newtenant"); // Should be lowercased
        tenant.Name.Should().Be("New Tenant Corp");
        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTenantKeyIsEmpty()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "",
            Name = "Test Tenant"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTenantKeyIsWhitespace()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "   ",
            Name = "Test Tenant"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "test",
            Name = ""
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenTenantKeyAlreadyExists()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "demo",
            Name = "Demo Tenant"
        };
        var existing = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Existing" };
        _tenantRepoMock.Setup(r => r.GetByKeyAsync("demo")).ReturnsAsync(existing);

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_TrimsAndLowercasesTenantKey()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "  UPPERCASE  ",
            Name = "Test Tenant"
        };
        _tenantRepoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((Tenant?)null);
        _tenantRepoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync("new-id");

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = result.Result as CreatedAtActionResult;
        var tenant = created!.Value as Tenant;
        tenant!.TenantKey.Should().Be("uppercase");
    }

    [Fact]
    public async Task Create_TrimsName()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            TenantKey = "test",
            Name = "  Test Tenant  "
        };
        _tenantRepoMock.Setup(r => r.GetByKeyAsync(It.IsAny<string>())).ReturnsAsync((Tenant?)null);
        _tenantRepoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync("new-id");

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = result.Result as CreatedAtActionResult;
        var tenant = created!.Value as Tenant;
        tenant!.Name.Should().Be("Test Tenant");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsOk_WhenValidRequest()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo Tenant", IsActive = true };
        var request = new UpdateTenantRequest
        {
            Name = "Updated Name",
            IsActive = false
        };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update("1", request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedTenant = okResult!.Value as Tenant;
        updatedTenant!.Name.Should().Be("Updated Name");
        updatedTenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        var request = new UpdateTenantRequest { Name = "New Name" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("999")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.Update("999", request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Original", IsActive = true };
        var request = new UpdateTenantRequest
        {
            Name = "Updated Name"
            // TenantKey and IsActive not provided
        };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update("1", request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedTenant = okResult!.Value as Tenant;
        updatedTenant!.Name.Should().Be("Updated Name");
        updatedTenant.TenantKey.Should().Be("demo"); // Unchanged
        updatedTenant.IsActive.Should().BeTrue(); // Unchanged
    }

    [Fact]
    public async Task Update_TrimsAndLowercasesTenantKey()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo", IsActive = true };
        var request = new UpdateTenantRequest
        {
            TenantKey = "  NEWKEY  "
        };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update("1", request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedTenant = okResult!.Value as Tenant;
        updatedTenant!.TenantKey.Should().Be("newkey");
    }

    [Fact]
    public async Task Update_TrimsName()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo", IsActive = true };
        var request = new UpdateTenantRequest
        {
            Name = "  New Name  "
        };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update("1", request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedTenant = okResult!.Value as Tenant;
        updatedTenant!.Name.Should().Be("New Name");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.DeleteAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("1");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByIdAsync("999")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.Delete("999");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsInternalServerError_WhenDeleteFails()
    {
        // Arrange
        var tenant = new Tenant { TenantId = "1", TenantKey = "demo", Name = "Demo" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.DeleteAsync("1")).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("1");

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion
}
