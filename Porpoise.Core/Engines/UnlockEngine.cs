#nullable enable

using Porpoise.Core.Models;
using Porpoise.Core.Utilities;
using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Porpoise.Core.Engines;

/// <summary>
/// Unlock & account management engine — handles cloud license validation,
/// password strength, account creation, sign-in, password reset, and license consumption.
/// </summary>
public partial class UnlockEngine
{
    private readonly Account _account;
    private readonly string _webBaseUri;

#pragma warning disable IDE0290 // Use primary constructor
    public UnlockEngine(string webBaseUri, Account account)
    {
        _webBaseUri = webBaseUri ?? throw new ArgumentNullException(nameof(webBaseUri));
        _account = account ?? throw new ArgumentNullException(nameof(account));
    }
#pragma warning restore IDE0290

    public static PasswordScore CheckPasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return PasswordScore.Blank;

        int n = 0;
        int l = password.Length;

        if (DigitRegex().IsMatch(password)) n += 10;
        if (LowercaseRegex().IsMatch(password)) n += 26;
        if (UppercaseRegex().IsMatch(password)) n += 26;
        if (SpecialCharRegex().IsMatch(password))
            n += l <= 6 ? 40 : 43;

        if (n == 0) return PasswordScore.Blank;

        int h = Convert.ToInt32(l * Math.Round(Math.Log(n) / Math.Log(2)));

        return h <= 32 ? PasswordScore.VeryWeak :
               h <= 48 ? PasswordScore.Weak :
               h <= 64 ? PasswordScore.Medium :
               h <= 80 ? PasswordScore.Strong :
                         PasswordScore.VeryStrong;
    }

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex(@"[~`!@#$%\^&*\(\)\-_+=\[\{\]\}\|\\;:'""<,>\.\?\/£]")]
    private static partial Regex SpecialCharRegex();

    public RestHelper CreateNewAccount()
    {
        var address = new Uri($"{_webBaseUri}/accounts/");
        var data = new StringBuilder()
            .Append("email=").Append(HttpUtility.UrlEncode(_account.EmailId))
            .Append("&password=").Append(HttpUtility.UrlEncode(_account.Password))
            .Append("&firstName=").Append(HttpUtility.UrlEncode(_account.FirstName))
            .Append("&lastName=").Append(HttpUtility.UrlEncode(_account.LastName))
            .Append("&phone=").Append(HttpUtility.UrlEncode(_account.Phone))
            .Append("&organization=").Append(HttpUtility.UrlEncode(_account.Organization));

        var response = RestHelper.PostMethod(address, data.ToString());
        _account.AuthToken = response.AuthToken;
        return response;
    }

    public RestHelper? SignIn()
    {
        if (string.IsNullOrEmpty(_webBaseUri) || _account == null)
            return null;

        var address = new Uri($"{_webBaseUri}/authtokens/");
        var data = new StringBuilder()
            .Append("email=").Append(HttpUtility.UrlEncode(_account.EmailId))
            .Append("&password=").Append(HttpUtility.UrlEncode(_account.Password));

        var response = RestHelper.PostMethod(address, data.ToString());
        if (response == null) return null;

        _account.AuthToken = response.AuthToken;
        return response;
    }

    public RestHelper? ResetPassword()
    {
        if (string.IsNullOrEmpty(_account.EmailId)) return null;

        var emailEncoded = HttpUtility.UrlEncode(_account.EmailId);
        var address = new Uri($"{_webBaseUri}/accounts/{emailEncoded}/passwordResetTokens");

        var response = RestHelper.PostMethod(address, "");

        _account.AuthToken = "";
        return response;
    }

    public RestHelper GetAvailableLicenses()
    {
        var data = new StringBuilder()
            .Append(HttpUtility.UrlEncode(_account.EmailId))
            .Append("/licenses/?available=true&summary=true&authtoken=")
            .Append(HttpUtility.UrlEncode(_account.AuthToken));

        var address = new Uri($"{_webBaseUri}/accounts/{data}");
        return RestHelper.GetMethod(address, null);
    }

    public RestHelper ConsumeLicense(Guid surveyId, string surveyName)
    {
        var address = new Uri($"{_webBaseUri}/accounts/{HttpUtility.UrlEncode(_account.EmailId)}/surveys/?authtoken={_account.AuthToken}");
        var data = new StringBuilder()
            .Append("surveyHash=").Append(HttpUtility.UrlEncode(surveyId.ToString()))
            .Append("&surveyDescr=").Append(HttpUtility.UrlEncode(surveyName));

        return RestHelper.PostMethod(address, data.ToString());
    }

    public static bool IsEmailValidFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        if (email.Count(c => c == '@') != 1) return false;

        var invalidChars = @"\|/?< >!#$%^&*()[\]{}""=+:;',~`";
        return !invalidChars.Any(email.Contains);
    }
}

public enum PasswordScore
{
    [Description("Blank")] Blank = 0,
    [Description("Very Weak")] VeryWeak = 1,
    [Description("Weak")] Weak = 2,
    [Description("Medium")] Medium = 3,
    [Description("Strong")] Strong = 4,
    [Description("Very Strong")] VeryStrong = 5
}