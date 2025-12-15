using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Porpoise.Api.Controllers;
using Porpoise.Api.Models;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;

namespace Porpoise.Api.Tests.Controllers;

public class TenantsControllerTests
{
    private readonly Mock<ITenantRepository> _mockRepository;
    private readonly Mock<TenantContext> _mockTenantContext;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TenantsController _controller;

    public TenantsControllerTests()
    {
        _mockRepository = new Mock<ITenantRepository>();
        _mockTenantContext = new Mock<TenantContext>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        // Set up default tenant context
        _mockTenantContext.Object.TenantId = "test-tenant-id";
        _mockTenantContext.Object.TenantKey = "test-tenant-key";
        
        _controller = new TenantsController(
            _mockRepository.Object,
            _mockTenantContext.Object,
            _mockUnitOfWork.Object
        );
    }

    #region GetCurrentTenant Tests

    [Fact]
    public async Task GetCurrentTenant_ReturnsOkWithTenant_WhenTenantExists()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationName = "Test Org",
            OrganizationTagline = "Test Tagline",
            OrganizationLogo = new byte[] { 1, 2, 3, 4 },
            CreatedBy = "admin",
            ModifiedBy = "admin"
        };
        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetCurrentTenant();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as TenantResponse;
        response.Should().NotBeNull();
        response!.TenantId.Should().Be("test-tenant-id");
        response.TenantKey.Should().Be("test-tenant-key");
        response.Name.Should().Be("Test Tenant");
        response.OrganizationName.Should().Be("Test Org");
        response.OrganizationTagline.Should().Be("Test Tagline");
        response.OrganizationLogoBase64.Should().NotBeNullOrEmpty();
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentTenant_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.GetCurrentTenant();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCurrentTenant_HandlesNullLogo()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationLogo = null
        };
        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetCurrentTenant();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as TenantResponse;
        response!.OrganizationLogoBase64.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentTenant_ConvertsLogoToBase64()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationLogo = new byte[] { 1, 2, 3, 4, 5 }
        };
        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetCurrentTenant();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as TenantResponse;
        response!.OrganizationLogoBase64.Should().Be(Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }));
    }

    #endregion

    #region UpdateOrganizationInfo Tests

    [Fact]
    public async Task UpdateOrganizationInfo_ReturnsNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationName = "Old Org",
            OrganizationTagline = "Old Tagline"
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "New Org",
            OrganizationTagline = "New Tagline"
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => 
            t.OrganizationName == "New Org" && 
            t.OrganizationTagline == "New Tagline")), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_ReturnsNotFound_WhenTenantDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "New Org"
        };
        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var updateRequest = new UpdateOrganizationRequest();
        _controller.ModelState.AddModelError("OrganizationName", "Required");

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateOrganizationInfo_TrimsWhitespace_FromFields()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "  Org With Spaces  ",
            OrganizationTagline = "  Tagline With Spaces  "
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => 
            t.OrganizationName == "Org With Spaces" && 
            t.OrganizationTagline == "Tagline With Spaces")), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_HandlesBase64Logo_Successfully()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true
        };
        var base64Logo = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org",
            OrganizationLogoBase64 = base64Logo
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => t.OrganizationLogo != null)), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_HandlesBase64LogoWithDataUrlPrefix()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true
        };
        var base64Logo = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org",
            OrganizationLogoBase64 = base64Logo
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => t.OrganizationLogo != null)), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_ReturnsBadRequest_WhenLogoFormatIsInvalid()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org",
            OrganizationLogoBase64 = "invalid-base64!!!"
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Invalid logo image format");
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_SetsLogoToNull_WhenNullProvided()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationLogo = new byte[] { 1, 2, 3 } // Existing logo
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org",
            OrganizationLogoBase64 = null
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => t.OrganizationLogo == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_SetsModifiedBy_ToSystem()
    {
        // Arrange
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            ModifiedBy = "old-user"
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org"
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => t.ModifiedBy == "system")), Times.Once);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_KeepsExistingLogo_WhenEmptyStringProvided()
    {
        // Arrange
        var originalLogo = new byte[] { 1, 2, 3 };
        var tenant = new Tenant
        {
            TenantId = "test-tenant-id",
            TenantKey = "test-tenant-key",
            Name = "Test Tenant",
            IsActive = true,
            OrganizationLogo = originalLogo
        };
        var updateRequest = new UpdateOrganizationRequest
        {
            OrganizationName = "Test Org",
            OrganizationLogoBase64 = "" // Empty string, not null
        };

        _mockRepository.Setup(r => r.GetByIdAsync("test-tenant-id")).ReturnsAsync(tenant);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateOrganizationInfo(updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        // Logo should remain unchanged (not set to null) when empty string is provided
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Tenant>(t => t.OrganizationLogo == originalLogo)), Times.Once);
    }

    #endregion
}
