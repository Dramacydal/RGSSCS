using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RGSSGui;

public class TreeNodeModel : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isSelected;

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

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
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
