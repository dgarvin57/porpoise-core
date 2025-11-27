#nullable enable

using Microsoft.VisualBasic.FileIO; // Required for FileSystem.RenameDirectory/RenameFile
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Legacy static utility class containing file/folder/path helpers from the original Porpoise codebase.
/// Includes directory creation, filename incrementing, image loading, logging, and recursive copy/delete.
/// </summary>
public static class IOUtils
{
    // Validate directory format
    public static bool IsDirectoryValid(string path)
    {
        try
        {
            _ = Path.GetDirectoryName(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Create directory
    public static bool CreateDirectory(string path)
    {
        if (!IsDirectoryValid(path)) return false;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return true;
    }

    // Increment filename
    public static string IncrementFileName(string filePath)
    {
        if (File.Exists(filePath))
        {
            string folderPath = Path.GetDirectoryName(filePath)!;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int fileNumber = 0;

            do
            {
                fileNumber++;
                filePath = Path.Combine(folderPath,
                    string.Format("{0}({1}){2}", fileName, fileNumber, extension));
            } while (File.Exists(filePath));
        }
        return filePath;
    }

    // Find highest incremented filename
    public static string GetLastIncrementedFileName(string filePath)
    {
        string previousFile = filePath;
        string folderPath = Path.GetDirectoryName(filePath)!;
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        int fileNumber = 0;

        do
        {
            fileNumber++;
            filePath = Path.Combine(folderPath, string.Format("{0}({1}){2}", fileName, fileNumber, extension));
            if (File.Exists(filePath)) previousFile = filePath;
        } while (File.Exists(filePath));

        return previousFile;
    }

    // Add increment to filename
    public static string AddIncrementToFilename(string filePath, int increment)
    {
        string folderPath = Path.GetDirectoryName(filePath)!;
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        var nameParts = fileName.Split('(');
        string baseName = nameParts[0];

        if (nameParts.Length > 1)
        {
            string incStr = nameParts[1].Replace(")", "");
            if (!int.TryParse(incStr, out _))
            {
                increment = 1;
            }
        }
        else
        {
            increment = 1;
        }

        return Path.Combine(folderPath, string.Format("{0}({1}){2}", baseName, increment, extension));
    }

    // Return the last part of the folder path (after default Porpoise project directory and before filename)
    public static string? GetProjectFolderDirectoryPart(string path, string defaultPropoiseProjectFolder)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        if (parts.Length <= 1) return null;

        for (int i = 0; i < parts.Length; i++)
        {
            if (string.Equals(parts[i], defaultPropoiseProjectFolder.Replace("\\", ""), StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < parts.Length)
                    return parts[i + 1];
            }
        }
        return null;
    }

    // Given a folder path (c:\...\PorpoiseProjects\Project Name) return Project Name
    public static string GetLastPartOfFolderPath(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        return parts[^1];
    }

    // Return the full base project folder path (c:\user\documents\Porpoise Projects)
    public static string? GetFullBaseProjectPath(string path, string defaultPropoiseProjectFolder)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        if (parts.Length <= 1) return null;

        string fullProjectPath = "";
        foreach (string part in parts)
        {
            if (!string.Equals(part, defaultPropoiseProjectFolder.Replace("\\", ""), StringComparison.OrdinalIgnoreCase))
            {
                fullProjectPath = fullProjectPath == "" ? part : fullProjectPath + Path.DirectorySeparatorChar + part;
            }
            else
            {
                fullProjectPath += Path.DirectorySeparatorChar + part;
                return fullProjectPath;
            }
        }
        return null;
    }

    // Given the Survey folder name and the full survey file path, derive folder name and path
    public static string? GetFullProjectPathFromSurveyPath(string path, string surveyFolder, string surveyFileName)
    {
        string surveyFolderAndName = surveyFolder + Path.DirectorySeparatorChar + surveyFileName;
        int endIndex = path.IndexOf(surveyFolderAndName, StringComparison.Ordinal);
        if (endIndex == -1) return null;

        return path[..(endIndex - 1)];
    }

    public static bool IsFilePathIsOK(string fileNameAndPath)
    {
        string fileName = string.Empty;
        string theDirectory = fileNameAndPath;
        char p = Path.DirectorySeparatorChar;
        var splitPath = fileNameAndPath.Split(p);

        if (splitPath.Length > 1)
        {
            fileName = splitPath[^1];
            theDirectory = string.Join(p, splitPath, 0, splitPath.Length - 1);
        }

        if (Path.GetInvalidFileNameChars().Any(c => fileName.Contains(c)))
            return false;

        return Path.GetInvalidPathChars().All(c => !theDirectory.Contains(c));
    }

    public static void RenameFolder(string oldDir, string newDir)
    {
        FileSystem.RenameDirectory(oldDir, newDir);
    }

    public static void RenameFile(string oldName, string newName)
    {
        FileSystem.RenameFile(oldName, newName);
    }

    //TODO: Re-implement image handling for web version
    // Load image file without locking it
    public static string GetImageUsingFileStream(string filepath)
    {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        // var tmpImage = Image.FromStream(fs);
        // var returnImage = new Bitmap(tmpImage);
//        tmpImage.Dispose();
        //return returnImage;
        return "Not implemented for web version";
    }

    // Create data folder where writable data is to be installed, such as ApplicationErrors.xml
    public static string CreateStaticDataFolder(string companyFolder)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userFilePath = Path.Combine(localAppData, companyFolder);

        if (!Directory.Exists(userFilePath))
            Directory.CreateDirectory(userFilePath);

        return userFilePath;
    }

    /// <summary>
    /// Write a line of text to a given log file
    /// </summary>
    public static void WriteToLogFile(string path, string logText)
    {
        if (!File.Exists(path))
            File.Create(path).Dispose();

        using var objWriter = new StreamWriter(path);
        objWriter.WriteLine(logText);
    }

    // Color red
    public static byte[] GetRed() => [0x0F, 0x1E, 0x92, 0xFD];

    // Copies all folders and files (including subfolders) from source to destination
    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        var sourceDirectoryInfo = new DirectoryInfo(sourcePath);

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        foreach (var fileSystemInfo in sourceDirectoryInfo.GetFileSystemInfos())
        {
            string destinationFileName = Path.Combine(destinationPath, fileSystemInfo.Name);

            if (fileSystemInfo is FileInfo)
                File.Copy(fileSystemInfo.FullName, destinationFileName, true);
            else
                CopyDirectory(fileSystemInfo.FullName, destinationFileName);
        }
    }

    public static void DeleteDirectory(string path)
    {
        DeleteDirectory(path, false);
    }

    public static void DeleteDirectory(string path, bool recursive)
    {
        if (recursive)
        {
            var subfolders = Directory.GetDirectories(path);
            foreach (string s in subfolders)
                DeleteDirectory(s, recursive);
        }

        var files = Directory.GetFiles(path);
        foreach (string f in files)
        {
            var attr = File.GetAttributes(f);
            if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);

            File.Delete(f);
        }

        Directory.Delete(path);
    }
}