using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RGSSGui;

public class TreeNodeModel : INotifyPropertyChanged
{
    private bool _isExpanded;

    public string Text { get; set; } = "";
    public string Path { get; set; } = "";
    public bool IsFile { get; set; }
    public ObservableCollection<TreeNodeModel> Children { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public void SetExpandedRecursive(bool value)
    {
        IsExpanded = value;
        foreach (var child in Children)
            child.SetExpandedRecursive(value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
