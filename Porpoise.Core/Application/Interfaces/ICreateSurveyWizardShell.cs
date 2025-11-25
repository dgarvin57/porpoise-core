// Porpoise.Core/Application/Interfaces/ICreateSurveyWizardShell.cs
#nullable enable

using Porpoise.Core.Models;
using System;
using System.ComponentModel;

namespace Porpoise.Core.Application.Interfaces;

public enum CreateWizardType
{
    CreateProjectAndSurvey,
    JustCreateSurvey,
    ImportFromOrcaIntoExistingProject,
    ImportFromOrcaIntoNewProject
}

public class WizardPageChangedEventArgs : EventArgs
{
    public int PageIndex { get; }
    public WizardPageChangedEventArgs(int index) => PageIndex = index;
}

public class WizardNextEventArgs : CancelEventArgs { }

public interface ICreateSurveyWizardShell
{
    Project Project { get; }
    Survey Survey { get; }
    OrcaExport? OrcaExportObject { get; }

    CreateWizardType WizardType { get; }
    int CurrentPage { get; }

    string ProjectName { get; set; }
    string ClientName { get; set; }
    string SurveyName { get; set; }
    string DataFilePath { get; set; }
    string OrcaXmlPath { get; set; }
    string TemplatePath { get; set; }

    string MissValue1 { get; set; }
    string MissValue2 { get; set; }
    string MissValue3 { get; set; }

    bool UseTemplate { get; }
    bool IsOrcaImport => WizardType == CreateWizardType.ImportFromOrcaIntoExistingProject ||
                         WizardType == CreateWizardType.ImportFromOrcaIntoNewProject;

    void GoToPage(int index);
    void HideBackButton();
    void FocusProjectName();
    void FocusSurveyName();
    void DisableDataFileBrowser();
    void SetTemplateEnabled(bool enabled);
    void UpdateDataLinks(string label, string fullPath);

    event EventHandler OnLoad;
    event EventHandler<WizardPageChangedEventArgs> OnPageChanged;
    event EventHandler<WizardNextEventArgs> OnNext;
    event EventHandler OnFinish;
    event EventHandler OnSeeData;
    event EventHandler OnUseNewDefinition;
    event EventHandler OnUseTemplate;
}