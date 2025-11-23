#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Legacy class used to track Orca export state (CSV + XML output paths and freshness).
/// Part of the old export pipeline.
/// </summary>
public class OrcaExport
{
    #region Private Members

    private string _fileName = string.Empty;
    private string _csvPath = string.Empty;
    private string _xmlPath = string.Empty;
    private ExportStateType _exportState = ExportStateType.None;

    #endregion

    #region Public Properties

    public string FileName
    {
        get => _fileName;
        set => _fileName = value;
    }

    public string CSVPath
    {
        get => _csvPath;
        set => _csvPath = value;
    }

    public string XMLPath
    {
        get => _xmlPath;
        set => _xmlPath = value;
    }

    public ExportStateType ExportState
    {
        get => _exportState;
        set => _exportState = value;
    }

    #endregion

    public OrcaExport() { }

    public OrcaExport(string file, string csv, string xml, ExportStateType state)
    {
        _fileName = file;
        _csvPath = csv;
        _xmlPath = xml;
        _exportState = state;
    }
}