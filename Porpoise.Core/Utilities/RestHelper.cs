#nullable enable

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Porpoise.Core.Utilities;

/// <summary>
/// REST helper for calling the Porpoise licensing/auth web service.
/// Modernized to use HttpClient (.NET 5+) instead of legacy WebRequest.
/// </summary>
public class RestHelper
{
    private string _responseTime = string.Empty;
    private string _status = string.Empty;
    private string _message = string.Empty;
    private string _dateCreated = string.Empty;
    private string _authToken = string.Empty;
    private bool _enterpriseLicensed;
    private string _enterpriseExpiry = string.Empty;
    private int _singleKeyLicensesAvailable;

    public RestHelper() { }

    public RestHelper(DateTime responseTime, string status, string message, DateTime dateCreated, string authToken)
    {
        _responseTime = responseTime.ToString("o");
        _status = status;
        _message = message;
        _dateCreated = dateCreated.ToString("o");
        _authToken = authToken;
    }

    public string ResponseTime
    {
        get => _responseTime;
        set => _responseTime = value;
    }

    public string Status
    {
        get => _status;
        set => _status = value;
    }

    public string Message
    {
        get => _message;
        set => _message = value;
    }

    public string DateCreated
    {
        get => _dateCreated;
        set => _dateCreated = value;
    }

    public string AuthToken
    {
        get => _authToken;
        set => _authToken = value;
    }

    public bool EnterpriseLicensed
    {
        get => _enterpriseLicensed;
        set => _enterpriseLicensed = value;
    }

    public string EnterpriseExpiry
    {
        get => _enterpriseExpiry;
        set => _enterpriseExpiry = value;
    }

    public int SingleKeyLicensesAvailable
    {
        get => _singleKeyLicensesAvailable;
        set => _singleKeyLicensesAvailable = value;
    }

    // Synchronous wrapper methods for backward compatibility
    public static RestHelper GetMethod(Uri address, string? data)
        => GetMethodAsync(address, data ?? string.Empty).GetAwaiter().GetResult();

    public static RestHelper PostMethod(Uri address, string data)
        => PostMethodAsync(address, data).GetAwaiter().GetResult();

    public static Task<RestHelper> GetMethodAsync(Uri address, string data) => MethodAsync(address, data, MethodType.GetMethod);

    public static Task<RestHelper> PostMethodAsync(Uri address, string data) => MethodAsync(address, data, MethodType.PostMethod);

    private static async Task<RestHelper> MethodAsync(Uri address, string data, MethodType type)
    {
        using var httpClient = new HttpClient();
        HttpResponseMessage response;

        if (type == MethodType.PostMethod)
        {
            var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await httpClient.PostAsync(address, content);
        }
        else
        {
            response = await httpClient.GetAsync(address);
        }

        response.EnsureSuccessStatusCode();
        string result = await response.Content.ReadAsStringAsync();

        // Check for non-JSON response
        if (!string.IsNullOrWhiteSpace(result) && result.TrimStart().StartsWith('{') == false)
        {
            var msg = result.Split('{');
            if (msg.Length > 0 && !string.IsNullOrWhiteSpace(msg[0]))
                throw new Exception(msg[0].Trim());
        }

        return JSONHelper.FromJSON<RestHelper>(result);
    }
}

public enum MethodType
{
    GetMethod,
    PostMethod
}