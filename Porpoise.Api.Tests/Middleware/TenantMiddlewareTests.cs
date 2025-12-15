using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Porpoise.Api.Middleware;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using System.Text.Json;

namespace Porpoise.Api.Tests.Middleware;

public class TenantMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<TenantMiddleware>> _mockLogger;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly TenantContext _tenantContext;
    private readonly TenantMiddleware _middleware;

    public TenantMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<TenantMiddleware>>();
        _mockTenantRepository = new Mock<ITenantRepository>();
        _tenantContext = new TenantContext();
        _middleware = new TenantMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    #region OPTIONS Request Tests

    [Fact]
    public async Task InvokeAsync_SkipsTenantResolution_ForOptionsRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Method = "OPTIONS";

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
        _mockTenantRepository.Verify(repo => repo.GetByKeyAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Health/Swagger Tests

    [Theory]
    [InlineData("/swagger/index.html")]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/health")]
    [InlineData("/Health")]
    [InlineData("/SWAGGER")]
    public async Task InvokeAsync_SkipsTenantResolution_ForHealthAndSwagger(string path)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
        _mockTenantRepository.Verify(repo => repo.GetByKeyAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Tenant Header Tests

    [Fact]
    public async Task InvokeAsync_UsesDefaultDemo_WhenHeaderMissing()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        
        var tenant = new Tenant
        {
            TenantId = "demo-id",
            TenantKey = "demo",
            Name = "Demo Tenant",
            IsActive = true
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("demo")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockTenantRepository.Verify(r => r.GetByKeyAsync("demo"), Times.Once);
        _mockNext.Verify(next => next(context), Times.Once);
        _tenantContext.TenantKey.Should().Be("demo");
    }

    [Fact]
    public async Task InvokeAsync_UsesHeaderValue_WhenProvided()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "test-tenant";
        
        var tenant = new Tenant
        {
            TenantId = "test-id",
            TenantKey = "test-tenant",
            Name = "Test Tenant",
            IsActive = true
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("test-tenant")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockTenantRepository.Verify(r => r.GetByKeyAsync("test-tenant"), Times.Once);
        _mockNext.Verify(next => next(context), Times.Once);
        _tenantContext.TenantKey.Should().Be("test-tenant");
        _tenantContext.TenantId.Should().Be("test-id");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_WhenHeaderIsEmpty()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "";

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        _mockNext.Verify(next => next(context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_WhenHeaderIsWhitespace()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "   ";

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        _mockNext.Verify(next => next(context), Times.Never);
        
        // Verify error message
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("X-Tenant-Id header cannot be empty");
    }

    #endregion

    #region Tenant Validation Tests

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_WhenTenantNotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "nonexistent";
        
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("nonexistent")).ReturnsAsync((Tenant?)null);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        _mockNext.Verify(next => next(context), Times.Never);
        
        // Verify error message
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Invalid tenant");
        responseBody.Should().Contain("nonexistent");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenTenantIsInactive()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "inactive-tenant";
        
        var tenant = new Tenant
        {
            TenantId = "inactive-id",
            TenantKey = "inactive-tenant",
            Name = "Inactive Tenant",
            IsActive = false
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("inactive-tenant")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        context.Response.StatusCode.Should().Be(403);
        _mockNext.Verify(next => next(context), Times.Never);
        
        // Verify error message
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Tenant is inactive");
    }

    #endregion

    #region Tenant Context Tests

    [Fact]
    public async Task InvokeAsync_SetsTenantContext_WhenTenantIsValid()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "valid-tenant";
        
        var tenant = new Tenant
        {
            TenantId = "valid-id",
            TenantKey = "valid-tenant",
            Name = "Valid Tenant",
            IsActive = true
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("valid-tenant")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _tenantContext.TenantId.Should().Be("valid-id");
        _tenantContext.TenantKey.Should().Be("valid-tenant");
        _tenantContext.TenantName.Should().Be("Valid Tenant");
        _tenantContext.IsActive.Should().BeTrue();
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware_AfterSettingContext()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "test-tenant";
        
        var tenant = new Tenant
        {
            TenantId = "test-id",
            TenantKey = "test-tenant",
            Name = "Test Tenant",
            IsActive = true
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("test-tenant")).ReturnsAsync(tenant);

        var nextCalled = false;
        _mockNext.Setup(next => next(context))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
        _tenantContext.TenantKey.Should().Be("test-tenant");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task InvokeAsync_LogsInformation_WhenHeaderMissing()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        
        var tenant = new Tenant
        {
            TenantId = "demo-id",
            TenantKey = "demo",
            Name = "Demo Tenant",
            IsActive = true
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("demo")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing X-Tenant-Id header")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsWarning_WhenTenantNotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "nonexistent";
        
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("nonexistent")).ReturnsAsync((Tenant?)null);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid tenant key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsWarning_WhenTenantIsInactive()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/surveys";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "inactive-tenant";
        
        var tenant = new Tenant
        {
            TenantId = "inactive-id",
            TenantKey = "inactive-tenant",
            Name = "Inactive Tenant",
            IsActive = false
        };
        _mockTenantRepository.Setup(r => r.GetByKeyAsync("inactive-tenant")).ReturnsAsync(tenant);

        // Act
        await _middleware.InvokeAsync(context, _mockTenantRepository.Object, _tenantContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inactive tenant attempted access")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
