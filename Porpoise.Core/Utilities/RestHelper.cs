#nullable enable

using System;
using System.IO;
using System.Net;
using System.Text;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Legacy REST helper used to call the old Porpoise licensing/auth web service.
/// Uses HttpWebRequest + DataContractJsonSerializer (pre-HttpClient era).
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

    public string responseTime
    {
        get => _responseTime;
        set => _responseTime = value;
    }

    public string status
    {
        get => _status;
        set => _status = value;
    }

    public string message
    {
        get => _message;
        set => _message = value;
    }

    public string dateCreated
    {
        get => _dateCreated;
        set => _dateCreated = value;
    }

    public string authToken
    {
        get => _authToken;
        set => _authToken = value;
    }

    public bool enterpriseLicensed
    {
        get => _enterpriseLicensed;
        set => _enterpriseLicensed = value;
    }

    public string enterpriseExpiry
    {
        get => _enterpriseExpiry;
        set => _enterpriseExpiry = value;
    }

    public int singleKeyLicensesAvailable
    {
        get => _singleKeyLicensesAvailable;
        set => _singleKeyLicensesAvailable = value;
    }

    public Task<RestHelper> GetMethodAsync(Uri address, string data) => MethodAsync(address, data, MethodType.GetMethod);

    public Task<RestHelper> PostMethodAsync(Uri address, string data) => MethodAsync(address, data, MethodType.PostMethod);

    private async Task<RestHelper> MethodAsync(Uri address, string data, MethodType type)
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
        if (!string.IsNullOrWhiteSpace(result) && result.TrimStart().StartsWith("{") == false)
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