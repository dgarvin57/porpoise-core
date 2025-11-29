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
/// Integration tests for multi-tenancy isolation.
/// These tests write to the actual dev database to verify tenant isolation.
/// </summary>
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

        // Create test tenants
        var tenant1 = new Tenant { TenantKey = "test-tenant-1", Name = "Test Tenant 1", IsActive = true };
        var tenant2 = new Tenant { TenantKey = "test-tenant-2", Name = "Test Tenant 2", IsActive = true };
        
        _testTenant1Id = _tenantRepository.AddAsync(tenant1).Result;
        _testTenant2Id = _tenantRepository.AddAsync(tenant2).Result;

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

        // Assert - Each tenant sees only their own surveys
        var tenant1List = new List<Survey>(tenant1Surveys);
        var tenant2List = new List<Survey>(tenant2Surveys);
        
        Assert.Single(tenant1List);
        Assert.Single(tenant2List);
        Assert.Equal("Tenant 1 Survey", tenant1List[0].SurveyName);
        Assert.Equal("Tenant 2 Survey", tenant2List[0].SurveyName);
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

        // Assert - Tenant 1 sees only their data
        var tenant1Projects = new List<Project>(await _projectRepositoryTenant1.GetAllAsync());
        var tenant1Surveys = new List<Survey>(await _surveyRepositoryTenant1.GetAllAsync());
        
        Assert.Single(tenant1Projects);
        Assert.Single(tenant1Surveys);
        Assert.Equal("Isolation Test Project 1", tenant1Projects[0].ProjectName);
        Assert.Equal("Complete Survey Tenant 1", tenant1Surveys[0].SurveyName);

        // Assert - Tenant 2 sees only their data
        var tenant2Projects = new List<Project>(await _projectRepositoryTenant2.GetAllAsync());
        var tenant2Surveys = new List<Survey>(await _surveyRepositoryTenant2.GetAllAsync());
        
        Assert.Single(tenant2Projects);
        Assert.Single(tenant2Surveys);
        Assert.Equal("Isolation Test Project 2", tenant2Projects[0].ProjectName);
        Assert.Equal("Complete Survey Tenant 2", tenant2Surveys[0].SurveyName);

        // Assert - Cross-tenant access returns nothing
        var tenant1CannotSeeProject2 = await _projectRepositoryTenant1.GetByNameAsync("Isolation Test Project 2");
        var tenant2CannotSeeSurvey1 = await _surveyRepositoryTenant2.GetByNameAsync("Complete Survey Tenant 1");
        
        Assert.Null(tenant1CannotSeeProject2);
        Assert.Null(tenant2CannotSeeSurvey1);
    }

    public void Dispose()
    {
        // Cleanup: Delete all created test data
        foreach (var surveyId in _createdSurveyIds)
        {
            try
            {
                _surveyRepositoryTenant1.DeleteAsync(surveyId).Wait();
            }
            catch { /* Ignore if already deleted */ }
            try
            {
                _surveyRepositoryTenant2.DeleteAsync(surveyId).Wait();
            }
            catch { /* Ignore if already deleted */ }
        }

        foreach (var projectId in _createdProjectIds)
        {
            try
            {
                _projectRepositoryTenant1.DeleteAsync(projectId).Wait();
            }
            catch { /* Ignore if already deleted */ }
            try
            {
                _projectRepositoryTenant2.DeleteAsync(projectId).Wait();
            }
            catch { /* Ignore if already deleted */ }
        }

        // Delete test tenants
        _tenantRepository.DeleteAsync(_testTenant1Id).Wait();
        _tenantRepository.DeleteAsync(_testTenant2Id).Wait();
    }
}
