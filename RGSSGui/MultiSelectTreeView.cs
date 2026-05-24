using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RGSSGui;

/// <summary>
/// TreeView with multi-select support via Ctrl+click and Shift+click.
/// Selection is managed externally via TreeNodeModel.IsSelected.
/// </summary>
public class MultiSelectTreeView : TreeView
{
    protected override DependencyObject GetContainerForItemOverride() =>
        new MultiSelectTreeViewItem();

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is MultiSelectTreeViewItem;
}

public class MultiSelectTreeViewItem : TreeViewItem
{
    static MultiSelectTreeViewItem()
    {
        // Reuse the default TreeViewItem style so it looks identical
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(typeof(TreeViewItem)));
    }

    protected override DependencyObject GetContainerForItemOverride() =>
        new MultiSelectTreeViewItem();

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is MultiSelectTreeViewItem;

    /// <summary>
    /// Override to do nothing: prevents TreeView from calling IsSelected = true
    /// and triggering ChangeSelection which would clear multi-select.
    /// All selection logic is handled by MultiSelectTreeView's PreviewMouseLeftButtonDown.
    /// </summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
    }
}
