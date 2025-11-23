#nullable enable
using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a user account in the Porpoise system.
/// Contains authentication and profile information with basic validation methods.
/// </summary>
public class Account : Porpoise.Core.Models.ObjectBase
{
    private string _authToken = string.Empty;
    private string _emailId = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _organization = string.Empty;
    private string _phone = string.Empty;

    /// <summary>
    /// Authentication token for the account.
    /// </summary>
    public string AuthToken
    {
        get => _authToken;
        set => SetProperty(ref _authToken, value, nameof(AuthToken));
    }

    /// <summary>
    /// Email address / identifier for the account.
    /// </summary>
    public string EmailId
    {
        get => _emailId;
        set => SetProperty(ref _emailId, value, nameof(EmailId));
    }

    /// <summary>
    /// Account password.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value, nameof(Password));
    }

    /// <summary>
    /// Confirmation of the account password.
    /// </summary>
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value, nameof(ConfirmPassword));
    }

    /// <summary>
    /// First name of the account holder.
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value, nameof(FirstName));
    }

    /// <summary>
    /// Last name of the account holder.
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value, nameof(LastName));
    }

    /// <summary>
    /// Organization associated with the account.
    /// </summary>
    public string Organization
    {
        get => _organization;
        set => SetProperty(ref _organization, value, nameof(Organization));
    }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value, nameof(Phone));
    }

    #region "Validation Methods"

    /// <summary>
    /// Email Id
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="EmailId"/> is null or empty.</exception>
    public void ValidateEmailId()
    {
        if (string.IsNullOrEmpty(EmailId))
            throw new ArgumentNullException(nameof(EmailId), "Email id is required");
    }

    /// <summary>
    /// Password
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="Password"/> is null or empty.</exception>
    public void ValidatePassword()
    {
        if (string.IsNullOrEmpty(Password))
            throw new ArgumentNullException(nameof(Password), "Password id is required");
    }

    /// <summary>
    /// First name
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="FirstName"/> is null or empty.</exception>
    public void ValidateFirstName()
    {
        if (string.IsNullOrEmpty(FirstName))
            throw new ArgumentNullException(nameof(FirstName), "First name is required");
    }

    /// <summary>
    /// Last name
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="LastName"/> is null or empty.</exception>
    public void ValidateLastName()
    {
        if (string.IsNullOrEmpty(LastName))
            throw new ArgumentNullException(nameof(LastName), "Last name id is required");
    }

    /// <summary>
    /// Phone
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="Phone"/> is null or empty.</exception>
    public void ValidatePhone()
    {
        if (string.IsNullOrEmpty(Phone))
            throw new ArgumentNullException(nameof(Phone), "Phone id is required");
    }

    /// <summary>
    /// Organization
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="Organization"/> is null or empty.</exception>
    public void ValidateOrganization()
    {
        if (string.IsNullOrEmpty(Organization))
            throw new ArgumentNullException(nameof(Organization), "Organization id is required");
    }

    #endregion // Validation methods
}
