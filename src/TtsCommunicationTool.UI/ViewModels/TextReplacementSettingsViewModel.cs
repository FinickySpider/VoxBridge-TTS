using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Win32;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class TextReplacementRuleViewModel : ViewModelBase
{
    private string _triggerText = string.Empty;
    private string _replacementText = string.Empty;
    private bool _isEnabled = true;
    private bool _isCaseSensitive = false;
    private bool _wholeWordOnly = false;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string TriggerText
    {
        get => _triggerText;
        set => SetField(ref _triggerText, value);
    }

    public string ReplacementText
    {
        get => _replacementText;
        set => SetField(ref _replacementText, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public bool IsCaseSensitive
    {
        get => _isCaseSensitive;
        set => SetField(ref _isCaseSensitive, value);
    }

    public bool WholeWordOnly
    {
        get => _wholeWordOnly;
        set => SetField(ref _wholeWordOnly, value);
    }

    internal string SourceId { get; private set; } = Guid.NewGuid().ToString();

    internal static TextReplacementRuleViewModel FromModel(TextReplacement r)
    {
        return new TextReplacementRuleViewModel
        {
            SourceId = r.Id,
            _triggerText = r.TriggerText,
            _replacementText = r.ReplacementText,
            _isEnabled = r.IsEnabled,
            _isCaseSensitive = r.IsCaseSensitive,
            _wholeWordOnly = r.WholeWordOnly,
        };
    }

    internal TextReplacement ToModel(int sortOrder) => new()
    {
        Id = SourceId,
        TriggerText = TriggerText,
        ReplacementText = ReplacementText,
        IsEnabled = IsEnabled,
        IsCaseSensitive = IsCaseSensitive,
        WholeWordOnly = WholeWordOnly,
        SortOrder = sortOrder,
    };
}

public sealed class TextReplacementSettingsViewModel : ViewModelBase
{
    private bool _isEnabled = true;
    private TextReplacementRuleViewModel? _selectedRule;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public ObservableCollection<TextReplacementRuleViewModel> Rules { get; } = [];

    public TextReplacementRuleViewModel? SelectedRule
    {
        get => _selectedRule;
        set
        {
            if (SetField(ref _selectedRule, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public TextReplacementSettingsViewModel()
    {
        AddCommand = new RelayCommand(AddRule);
        DeleteCommand = new RelayCommand(DeleteRule, () => SelectedRule is not null);
        MoveUpCommand = new RelayCommand(MoveUp, CanMoveUp);
        MoveDownCommand = new RelayCommand(MoveDown, CanMoveDown);
        ExportCommand = new RelayCommand(ExportRules, () => Rules.Count > 0);
        ImportCommand = new RelayCommand(ImportRules);
    }

    public void LoadFrom(TextReplacementSettings s)
    {
        IsEnabled = s.IsEnabled;
        Rules.Clear();
        foreach (var r in s.Rules.OrderBy(r => r.SortOrder))
            Rules.Add(TextReplacementRuleViewModel.FromModel(r));
    }

    public void ApplyTo(TextReplacementSettings s)
    {
        s.IsEnabled = IsEnabled;
        s.Rules.Clear();
        for (int i = 0; i < Rules.Count; i++)
            s.Rules.Add(Rules[i].ToModel(i));
    }

    private void AddRule()
    {
        var vm = new TextReplacementRuleViewModel();
        Rules.Add(vm);
        SelectedRule = vm;
    }

    private void DeleteRule()
    {
        if (SelectedRule is null) return;
        var idx = Rules.IndexOf(SelectedRule);
        Rules.Remove(SelectedRule);
        if (Rules.Count > 0)
            SelectedRule = Rules[Math.Min(idx, Rules.Count - 1)];
        else
            SelectedRule = null;
    }

    private bool CanMoveUp() => SelectedRule is not null && Rules.IndexOf(SelectedRule) > 0;
    private bool CanMoveDown() => SelectedRule is not null && Rules.IndexOf(SelectedRule) < Rules.Count - 1;

    private void MoveUp()
    {
        if (SelectedRule is null) return;
        var idx = Rules.IndexOf(SelectedRule);
        if (idx <= 0) return;
        Rules.Move(idx, idx - 1);
        CommandManager.InvalidateRequerySuggested();
    }

    private void MoveDown()
    {
        if (SelectedRule is null) return;
        var idx = Rules.IndexOf(SelectedRule);
        if (idx >= Rules.Count - 1) return;
        Rules.Move(idx, idx + 1);
        CommandManager.InvalidateRequerySuggested();
    }

    private void ExportRules()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export Text Replacements",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
            FileName = "tts-replacements"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var models = Rules.Select((r, i) => r.ToModel(i)).ToList();
            var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dlg.FileName, json, System.Text.Encoding.UTF8);
            StatusMessage = $"Exported {Rules.Count} rule(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private void ImportRules()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Import Text Replacements",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var json = File.ReadAllText(dlg.FileName, System.Text.Encoding.UTF8);
            var models = JsonSerializer.Deserialize<List<TextReplacement>>(json);
            if (models is null) { StatusMessage = "Import failed: invalid file."; return; }
            foreach (var r in models.OrderBy(r => r.SortOrder))
                Rules.Add(TextReplacementRuleViewModel.FromModel(r));
            StatusMessage = $"Imported {models.Count} rule(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import failed: {ex.Message}";
        }
    }
}
