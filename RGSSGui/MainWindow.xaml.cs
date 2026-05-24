using Microsoft.Win32;
using RGSSLib;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RGSSGui;

public partial class MainWindow : Window
{
    private AbstractArchiveReader? _reader;
    private Action? _previewCallback;
    private Regex? _regexpFilter;
    private readonly DispatcherTimer _filterTimer;
    private readonly ObservableCollection<TreeNodeModel> _rootNodes = new();

    public MainWindow()
    {
        InitializeComponent();
        treeView.ItemsSource = _rootNodes;
        _filterTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _filterTimer.Tick += (s, e) =>
        {
            _filterTimer.Stop();
            SetupTree();
            ExpandAll();
        };
    }

    private void OpenArchive_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "All files|*.*|VX Ace|*.rgss3a|VX|*.rgss2a|XP|*.rgssad"
        };
        if (dlg.ShowDialog() != true)
            return;
        ReadArchive(dlg.FileName);
    }

    private void ReadArchive(string path)
    {
        try
        {
            _reader = ArchiveReader.Open(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        UpdateView(path);
    }

    private void UpdateView(string path = "")
    {
        Title = string.IsNullOrEmpty(path) ? "RGSSGui" : $"RGSSGui - {path}";
        statusLabel.Text = _reader != null ? $"Total files: {_reader.Table.Size}" : "";
        _previewCallback = null;
        SetupInfo(null);
        SetupTree();
        extractAllMenuItem.IsEnabled = _reader != null;
        closeArchiveMenuItem.IsEnabled = _reader != null;
    }

    private void SetupTree()
    {
        _rootNodes.Clear();
        if (_reader == null)
            return;

        var directories = new Dictionary<string, TreeNodeModel>();
        foreach (var entry in _reader.Table)
            AddNode(entry.Path, directories, true);
    }

    private TreeNodeModel CreateNode(string path, bool isFile) =>
        new() { Text = Path.GetFileName(path), Path = path, IsFile = isFile };

    private TreeNodeModel? AddNode(string path, Dictionary<string, TreeNodeModel> createdTree, bool isFile)
    {
        if (isFile && _regexpFilter != null)
        {
            if (!_regexpFilter.IsMatch(Path.GetFileName(path)))
                return null;
        }

        var directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(directory))
        {
            var node = CreateNode(path, isFile);
            _rootNodes.Add(node);
            return node;
        }

        if (createdTree.TryGetValue(directory.ToLower(), out var parent))
        {
            var node = CreateNode(path, isFile);
            parent.Children.Add(node);
            return node;
        }

        var parentNode = AddNode(directory, createdTree, false)!;
        createdTree[directory.ToLower()] = parentNode;

        var node2 = CreateNode(path, isFile);
        parentNode.Children.Add(node2);
        return node2;
    }

    private void SetupInfo(TreeNodeModel? node)
    {
        if (node != null && node.IsFile)
        {
            var entry = _reader?.Table.GetEntry(node.Path);
            if (entry != null)
            {
                pathTextBox.Text = entry.Path;
                sizeTextBox.Text = $"{entry.Size:N0} bytes";
                DoPreview(entry);
                return;
            }
        }

        previewImage.Source = null;
        pathTextBox.Text = "";
        sizeTextBox.Text = "";
    }

    private void DoPreview(TableEntry entry)
    {
        previewImage.Source = null;
        var ext = Path.GetExtension(entry.Path).ToLower();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp"))
            return;
        InitImage(entry);
    }

    private void InitImage(TableEntry entry)
    {
        var action = () =>
        {
            try
            {
                var stream = _reader!.GetFileStream(entry);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                previewImage.Source = bitmap;
            }
            catch
            {
                previewImage.Source = null;
            }
        };

        if (tabControl.SelectedIndex == 1)
            action();
        else
            _previewCallback = action;
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (tabControl.SelectedIndex == 1)
            _previewCallback?.Invoke();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SetupInfo(e.NewValue as TreeNodeModel);
    }

    private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeViewItem item)
            item.IsSelected = true;
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _filterTimer.Stop();
        _regexpFilter = null;
        try
        {
            _regexpFilter = new Regex(filterTextBox.Text.Trim(), RegexOptions.IgnoreCase);
        }
        catch { }
        _filterTimer.Start();
    }

    private void ClearFilter_Click(object sender, RoutedEventArgs e)
    {
        filterTextBox.Text = "";
    }

    private void ExpandAll() => SetExpandedAll(true);
    private void CollapseAll() => SetExpandedAll(false);

    private void SetExpandedAll(bool expanded)
    {
        foreach (var node in _rootNodes)
            node.IsExpanded = expanded;
    }

    private void ExpandAll_Click(object sender, RoutedEventArgs e) => ExpandAll();
    private void CollapseAll_Click(object sender, RoutedEventArgs e) => CollapseAll();

    private void CloseArchive_Click(object sender, RoutedEventArgs e) => CloseArchive();

    private void CloseArchive()
    {
        _reader?.Dispose();
        _reader = null;
        UpdateView();
    }

    private void ExtractSelected_Click(object sender, RoutedEventArgs e)
    {
        var selectedNode = treeView.SelectedItem as TreeNodeModel;
        if (selectedNode == null)
            return;

        var folderDlg = new OpenFolderDialog();
        if (folderDlg.ShowDialog(this) != true)
            return;

        var path = folderDlg.FolderName;

        if (selectedNode.IsFile)
        {
            try
            {
                var entry = _reader!.Table.GetEntry(selectedNode.Path);
                if (entry == null) return;
                _reader.Extract(entry, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            var files = _reader!.Table.GetEntriesInPath(selectedNode.Path).ToArray();
            if (files.Length == 0) return;

            var aborted = false;
            DoWithProgress(me =>
            {
                var index = 0;
                try
                {
                    foreach (var entry in files)
                    {
                        if (aborted) break;
                        me.Invoke(() => me.SetProgress(++index, files.Length, entry.Path));
                        _reader.Extract(entry, path);
                    }
                    me.Invoke(me.Close);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            }, () => aborted = true);
        }
    }

    private void ExtractAll_Click(object sender, RoutedEventArgs e)
    {
        var folderDlg = new OpenFolderDialog();
        if (folderDlg.ShowDialog(this) != true)
            return;

        var path = folderDlg.FolderName;
        DoWithProgress(me =>
        {
            try
            {
                _reader!.ExtractAll(path, (index, total, s) =>
                    me.Invoke(() => me.SetProgress(index, total, s)));
                me.Invoke(me.Close);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }, () => _reader!.Abort());
    }

    private void DoWithProgress(Action<ProgressWindow> action, Action abortAction)
    {
        var win = new ProgressWindow(action, abortAction) { Owner = this };
        win.ShowDialog();
    }

    private void CreateArchive(ArchiveVersion version)
    {
        var folderDlg = new OpenFolderDialog();
        if (folderDlg.ShowDialog(this) != true)
            return;

        var inputFolder = folderDlg.FolderName;

        var saveDlg = new SaveFileDialog
        {
            Filter = version == ArchiveVersion.V3 ? "VX Ace|*.rgss3a" : "VX|*.rgss2a|XP|*.rgssad"
        };
        if (saveDlg.ShowDialog() != true)
            return;

        var aborted = false;
        DoWithProgress(me =>
        {
            try
            {
                var outPath = saveDlg.FileName;
                var tmpPath = outPath + ".tmp";
                ArchiveWriter.Encrypt(inputFolder, tmpPath, version, (index, total, s) =>
                    me.Invoke(() => me.SetProgress(index, total, s)));

                me.Invoke(me.Close);
                if (aborted) return;

                Dispatcher.Invoke(CloseArchive);
                File.Move(tmpPath, outPath, true);
                Dispatcher.Invoke(() => ReadArchive(outPath));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }, () => aborted = true);
    }

    private void CreateV1_Click(object sender, RoutedEventArgs e) => CreateArchive(ArchiveVersion.V1);
    private void CreateV3_Click(object sender, RoutedEventArgs e) => CreateArchive(ArchiveVersion.V3);

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void About_Click(object sender, RoutedEventArgs e)
    {
        new AboutWindow { Owner = this }.ShowDialog();
    }
}
