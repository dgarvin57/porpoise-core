using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;
using Xunit;

namespace Porpoise.DataAccess.Tests;

/// <summary>
/// Collection definition to ensure tenant isolation tests run sequentially, not in parallel.
/// </summary>
[CollectionDefinition("TenantIsolation", DisableParallelization = true)]
public class TenantIsolationCollection { }

/// <summary>
/// Integration tests for multi-tenancy isolation.
/// These tests write to the actual dev database to verify tenant isolation.
/// </summary>
[Collection("TenantIsolation")]
public class TenantIsolationTests : IDisposable
{
    private readonly DapperContext _context;
    private readonly ITenantRepository _tenantRepository;
    private readonly IProjectRepository _projectRepositoryTenant1;
    private readonly IProjectRepository _projectRepositoryTenant2;
    private readonly ISurveyRepository _surveyRepositoryTenant1;
    private readonly ISurveyRepository _surveyRepositoryTenant2;
    private readonly SurveyPersistenceService _persistenceServiceTenant1;
    private readonly SurveyPersistenceService _persistenceServiceTenant2;
    
    private int _testTenant1Id;
    private int _testTenant2Id;
    private readonly List<Guid> _createdProjectIds = new();
    private readonly List<Guid> _createdSurveyIds = new();

    public TenantIsolationTests()
    {
        // Use hardcoded connection string for tests
        var connectionString = "Server=localhost;Database=porpoise_dev;User=root;Password=Dg5901%1;";

        _context = new DapperContext(connectionString);
        _tenantRepository = new TenantRepository(_context);

        // Try to get existing test tenants, or create new ones
        var tenant1 = _tenantRepository.GetByKeyAsync("test-tenant-1").Result;
        var tenant2 = _tenantRepository.GetByKeyAsync("test-tenant-2").Result;

        if (tenant1 == null)
        {
            tenant1 = new Tenant { TenantKey = "test-tenant-1", Name = "Test Tenant 1", IsActive = true };
            _testTenant1Id = _tenantRepository.AddAsync(tenant1).Result;
        }
        else
        {
            _testTenant1Id = tenant1.TenantId;
        }

        if (tenant2 == null)
        {
            tenant2 = new Tenant { TenantKey = "test-tenant-2", Name = "Test Tenant 2", IsActive = true };
            _testTenant2Id = _tenantRepository.AddAsync(tenant2).Result;
        }
        else
        {
            _testTenant2Id = tenant2.TenantId;
        }

        // Create contexts for each tenant
        var tenantContext1 = new TenantContext { TenantId = _testTenant1Id, TenantKey = "test-tenant-1" };
        var tenantContext2 = new TenantContext { TenantId = _testTenant2Id, TenantKey = "test-tenant-2" };

        // Create repositories for each tenant
        _projectRepositoryTenant1 = new ProjectRepository(_context, tenantContext1);
        _projectRepositoryTenant2 = new ProjectRepository(_context, tenantContext2);
        _surveyRepositoryTenant1 = new SurveyRepository(_context, tenantContext1);
        _surveyRepositoryTenant2 = new SurveyRepository(_context, tenantContext2);

        var questionRepo1 = new QuestionRepository(_context);
        var questionRepo2 = new QuestionRepository(_context);
        var responseRepo1 = new ResponseRepository(_context);
        var responseRepo2 = new ResponseRepository(_context);
        var surveyDataRepo1 = new SurveyDataRepository(_context);
        var surveyDataRepo2 = new SurveyDataRepository(_context);

        _persistenceServiceTenant1 = new SurveyPersistenceService(
            _surveyRepositoryTenant1, questionRepo1, responseRepo1, surveyDataRepo1, _projectRepositoryTenant1);
        _persistenceServiceTenant2 = new SurveyPersistenceService(
            _surveyRepositoryTenant2, questionRepo2, responseRepo2, surveyDataRepo2, _projectRepositoryTenant2);

        // Clean up any leftover data from previous test runs
        CleanupTestDataAsync().Wait();
    }

    [Fact]
    public async Task Projects_AreIsolatedByTenant()
    {
        // Arrange
        var project1 = new Project { ProjectName = "Tenant 1 Project", ClientName = "Client A" };
        var project2 = new Project { ProjectName = "Tenant 2 Project", ClientName = "Client B" };

        // Act - Create projects in different tenants
        var saved1 = await _projectRepositoryTenant1.AddAsync(project1);
        var saved2 = await _projectRepositoryTenant2.AddAsync(project2);
        
        _createdProjectIds.Add(saved1.Id);
        _createdProjectIds.Add(saved2.Id);

        // Act - Query from each tenant's perspective
        var tenant1Projects = await _projectRepositoryTenant1.GetAllAsync();
        var tenant2Projects = await _projectRepositoryTenant2.GetAllAsync();

        // Assert - Each tenant sees only their own projects
        var tenant1List = new List<Project>(tenant1Projects);
        var tenant2List = new List<Project>(tenant2Projects);
        
        Assert.Single(tenant1List);
        Assert.Single(tenant2List);
        Assert.Equal("Tenant 1 Project", tenant1List[0].ProjectName);
        Assert.Equal("Tenant 2 Project", tenant2List[0].ProjectName);
        
        // Assert - Tenant 1 cannot see Tenant 2's project
        var tenant1TryGet = await _projectRepositoryTenant1.GetByIdAsync(saved2.Id);
        Assert.Null(tenant1TryGet);
        
        // Assert - Tenant 2 cannot see Tenant 1's project
        var tenant2TryGet = await _projectRepositoryTenant2.GetByIdAsync(saved1.Id);
        Assert.Null(tenant2TryGet);
    }

    [Fact]
    public async Task Surveys_AreIsolatedByTenant()
    {
        // Arrange
        var survey1 = new Survey 
        { 
            SurveyName = "Tenant 1 Survey", 
            Status = SurveyStatus.Verified,
            SurveyNotes = "Survey for Tenant 1"
        };
        var survey2 = new Survey 
        { 
            SurveyName = "Tenant 2 Survey", 
            Status = SurveyStatus.Initial,
            SurveyNotes = "Survey for Tenant 2"
        };

        // Act - Create surveys in different tenants
        var saved1 = await _surveyRepositoryTenant1.AddAsync(survey1);
        var saved2 = await _surveyRepositoryTenant2.AddAsync(survey2);
        
        _createdSurveyIds.Add(saved1.Id);
        _createdSurveyIds.Add(saved2.Id);

        // Act - Query from each tenant's perspective
        var tenant1Surveys = await _surveyRepositoryTenant1.GetAllAsync();
        var tenant2Surveys = await _surveyRepositoryTenant2.GetAllAsync();

        // Assert - Each tenant can see their own survey
        Assert.Contains(tenant1Surveys, s => s.Id == saved1.Id && s.SurveyName == "Tenant 1 Survey");
        Assert.Contains(tenant2Surveys, s => s.Id == saved2.Id && s.SurveyName == "Tenant 2 Survey");

        // Assert - Each tenant cannot see the other tenant's survey
        Assert.DoesNotContain(tenant1Surveys, s => s.Id == saved2.Id);
        Assert.DoesNotContain(tenant2Surveys, s => s.Id == saved1.Id);

        // Assert - Direct lookup across tenants should fail
        var tenant1CannotSeeSurvey2 = await _surveyRepositoryTenant1.GetByIdAsync(saved2.Id);
        var tenant2CannotSeeSurvey1 = await _surveyRepositoryTenant2.GetByIdAsync(saved1.Id);
        
        Assert.Null(tenant1CannotSeeSurvey2);
        Assert.Null(tenant2CannotSeeSurvey1);
    }

    [Fact]
    public async Task FullSurveyImport_IsIsolatedByTenant()
    {
        // Arrange - Create projects for each tenant
        var project1 = new Project { ProjectName = "Isolation Test Project 1", ClientName = "Client X" };
        var project2 = new Project { ProjectName = "Isolation Test Project 2", ClientName = "Client Y" };

        // Arrange - Create surveys with questions
        var survey1 = new Survey 
        { 
            SurveyName = "Complete Survey Tenant 1",
            Status = SurveyStatus.Verified,
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstNumber = "Q1", QstLabel = "Age", Responses = new ObjectListBase<Response>
                {
                    new Response { RespValue = 1, Label = "18-25" },
                    new Response { RespValue = 2, Label = "26-35" }
                }}
            },
            Data = new SurveyData
            {
                DataList = new List<List<string>>
                {
                    new() { "Q1" },
                    new() { "1" },
                    new() { "2" }
                }
            }
        };

        var survey2 = new Survey 
        { 
            SurveyName = "Complete Survey Tenant 2",
            Status = SurveyStatus.Initial,
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstNumber = "Q1", QstLabel = "Satisfaction", Responses = new ObjectListBase<Response>
                {
                    new Response { RespValue = 1, Label = "Yes" },
                    new Response { RespValue = 2, Label = "No" }
                }}
            },
            Data = new SurveyData
            {
                DataList = new List<List<string>>
                {
                    new() { "Q1" },
                    new() { "1" }
                }
            }
        };

        // Act - Save complete surveys with persistence service
        var savedSurvey1 = await _persistenceServiceTenant1.SaveSurveyWithDetailsAsync(survey1, project1);
        var savedSurvey2 = await _persistenceServiceTenant2.SaveSurveyWithDetailsAsync(survey2, project2);
        
        _createdSurveyIds.Add(savedSurvey1.Id);
        _createdSurveyIds.Add(savedSurvey2.Id);

        // Assert - Tenant 1 can see their own project and survey
        var tenant1Projects = await _projectRepositoryTenant1.GetAllAsync();
        var tenant1Surveys = await _surveyRepositoryTenant1.GetAllAsync();
        
        Assert.Contains(tenant1Projects, p => p.ProjectName == "Isolation Test Project 1");
        Assert.Contains(tenant1Surveys, s => s.Id == savedSurvey1.Id && s.SurveyName == "Complete Survey Tenant 1");

        // Assert - Tenant 2 can see their own project and survey
        var tenant2Projects = await _projectRepositoryTenant2.GetAllAsync();
        var tenant2Surveys = await _surveyRepositoryTenant2.GetAllAsync();
        
        Assert.Contains(tenant2Projects, p => p.ProjectName == "Isolation Test Project 2");
        Assert.Contains(tenant2Surveys, s => s.Id == savedSurvey2.Id && s.SurveyName == "Complete Survey Tenant 2");

        // Assert - Each tenant cannot see the other tenant's data
        Assert.DoesNotContain(tenant1Projects, p => p.ProjectName == "Isolation Test Project 2");
        Assert.DoesNotContain(tenant1Surveys, s => s.Id == savedSurvey2.Id);
        Assert.DoesNotContain(tenant2Projects, p => p.ProjectName == "Isolation Test Project 1");
        Assert.DoesNotContain(tenant2Surveys, s => s.Id == savedSurvey1.Id);

        // Assert - Cross-tenant access by name returns nothing
        var tenant1CannotSeeProject2 = await _projectRepositoryTenant1.GetByNameAsync("Isolation Test Project 2");
        var tenant2CannotSeeSurvey1 = await _surveyRepositoryTenant2.GetByNameAsync("Complete Survey Tenant 1");
        
        Assert.Null(tenant1CannotSeeProject2);
        Assert.Null(tenant2CannotSeeSurvey1);
    }

    // Helper method to clean up test data
    private async Task CleanupTestDataAsync()
    {
        // Get all surveys for both test tenants and delete them
        var tenant1Surveys = await _surveyRepositoryTenant1.GetAllAsync();
        foreach (var survey in tenant1Surveys)
        {
            _createdSurveyIds.Add(survey.Id);
            try
            {
                await _surveyRepositoryTenant1.DeleteAsync(survey.Id);
            }
            catch { /* Ignore if already deleted */ }
        }

        var tenant2Surveys = await _surveyRepositoryTenant2.GetAllAsync();
        foreach (var survey in tenant2Surveys)
        {
            _createdSurveyIds.Add(survey.Id);
            try
            {
                await _surveyRepositoryTenant2.DeleteAsync(survey.Id);
            }
            catch { /* Ignore if already deleted */ }
        }

        // Get all projects for both test tenants and delete them
        var tenant1Projects = await _projectRepositoryTenant1.GetAllAsync();
        foreach (var project in tenant1Projects)
        {
            _createdProjectIds.Add(project.Id);
            try
            {
                await _projectRepositoryTenant1.DeleteAsync(project.Id);
            }
            catch { /* Ignore if already deleted */ }
        }

        var tenant2Projects = await _projectRepositoryTenant2.GetAllAsync();
        foreach (var project in tenant2Projects)
        {
            _createdProjectIds.Add(project.Id);
            try
            {
                await _projectRepositoryTenant2.DeleteAsync(project.Id);
            }
            catch { /* Ignore if already deleted */ }
        }

        _createdSurveyIds.Clear();
        _createdProjectIds.Clear();
    }

    public void Dispose()
    {
        // Cleanup: Delete all test data for these tenants
        // Note: We intentionally do NOT delete the test tenants themselves,
        // as they are reused across test runs for efficiency
        CleanupTestDataAsync().Wait();
    }
}
