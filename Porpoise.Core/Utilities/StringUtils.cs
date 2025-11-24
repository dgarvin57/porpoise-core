#nullable enable

using System;
using System.IO;
using System.Text;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Classic Porpoise string utilities — ProperCase and RTF/text file reading.
/// Used throughout the UI and reporting layers.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Converts a string to Proper Case (Title Case).
    /// Optionally forces the rest of the word to lowercase.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="forceLower">If true, all non-first letters are forced to lowercase</param>
    /// <returns>Proper-cased string, or null if input is null/empty/whitespace</returns>
    public static string? ProperCase(string? input, bool forceLower = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var sb = new StringBuilder();
        bool previousWasWhitespace = true;

        foreach (char c in input)
        {
            char current = c;

            if (char.IsWhiteSpace(current))
            {
                previousWasWhitespace = true;
            }
            else if (char.IsLetter(current))
            {
                if (previousWasWhitespace)
                {
                    current = char.ToUpper(current);
                }
                else if (forceLower)
                {
                    current = char.ToLower(current);
                }

                previousWasWhitespace = false;
            }

            sb.Append(current);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Reads the entire contents of a text file (RTF, TXT, etc.) as UTF-16 string.
    /// Used when loading RTF templates or notes.
    /// </summary>
    public static string GetTextFile(string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filepath));

        if (!File.Exists(filepath))
            throw new FileNotFoundException($"File not found: {filepath}", filepath);

        return File.ReadAllText(filepath);
    }
}