#nullable enable

using System;
using Porpoise.Core.Models;
using Porpoise.Core.DataAccess;

namespace Porpoise.Core.Engines;

/// <summary>
/// Simple static facade for reading/writing the Porpoise error log (XML-based).
/// Keeps the UI layer clean — all error logging goes through here.
/// </summary>
public static class ErrorLogger
{
    /// <summary>
    /// Retrieves all logged errors from the XML file.
    /// </summary>
    public static ErrorLogs GetAllErrorLogs(string path)
    {
        try
        {
            return LegacyDataAccess.GetAllErrorsFromXmlFile(path);
        }
        catch
        {
            // Preserve stack trace and let caller handle
            throw;
        }
    }

    /// <summary>
    /// Saves the complete error log collection (with optional file size limiting).
    /// </summary>
    public static bool SaveAllErrorLogs(string path, ErrorLogs logs, int maxSize)
    {
        try
        {
            LegacyDataAccess.SaveAllErrorsToXmlFile(path, logs, maxSize);
            return true;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Appends a single error to the log file.
    /// </summary>
    public static bool LogError(string path, ErrorLog errorLog)
    {
        try
        {
            LegacyDataAccess.SaveErrorToXmlFile(path, errorLog);
            return true;
        }
        catch
        {
            throw;
        }
    }
}