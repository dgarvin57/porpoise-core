#nullable enable

using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a logged error with full exception details, user context, and formatting helpers.
/// </summary>
[Serializable]
public class ErrorLog : ObjectBase
{
    #region Private Members

    private long _errorLogId = -1;
    private string _errorLocation = string.Empty;
    private string _userMessage = string.Empty;
    private Exception? _exception;
    private string _exceptionMessage = string.Empty;
    private string _exceptionSummary = string.Empty;
    private string _source = string.Empty;
    private string _stackTrace = string.Empty;
    private string _userId = string.Empty;
    private string _userRoleId = string.Empty;
    private DateTime _exceptionDate = DateTime.Now;
    private string _logFilePath = string.Empty;
    private int _maxErrorLogFileSizeMB;

    #endregion

    #region Public Properties

    [DisplayName("Error Log Id")]
    public long ErrorLogId
    {
        get => _errorLogId;
        set => SetProperty(ref _errorLogId, value, nameof(ErrorLogId));
    }

    [DisplayName("Error Location")]
    public string ErrorLocation
    {
        get => _errorLocation;
        set => SetProperty(ref _errorLocation, value, nameof(ErrorLocation));
    }

    [DisplayName("User Message")]
    public string UserMessage
    {
        get => _userMessage;
        set => SetProperty(ref _userMessage, value, "ErrorMessage");
    }

    [XmlIgnore]
    [DisplayName("Exception")]
    public Exception? Exception
    {
        get => _exception;
        set => SetProperty(ref _exception, value, nameof(Exception));
    }

    [DisplayName("Exception Message")]
    public string ExceptionMessage
    {
        get => _exceptionMessage;
        set => SetProperty(ref _exceptionMessage, value, nameof(ExceptionMessage));
    }

    [DisplayName("Exception Summary")]
    public string ExceptionSummary
    {
        get => _exceptionSummary;
        set => SetProperty(ref _exceptionSummary, value, nameof(ExceptionSummary));
    }

    [DisplayName("Source")]
    public string Source
    {
        get => _source;
        set => SetProperty(ref _source, value, nameof(Source));
    }

    [DisplayName("Stack Trace")]
    public string StackTrace
    {
        get => _stackTrace;
        set => SetProperty(ref _stackTrace, value, nameof(StackTrace));
    }

    [DisplayName("User Id")]
    public string UserId
    {
        get => _userId;
        set => SetProperty(ref _userId, value, nameof(UserId));
    }

    [DisplayName("User Role Id")]
    public string UserRoleId
    {
        get => _userRoleId;
        set => SetProperty(ref _userRoleId, value, nameof(UserRoleId));
    }

    [DisplayName("Exception Date")]
    public DateTime ExceptionDate
    {
        get => _exceptionDate;
        set => SetProperty(ref _exceptionDate, value, nameof(ExceptionDate));
    }

    public string LogFilePath
    {
        get => _logFilePath;
        set
        {
            _logFilePath = value;
            SetProperty(ref _logFilePath, value, nameof(LogFilePath));
        }
    }

    public int MaxErrorLogFileSizeMB
    {
        get => _maxErrorLogFileSizeMB;
        set => SetProperty(ref _maxErrorLogFileSizeMB, value, nameof(MaxErrorLogFileSizeMB));
    }

    #endregion

    #region Constructor

    public ErrorLog()
    {
        // Default constructor
    }

    public ErrorLog(Exception ex, string userMsg, string fromObject, string fromMethod, string user, string role, string logPath)
    {
        _errorLocation = fromObject + "." + fromMethod.Replace("Void ", "");
        _userMessage = userMsg;
        _exception = ex;
        _exceptionMessage = ex.Message;
        _exceptionSummary = ex.ToString();
        _source = ex.Source ?? string.Empty;
        _stackTrace = ex.StackTrace ?? string.Empty;
        _userId = user;
        _userRoleId = role;
        _logFilePath = logPath;

        if (ex.InnerException is not null && !string.IsNullOrEmpty(ex.InnerException.Message))
        {
            _exceptionMessage = string.Format("Original error: {0}", ex.InnerException.Message);
        }
    }

    #endregion

    #region Public Methods

    // Format error message by combining user and exception inner message, if available
    public string FormatErrorMessage()
    {
        var sb = new StringBuilder();
        sb.Append(_userMessage);
        sb.Append(Environment.NewLine + Environment.NewLine + "Location: " + _errorLocation + "." + Environment.NewLine + Environment.NewLine);

        if (!_exceptionMessage.Contains("Original error"))
            sb.Append("Original error: " + _exceptionMessage);
        else
            sb.Append(_exceptionMessage);

        return sb.ToString();
    }

    // Color orange
    public static byte[] GetOrange() => new byte[] { 0xA1, 0xF2, 0xE7, 0xD6 };

    #endregion
}

#region ErrorLog Collection

[Serializable]
public class ErrorLogs : List<ErrorLog>
{
}

public class ErrorLogComparer : Comparer<ErrorLog>
{
    public override int Compare(ErrorLog? x, ErrorLog? y)
    {
        if (x is null || y is null) return 0;
        return x.ErrorLogId.CompareTo(y.ErrorLogId);
    }
}

#endregion

// Custom event args
public class ErrorOnViewArgs : EventArgs
{
    public ErrorLog? ErrorLogObject { get; set; }

    public ErrorOnViewArgs(ErrorLog e)
    {
        ErrorLogObject = e;
    }
}