#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single license record in the original Porpoise licensing system.
/// Still used by KeyItem and licensing UI logic.
/// </summary>
public class License : ObjectBase
{
    private Guid _id = Guid.NewGuid();
    private Guid _projectGuid;
    private Guid _surveyGuid;
    private string _surveyName = string.Empty;
    private LicenseStatusType _status;
    private DateTime _dateLicenseIssued;
    private string _customerName = string.Empty;
    private string _licenseKey = string.Empty; // Only for development purposes

    protected License() { }

    public Guid Id
    {
        get => _id;
        set => _id = value;
    }

    public Guid ProjectGuid
    {
        get => _projectGuid;
        set => _projectGuid = value;
    }

    public Guid SurveyGuid
    {
        get => _surveyGuid;
        set => _surveyGuid = value;
    }

    public string SurveyName
    {
        get => _surveyName;
        set => _surveyName = value;
    }

    public LicenseStatusType Status
    {
        get => _status;
        set => SetProperty(ref _status, value, nameof(Status));
    }

    public DateTime DateLicenseIssued
    {
        get => _dateLicenseIssued;
        set => _dateLicenseIssued = value;
    }

    public string CustomerName
    {
        get => _customerName;
        set => _customerName = value;
    }

    public string LicenseKey
    {
        get => _licenseKey;
        set => _licenseKey = value;
    }
}