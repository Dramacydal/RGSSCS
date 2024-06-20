using RGSSLib;
using System.Text.RegularExpressions;

namespace RGSSGui;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private AbstractArchiveReader? _reader;
    private Action? _previewCallback;
    private Regex? _regexpFilter;

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var res = openFileDialog1.ShowDialog();
        if (res != DialogResult.OK)
            return;

        ReadArchive(openFileDialog1.FileName);
    }

    private void ReadArchive(string path)
    {
        try
        {
            _reader = ArchiveReader.Open(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        UpdateView(path);
    }

    private void UpdateView(string path = "")
    {
        var title = "RGSSCLi";
        if (!string.IsNullOrEmpty(path))
            title += $" - {path}";

        Text = title;
            
        toolStripStatusLabel1.Text = _reader != null ? $"Total files: {_reader.Table.Size}" : "";
        _previewCallback = null;
        SetupInfo(null);
        SetupTree();
        extractAllToolStripMenuItem.Enabled = _reader != null;
        closeToolStripMenuItem.Enabled = _reader != null;
    }

    private void SetupTree()
    {
        treeView1.SuspendLayout();
        treeView1.Nodes.Clear();

        if (_reader == null)
        {
            treeView1.ResumeLayout();
            return;
        }

        Dictionary<string, TreeNode> directories = new();
        foreach (var entry in _reader.Table)
        {
            var path = entry.Path;
            AddNode(path, directories, true);
        }

        treeView1.ResumeLayout();
    }

    private TreeNode CreateNode(string path, bool isFile) => new TreeFileNode(Path.GetFileName(path), path, isFile);

    private TreeNode? AddNode(string path, Dictionary<string, TreeNode> createdTree, bool isFile)
    {
        if (isFile && _regexpFilter != null)
        {
            var file = Path.GetFileName(path);
            if (!_regexpFilter.IsMatch(file))
                return null;
        }

        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directory))
        {
            var node = CreateNode(path, isFile);
            treeView1.Nodes.Add(node);
            return node;
        }

        if (createdTree.TryGetValue(directory.ToLower(), out var item1))
        {
            var node = CreateNode(path, isFile);
            item1.Nodes.Add(node);
            return node;
        }

        var item2 = AddNode(directory, createdTree, false);

        createdTree[directory.ToLower()] = item2;

        var node2 = CreateNode(path, isFile);

        item2.Nodes.Add(node2);

        return node2;
    }

    private void SetupInfo(TreeFileNode? tf)
    {
        if (tf != null && tf.IsFile)
        {
            var entry = _reader?.Table.GetEntry(tf.Path);
            if (entry != null)
            {
                textBoxPath.Text = entry.Path;
                textBoxSize.Text = $"{entry.Size:N0} bytes";

                DoPreview(entry);
                return;
            }
        }

        panel1.BackgroundImage = null;
        textBoxPath.Text = "";
        textBoxSize.Text = "";
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e) => CloseArchive();

    private void CloseArchive()
    {
        _reader?.Dispose();
        _reader = null;
        UpdateView();
    }

    private void DoPreview(TableEntry entry)
    {
        panel1.BackgroundImage = null;

        switch (Path.GetExtension(entry.Path))
        {
            case ".jpg":
            case ".png":
            case ".gif":
            case ".bmp":
            case ".tga":
                break;
            default:
                return;
        }

        InitImage(entry);
    }

    private void InitImage(TableEntry entry)
    {
        var action = () =>
        {
            var stream = _reader.GetFileStream(entry);

            var image = Image.FromStream(stream);
            panel1.BackgroundImage = image;
            panel1.Size = new Size(image.Width, image.Height);
        };

        if (tabControl1.SelectedTab == tabPage2)
            action.Invoke();
        else
            _previewCallback = action;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabControl1.SelectedTab == tabPage2)
            _previewCallback?.Invoke();
    }

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
        SetupInfo(e.Node as TreeFileNode);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        timer1.Stop();
        SetupTree();
        treeView1.ExpandAll();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        timer1.Stop();
        _regexpFilter = null;

        try
        {
            _regexpFilter = new Regex(textBox1.Text.Trim(), RegexOptions.IgnoreCase);
        }
        catch (Exception ex)
        {
            return;
        }

        timer1.Start();
    }

    private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        treeView1.ExpandAll();
    }

    private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        treeView1.CollapseAll();
    }

    private void extractSelectedToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var selectedNode = treeView1.SelectedNode as TreeFileNode;
        if (selectedNode == null)
            return;

        var res = folderBrowserDialog1.ShowDialog(this);
        if (res != DialogResult.OK)
            return;

        var path = folderBrowserDialog1.SelectedPath;

        if (selectedNode.IsFile)
        {
            try
            {
                var entry = _reader.Table.GetEntry(selectedNode.Path);
                if (entry == null)
                    return;
                _reader.Extract(entry, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            var directory = selectedNode.Path;
            var files = _reader.Table.GetEntriesInPath(directory).ToArray();

            if (files.Length > 0)
            {
                var aborted = false;
                DoWithProgress((me) =>
                {
                    var index = 0;

                    try
                    {
                        foreach (var entry in files)
                        {
                            if (aborted)
                                break;
                                
                            me.Invoke(() => me.SetProgress(++index, files.Length, entry.Path));
                            _reader.Extract(entry, path);
                        }

                        me.Invoke(me.Close);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }, () => aborted = true);
            }
        }
    }

    private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var res = folderBrowserDialog1.ShowDialog(this);
        if (res != DialogResult.OK)
            return;

        var path = folderBrowserDialog1.SelectedPath;

        DoWithProgress((me) =>
        {
            try
            {
                _reader.ExtractAll(path, (index, total, s) =>
                {
                    me.Invoke(() => me.SetProgress(index, total, s));
                });
                me.Invoke(me.Close);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }, () => _reader.Abort());
    }

    private void DoWithProgress(Action<ProgressForm> action, Action abortAction)
    {
        var progressForm = new ProgressForm(action, abortAction);
        progressForm.StartPosition = FormStartPosition.CenterParent;

        progressForm.ShowDialog(this);
    }

    private void CreateArchive(ArchiveVersion version)
    {
        var res = folderBrowserDialog1.ShowDialog(this);
        if (res != DialogResult.OK)
            return;

        saveFileDialog1.Filter = version == ArchiveVersion.V3 ? "VX Ace|*.rgss3a" : "VX|*.rgss2a|XP|*.rgssad";

        res = saveFileDialog1.ShowDialog();
        if (res != DialogResult.OK)
            return;

        var aborted = false;
        DoWithProgress((progressForm) =>
        {
            try
            {
                var path = saveFileDialog1.FileName;
                var tmpPath = saveFileDialog1.FileName + ".tmp";
                ArchiveWriter.Encrypt(folderBrowserDialog1.SelectedPath, tmpPath, version, (index, total, s) =>
                {
                    progressForm.Invoke(() => progressForm.SetProgress(index, total, s));
                });

                progressForm.Invoke(progressForm.Close);

                if (aborted)
                    return;
                    
                Invoke(CloseArchive);
                    
                File.Move(tmpPath, path, true);

                Invoke(() => ReadArchive(path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }, () => aborted=true);
    }

    private void v1ToolStripMenuItem_Click(object sender, EventArgs e) => CreateArchive(ArchiveVersion.V1);

    private void v3ToolStripMenuItem_Click(object sender, EventArgs e) => CreateArchive(ArchiveVersion.V3);
}
