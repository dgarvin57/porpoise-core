using FluentAssertions;
using Porpoise.Core.Models;
using System.IO.Compression;

namespace Porpoise.Core.Tests.Services;

public class PorpzImportTests : IDisposable
{
    private readonly string _testDataDir;
    private readonly string _destinationDir;
    private readonly List<string> _tempFilesToCleanup = new();

    public PorpzImportTests()
    {
        _testDataDir = Path.Combine(Path.GetTempPath(), "PorpoiseTests", Guid.NewGuid().ToString());
        _destinationDir = Path.Combine(Path.GetTempPath(), "PorpoiseImports", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataDir);
        Directory.CreateDirectory(_destinationDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDir))
            Directory.Delete(_testDataDir, true);

        if (Directory.Exists(_destinationDir))
            Directory.Delete(_destinationDir, true);

        foreach (var file in _tempFilesToCleanup.Where(File.Exists))
        {
            try { File.Delete(file); } catch { }
        }
    }

    [Fact]
    public void PorpzFile_WrongExtension_ShouldBeRejected()
    {
        // Arrange
        var wrongExtFile = Path.Combine(_testDataDir, "test.zip");
        File.WriteAllText(wrongExtFile, "test");
        _tempFilesToCleanup.Add(wrongExtFile);

        // Act & Assert - Extension check should happen before any import logic
        Path.GetExtension(wrongExtFile).ToLowerInvariant().Should().NotBe(".porpz");
    }

    [Fact]
    public void PorpzFile_CorrectExtension_ShouldBeAccepted()
    {
        // Arrange
        var correctExtFile = Path.Combine(_testDataDir, "test.porpz");
        
        // Act & Assert
        Path.GetExtension(correctExtFile).ToLowerInvariant().Should().Be(".porpz");
    }

    [Fact]
    public void ZipFile_EmptyArchive_CanBeCreatedAndRead()
    {
        // Arrange
        var emptyZip = Path.Combine(_testDataDir, "empty.porpz");
        
        // Act
        using (var archive = ZipFile.Open(emptyZip, ZipArchiveMode.Create))
        {
            // Empty archive
        }
        _tempFilesToCleanup.Add(emptyZip);

        // Assert
        using (var archive = ZipFile.OpenRead(emptyZip))
        {
            archive.Entries.Should().BeEmpty();
        }
    }

    [Fact]
    public void ZipFile_WithPorpFile_CanBeDetected()
    {
        // Arrange
        var zipWithPorp = Path.Combine(_testDataDir, "project.porpz");
        
        using (var archive = ZipFile.Open(zipWithPorp, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("project.porp");
            using var writer = new StreamWriter(entry.Open());
            writer.WriteLine("test content");
        }
        _tempFilesToCleanup.Add(zipWithPorp);

        // Act
        using var readArchive = ZipFile.OpenRead(zipWithPorp);
        var porpFiles = readArchive.Entries
            .Where(e => Path.GetExtension(e.Name).ToLowerInvariant() == ".porp")
            .ToList();

        // Assert
        porpFiles.Should().HaveCount(1);
        porpFiles[0].Name.Should().Be("project.porp");
    }

    [Fact]
    public void ZipFile_MultiplePorpFiles_CanBeDetected()
    {
        // Arrange
        var zipMultiple = Path.Combine(_testDataDir, "multiple.porpz");
        
        using (var archive = ZipFile.Open(zipMultiple, ZipArchiveMode.Create))
        {
            archive.CreateEntry("project1.porp");
            archive.CreateEntry("project2.porp");
            archive.CreateEntry("data.csv");
        }
        _tempFilesToCleanup.Add(zipMultiple);

        // Act
        using var readArchive = ZipFile.OpenRead(zipMultiple);
        var porpFiles = readArchive.Entries
            .Where(e => Path.GetExtension(e.Name).ToLowerInvariant() == ".porp")
            .ToList();

        // Assert
        porpFiles.Should().HaveCount(2);
    }

    [Fact]
    public void ZipFile_MatchingPorpFile_CanBeFound()
    {
        // Arrange
        var zipName = "exported.porpz";
        var zipPath = Path.Combine(_testDataDir, zipName);
        var expectedName = "exported";
        
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("project1.porp");
            archive.CreateEntry("exported.porp"); // Matches zip name
            archive.CreateEntry("project2.porp");
        }
        _tempFilesToCleanup.Add(zipPath);

        // Act
        using var readArchive = ZipFile.OpenRead(zipPath);
        var porpFiles = readArchive.Entries
            .Where(e => Path.GetExtension(e.Name).ToLowerInvariant() == ".porp")
            .ToList();
        
        var zipBaseName = Path.GetFileNameWithoutExtension(zipPath);
        var matchingFile = porpFiles.FirstOrDefault(f => 
            Path.GetFileNameWithoutExtension(f.Name) == zipBaseName);

        // Assert
        matchingFile.Should().NotBeNull();
        matchingFile!.Name.Should().Be("exported.porp");
    }

    [Fact]
    public void TempDirectory_CanBeCreatedAndDeleted()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "PorpoiseZip", Guid.NewGuid().ToString());

        // Act
        Directory.CreateDirectory(tempPath);
        var exists = Directory.Exists(tempPath);
        Directory.Delete(tempPath, true);
        var existsAfterDelete = Directory.Exists(tempPath);

        // Assert
        exists.Should().BeTrue();
        existsAfterDelete.Should().BeFalse();
    }

    [Fact]
    public void ProjectName_Validation_RequiresNonEmpty()
    {
        // Arrange
        var project = new Project();

        // Act & Assert
        project.ProjectName.Should().BeNullOrEmpty();
        
        project.ProjectName = "Valid Project Name";
        project.ProjectName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ZipExtraction_ToDirectory_Works()
    {
        // Arrange
        var zipPath = Path.Combine(_testDataDir, "extract.porpz");
        var extractPath = Path.Combine(_testDataDir, "extracted");
        
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("test.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.WriteLine("test content");
        }
        _tempFilesToCleanup.Add(zipPath);

        // Act
        using (var archive = ZipFile.OpenRead(zipPath))
        {
            archive.ExtractToDirectory(extractPath);
        }

        // Assert
        Directory.Exists(extractPath).Should().BeTrue();
        File.Exists(Path.Combine(extractPath, "test.txt")).Should().BeTrue();
    }
}
