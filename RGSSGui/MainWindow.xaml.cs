using Microsoft.Win32;
using RGSSLib;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RGSSGui;

public partial class MainWindow : Window
{
    private AbstractArchiveReader? _reader;
    private Action? _previewCallback;
    private Regex? _regexpFilter;
    private readonly DispatcherTimer _filterTimer;
    private bool _suppressFilterUpdate;
    private readonly ObservableCollection<TreeNodeModel> _rootNodes = new();
    private double _zoom = 1.0;
    private string _previewBaseInfo = "";
    private Point _panStart;
    private Vector _panScrollStart;
    private bool _isPanning;
    private TreeNodeModel? _anchorNode;
    private bool _suppressSelectedItemChanged;
    private List<string> _recentFiles = new();

    private static string AppVersion =>
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "?";

    private static string RecentFilesPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RGSSGui", "recent.json");

    private const int MaxRecentFiles = 10;

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
        LoadRecentFiles();
        RebuildRecentMenu();

        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
            Loaded += (s, e) => ReadArchive(args[1]);
    }

    private void LoadRecentFiles()
    {
        try
        {
            if (File.Exists(RecentFilesPath))
                _recentFiles = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RecentFilesPath)) ?? new();
        }
        catch { _recentFiles = new(); }
    }

    private void SaveRecentFiles()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RecentFilesPath)!);
            File.WriteAllText(RecentFilesPath, JsonSerializer.Serialize(_recentFiles));
        }
        catch { }
    }

    private void AddToRecent(string path)
    {
        _recentFiles.Remove(path);
        _recentFiles.Insert(0, path);
        if (_recentFiles.Count > MaxRecentFiles)
            _recentFiles.RemoveRange(MaxRecentFiles, _recentFiles.Count - MaxRecentFiles);
        SaveRecentFiles();
        RebuildRecentMenu();
    }

    private void RebuildRecentMenu()
    {
        openRecentMenuItem.Items.Clear();
        foreach (var path in _recentFiles)
        {
            var item = new MenuItem { Header = path };
            item.Click += (_, _) => ReadArchive(path);
            openRecentMenuItem.Items.Add(item);
        }
        if (_recentFiles.Count > 0)
        {
            openRecentMenuItem.Items.Add(new Separator());
            var clearItem = new MenuItem { Header = "Clear recent" };
            clearItem.Click += (_, _) =>
            {
                _recentFiles.Clear();
                SaveRecentFiles();
                RebuildRecentMenu();
            };
            openRecentMenuItem.Items.Add(clearItem);
        }
        openRecentMenuItem.IsEnabled = _recentFiles.Count > 0;
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
        AddToRecent(path);
        UpdateView(path);
    }

    private void UpdateView(string path = "")
    {
        Title = string.IsNullOrEmpty(path) ? $"RGSSGui {AppVersion}" : $"RGSSGui {AppVersion} - {path}";
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
        _anchorNode = null;
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
            bool matches = _regexpFilter.IsMatch(Path.GetFileName(path));
            bool inverted = invertFilterCheckBox.IsChecked == true;
            if (matches == inverted)
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
        previewImage.LayoutTransform = null;
        previewImage.Cursor = null;
        _previewBaseInfo = "";
        _zoom = 1.0;
        _isPanning = false;
        previewInfoLabel.Text = "";
        pathTextBox.Text = "";
        sizeTextBox.Text = "";
    }

    private void SetPreviewInfo(string path, int w, int h)
    {
        _previewBaseInfo = $"{path}  {w} × {h}";
        UpdatePreviewInfoLabel();
    }

    private void UpdatePreviewInfoLabel()
    {
        previewInfoLabel.Text = string.IsNullOrEmpty(_previewBaseInfo)
            ? ""
            : $"{_previewBaseInfo}  {_zoom * 100:F0}%";
    }

    private void SetZoom(double zoom, Point? anchor = null)
    {
        var oldZoom = _zoom;
        _zoom = Math.Clamp(zoom, 0.05, 20.0);
        if (Math.Abs(_zoom - 1.0) < 0.02) _zoom = 1.0;

        double? newScrollX = null, newScrollY = null;
        if (anchor.HasValue)
        {
            var scale = _zoom / oldZoom;
            newScrollX = (previewScrollViewer.HorizontalOffset + anchor.Value.X) * scale - anchor.Value.X;
            newScrollY = (previewScrollViewer.VerticalOffset + anchor.Value.Y) * scale - anchor.Value.Y;
        }

        previewImage.LayoutTransform = new ScaleTransform(_zoom, _zoom);
        UpdatePreviewInfoLabel();

        if (newScrollX.HasValue)
        {
            previewScrollViewer.UpdateLayout();
            previewScrollViewer.ScrollToHorizontalOffset(newScrollX.Value);
            previewScrollViewer.ScrollToVerticalOffset(newScrollY!.Value);
        }
    }

    private void PreviewScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (previewImage.Source == null) return;
        e.Handled = true;
        SetZoom(_zoom * (e.Delta > 0 ? 1.1 : 1.0 / 1.1), e.GetPosition(previewScrollViewer));
    }

    private void PreviewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            _isPanning = false;
            previewImage.ReleaseMouseCapture();
            previewImage.Cursor = Cursors.Hand;
            SetZoom(1.0);
            return;
        }
        if (previewImage.Source == null) return;
        _panStart = e.GetPosition(previewScrollViewer);
        _panScrollStart = new Vector(previewScrollViewer.HorizontalOffset, previewScrollViewer.VerticalOffset);
        _isPanning = true;
        previewImage.CaptureMouse();
        previewImage.Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    private void PreviewImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning) return;
        var delta = e.GetPosition(previewScrollViewer) - _panStart;
        previewScrollViewer.ScrollToHorizontalOffset(_panScrollStart.X - delta.X);
        previewScrollViewer.ScrollToVerticalOffset(_panScrollStart.Y - delta.Y);
    }

    private void PreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isPanning) return;
        _isPanning = false;
        previewImage.ReleaseMouseCapture();
        previewImage.Cursor = Cursors.Hand;
    }

    private void DoPreview(TableEntry entry)
    {
        previewImage.Source = null;
        _previewBaseInfo = "";
        _zoom = 1.0;
        previewImage.LayoutTransform = null;
        previewInfoLabel.Text = "";
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
                var frame = BitmapDecoder.Create(stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad).Frames[0];

                BitmapSource source;
                if (Math.Abs(frame.DpiX - 96) > 0.5 || Math.Abs(frame.DpiY - 96) > 0.5)
                {
                    var stride = frame.PixelWidth * ((frame.Format.BitsPerPixel + 7) / 8);
                    var pixels = new byte[frame.PixelHeight * stride];
                    frame.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(frame.PixelWidth, frame.PixelHeight,
                        96, 96, frame.Format, frame.Palette, pixels, stride);
                }
                else
                {
                    source = frame;
                }
                source.Freeze();
                previewImage.Source = source;
                previewImage.Cursor = Cursors.Hand;
                SetPreviewInfo(entry.Path, frame.PixelWidth, frame.PixelHeight);
            }
            catch
            {
                previewImage.Source = null;
                previewInfoLabel.Text = "";
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

    // Keyboard navigation: revert to single-select and sync model state
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_suppressSelectedItemChanged) return;
        var node = e.NewValue as TreeNodeModel;
        ClearSelection();
        if (node != null) { node.IsSelected = true; _anchorNode = node; }
        SetupInfo(node);
    }

    private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Let expander toggle pass through untouched
        if (HasAncestorOfType<ToggleButton>(e.OriginalSource as DependencyObject))
            return;

        var item = FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject);
        if (item?.DataContext is not TreeNodeModel node) return;

        var ctrl  = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        var shift = (Keyboard.Modifiers & ModifierKeys.Shift)   != 0;

        if (ctrl)
        {
            node.IsSelected = !node.IsSelected;
            if (node.IsSelected) _anchorNode = node;
        }
        else if (shift && _anchorNode != null)
        {
            SelectRange(_anchorNode, node);
        }
        else
        {
            ClearSelection();
            node.IsSelected = true;
            _anchorNode = node;
        }

        _suppressSelectedItemChanged = true;
        SetupInfo(node);
        _suppressSelectedItemChanged = false;
        e.Handled = true;
    }

    private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Don't change selection on right-click — context menu operates on current selection.
        // Open the TreeView's context menu manually since we consume the event.
        if (treeView.ContextMenu != null)
        {
            treeView.ContextMenu.PlacementTarget = treeView;
            treeView.ContextMenu.IsOpen = true;
        }
        e.Handled = true;
    }

    // ── selection helpers ────────────────────────────────────────────────────

    private IEnumerable<TreeNodeModel> GetAllNodes()
    {
        var stack = new Stack<TreeNodeModel>(_rootNodes);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;
            foreach (var child in n.Children) stack.Push(child);
        }
    }

    private List<TreeNodeModel> GetVisibleNodes()
    {
        var list = new List<TreeNodeModel>();
        foreach (var root in _rootNodes) CollectVisible(root, list);
        return list;
    }

    private static void CollectVisible(TreeNodeModel node, List<TreeNodeModel> list)
    {
        list.Add(node);
        if (node.IsExpanded)
            foreach (var child in node.Children) CollectVisible(child, list);
    }

    private void ClearSelection()
    {
        foreach (var n in GetAllNodes()) n.IsSelected = false;
    }

    private void SelectRange(TreeNodeModel from, TreeNodeModel to)
    {
        var visible = GetVisibleNodes();
        var a = visible.IndexOf(from);
        var b = visible.IndexOf(to);
        if (a < 0 || b < 0) return;
        ClearSelection();
        for (var i = Math.Min(a, b); i <= Math.Max(a, b); i++)
            visible[i].IsSelected = true;
    }

    private static T? FindAncestor<T>(DependencyObject? d) where T : DependencyObject
    {
        while (d != null) { if (d is T t) return t; d = VisualTreeHelper.GetParent(d); }
        return null;
    }

    private static bool HasAncestorOfType<T>(DependencyObject? d) where T : DependencyObject
    {
        while (d != null) { if (d is T) return true; d = VisualTreeHelper.GetParent(d); }
        return false;
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressFilterUpdate) return;
        _filterTimer.Stop();
        _regexpFilter = null;
        try
        {
            _regexpFilter = new Regex(filterTextBox.Text.Trim(), RegexOptions.IgnoreCase);
        }
        catch { }
        _filterTimer.Start();
    }

    private void InvertFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressFilterUpdate) return;
        if (_regexpFilter != null)
        {
            SetupTree();
            ExpandAll();
        }
    }

    private void ResetFilter_Click(object sender, RoutedEventArgs e)
    {
        _suppressFilterUpdate = true;
        try
        {
            _filterTimer.Stop();
            invertFilterCheckBox.IsChecked = false;
            filterTextBox.Text = "";
            _regexpFilter = null;
        }
        finally
        {
            _suppressFilterUpdate = false;
        }
        SetupTree();
        ExpandAll();
    }

    private void ExpandAll() => SetExpandedAll(true);
    private void CollapseAll() => SetExpandedAll(false);

    private void SetExpandedAll(bool expanded)
    {
        foreach (var node in _rootNodes)
            node.SetExpandedRecursive(expanded);
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

    private TableEntry[] GetSelectedEntries()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<TableEntry>();
        foreach (var node in GetAllNodes().Where(n => n.IsSelected))
        {
            if (node.IsFile)
            {
                var entry = _reader!.Table.GetEntry(node.Path);
                if (entry != null && seen.Add(entry.Path)) result.Add(entry);
            }
            else
            {
                foreach (var entry in _reader!.Table.GetEntriesInPath(node.Path))
                    if (seen.Add(entry.Path)) result.Add(entry);
            }
        }
        return result.ToArray();
    }

    private void ExtractSelected_Click(object sender, RoutedEventArgs e)
    {
        var folderDlg = new OpenFolderDialog();
        if (folderDlg.ShowDialog(this) != true) return;

        var destPath = folderDlg.FolderName;
        var entries = GetSelectedEntries();
        if (entries.Length == 0) return;

        if (entries.Length == 1)
        {
            try { _reader!.Extract(entries[0], destPath); }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            return;
        }

        var aborted = false;
        DoWithProgress(me =>
        {
            var index = 0;
            try
            {
                foreach (var entry in entries)
                {
                    if (aborted) break;
                    me.Invoke(() => me.SetProgress(++index, entries.Length, entry.Path));
                    _reader!.Extract(entry, destPath);
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

    private void ExtractSelectedFlat_Click(object sender, RoutedEventArgs e)
    {
        var folderDlg = new OpenFolderDialog();
        if (folderDlg.ShowDialog(this) != true) return;

        var destPath = folderDlg.FolderName;
        var entries = GetSelectedEntries();
        if (entries.Length == 0) return;

        if (entries.Length == 1)
        {
            try
            {
                File.WriteAllBytes(
                    Path.Combine(destPath, Path.GetFileName(entries[0].Path)),
                    _reader!.GetFileContent(entries[0]).ToArray());
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            return;
        }

        var aborted = false;
        DoWithProgress(me =>
        {
            var index = 0;
            try
            {
                foreach (var entry in entries)
                {
                    if (aborted) break;
                    me.Invoke(() => me.SetProgress(++index, entries.Length, entry.Path));
                    File.WriteAllBytes(
                        Path.Combine(destPath, Path.GetFileName(entry.Path)),
                        _reader!.GetFileContent(entry).ToArray());
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
            var outPath = saveDlg.FileName;
            var tmpPath = outPath + ".tmp";
            try
            {
                ArchiveWriter.Encrypt(inputFolder, tmpPath, version, (index, total, s) =>
                    me.Invoke(() => me.SetProgress(index, total, s)), tmpPath);

                me.Invoke(me.Close);
                if (aborted) return;

                Dispatcher.Invoke(CloseArchive);
                File.Move(tmpPath, outPath, true);
                Dispatcher.Invoke(() => ReadArchive(outPath));
            }
            catch (Exception ex)
            {
                if (File.Exists(tmpPath)) try { File.Delete(tmpPath); } catch { }
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

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, uint uFlags, nint dwItem1, nint dwItem2);

    private void RegisterFileAssociations_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName!;
            const string progId = "RGSSGui.Archive";

            using (var cls = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
            {
                cls.SetValue("", "RGSS Archive");
                using var icon = cls.CreateSubKey("DefaultIcon");
                icon.SetValue("", $"\"{exePath}\",0");
                using var cmd = cls.CreateSubKey(@"shell\open\command");
                cmd.SetValue("", $"\"{exePath}\" \"%1\"");
            }

            string[] extensions = [".rgss3a", ".rgss2a", ".rgssad"];
            foreach (var ext in extensions)
            {
                using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ext}");
                key.SetValue("", progId);
            }

            SHChangeNotify(0x08000000, 0x0000, nint.Zero, nint.Zero);
            MessageBox.Show("File associations registered successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to register file associations:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
