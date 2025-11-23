#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Class representing a response definition object model
/// </summary>
[Serializable]
public class OrcaResponseDef : ObjectBase
{
    #region P R I V A T E   M E M B E R S

    private string _respValue = string.Empty;
    private string _label = string.Empty;
    private bool _responseIsLabeled;
    private bool _responseIsString;
    private bool _responseIsNull;
    private bool _responseIsDecimal;

    #endregion

    #region C O N S T R U C T O R S

    // Default constructor
    public OrcaResponseDef()
    {
    }

    #endregion

    #region P U B L I C   P R O P E R T I E S

    public string RespValue
    {
        get => _respValue;
        set
        {
            if (value == _respValue) return;
            _respValue = value;
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            if (value == _label) return;
            _label = value;
        }
    }

    public bool ResponseIsLabeled
    {
        get => _responseIsLabeled;
        set
        {
            if (value == _responseIsLabeled) return;
            _responseIsLabeled = value;
        }
    }

    public bool ResponseIsString
    {
        // Return true if response value is not a number (int, decimal...) or null
        get => _responseIsString;
        set
        {
            if (value == _responseIsString) return;
            _responseIsString = value;
        }
    }

    public bool ResponseIsNull
    {
        get => _responseIsNull;
        set
        {
            if (value == _responseIsNull) return;
            _responseIsNull = value;
        }
    }

    public bool ResponseIsDecimal
    {
        // Return true if response value is a decimal number
        get => _responseIsDecimal;
        set
        {
            if (value == _responseIsDecimal) return;
            _responseIsDecimal = value;
        }
    }

    #endregion

    #region M E T H O D S
    // (empty in original)
    #endregion
}