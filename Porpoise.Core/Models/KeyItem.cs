#nullable enable

using System.Drawing;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a license key item displayed in the original Porpoise UI (key management screen).
/// Contains image, title, description, quantity, and formatting logic based on KeyType.
/// </summary>
public class KeyItem : ObjectBase
{
    private Image? _keyImage;
    private string _title = string.Empty;
    private string _details = string.Empty;
    private int _qtyRemaining;
    private string _description = string.Empty;
    private KeyType _type = KeyType.None;
    private bool _shorten;

    public Image? KeyImage
    {
        get => _keyImage;
        set => SetProperty(ref _keyImage, value, nameof(KeyImage));
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value, nameof(Title));
    }

    public string Details
    {
        get => _details;
        set => SetProperty(ref _details, value, nameof(Details));
    }

    public int QtyRemaining
    {
        get => _qtyRemaining;
        set => SetProperty(ref _qtyRemaining, value, nameof(QtyRemaining));
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value, nameof(Description));
    }

    public KeyType Type
    {
        get => _type;
        set => SetProperty(ref _type, value, nameof(Type));
    }

    public bool Shorten
    {
        get => _shorten;
        set => SetProperty(ref _shorten, value, nameof(Shorten));
    }

    public void SetKeyType(string emailId, KeyType keyType, Image? parmKeyImage, int qtyLeft, string parmDetails, bool shortMsg)
    {
        _type = keyType;
        _keyImage = parmKeyImage;
        _qtyRemaining = qtyLeft;
        _details = "";
        _shorten = shortMsg;

        if (Type == KeyType.Enterprise)
        {
            _title = "Enterprise Key";
            if (!shortMsg)
            {
                _description = $"<html><p>Allows the owner '{emailId}' to activate an unlimited </p><p>number of surveys at a single physical address</p></html>";
            }
            else
            {
                _description = $"<html><p>Allows the owner '{emailId}' </p><p>to activate an unlimited number of surveys at a single </p><p>physical address</p></html>";
            }

            if (!string.IsNullOrEmpty(parmDetails))
            {
                _details = string.Format("Expires: {0:MM/dd/yyyy}", parmDetails);
            }
        }
        else if (Type == KeyType.SingleSurvey)
        {
            _title = "Single Survey Key";
            if (!shortMsg)
            {
                _description = $"<html><p>Allows the owner '{emailId}' to activate a single </p><p>survey</p></html>";
            }
            else
            {
                _description = $"<html><p>Allows the owner '{emailId}' </p><p>to activate a single survey</p></html>";
            }

            if (!string.IsNullOrEmpty(parmDetails))
            {
                _details = string.Format("Qty: {0}", _qtyRemaining);
            }
        }
        else if (Type == KeyType.None)
        {
            _title = "None";
            if (!shortMsg)
            {
                _description = $"<html><p>No available keys for '{emailId}'. Click below to </p><p>acquire keys</p></html>";
            }
            else
            {
                _description = $"<html><p>No available keys for '{emailId}'. </p><p>Click below to acquire keys</p></html>";
            }
            _details = "";
        }
    }
}