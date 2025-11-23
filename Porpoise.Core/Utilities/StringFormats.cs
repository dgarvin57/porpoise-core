#nullable enable

using System.Text;
using System.Text.RegularExpressions;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Legacy utility class containing string formatting helpers from the original Porpoise app.
/// Currently only contains PhoneNumber formatting (US 10-digit).
/// </summary>
public static class StringFormats
{
    public static string PhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "";

        if (phone.Contains("+")) return phone;

        var cleanPhone = new StringBuilder();

        // Strip off all but digits (original VB used a regex per character — we preserve the logic)
        var digitRegex = new Regex("^[0-9]*$");
        foreach (char c in phone)
        {
            if (digitRegex.IsMatch(c.ToString()))
                cleanPhone.Append(c);
        }

        string digits = cleanPhone.ToString();
        if (string.IsNullOrEmpty(digits)) return "";

        // Format (000) 000-0000 only if exactly 10 digits
        if (digits.Length == 10 && long.TryParse(digits, out long tryPhone))
        {
            return string.Format("{0:###-###-####}", tryPhone);
        }

        return phone;
    }
}