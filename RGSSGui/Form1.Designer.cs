namespace RGSSGui;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        createArchiveToolStripMenuItem = new ToolStripMenuItem();
        v1ToolStripMenuItem = new ToolStripMenuItem();
        v3ToolStripMenuItem = new ToolStripMenuItem();
        closeToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        extractAllToolStripMenuItem = new ToolStripMenuItem();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        statusStrip1 = new StatusStrip();
        toolStripStatusLabel1 = new ToolStripStatusLabel();
        splitContainer1 = new SplitContainer();
        textBox1 = new TextBox();
        treeView1 = new TreeView();
        contextMenuStrip1 = new ContextMenuStrip(components);
        extractSelectedToolStripMenuItem = new ToolStripMenuItem();
        expandAllToolStripMenuItem = new ToolStripMenuItem();
        collapseAllToolStripMenuItem = new ToolStripMenuItem();
        tabControl1 = new TabControl();
        tabPage1 = new TabPage();
        textBoxPath = new TextBox();
        textBoxSize = new TextBox();
        label2 = new Label();
        label1 = new Label();
        tabPage2 = new TabPage();
        panel1 = new Panel();
        openFileDialog1 = new OpenFileDialog();
        timer1 = new System.Windows.Forms.Timer(components);
        folderBrowserDialog1 = new FolderBrowserDialog();
        saveFileDialog1 = new SaveFileDialog();
        menuStrip1.SuspendLayout();
        statusStrip1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        contextMenuStrip1.SuspendLayout();
        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        tabPage2.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, extractAllToolStripMenuItem, aboutToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(770, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, createArchiveToolStripMenuItem, closeToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // openToolStripMenuItem
        // 
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.Size = new Size(180, 22);
        openToolStripMenuItem.Text = "Open archive";
        openToolStripMenuItem.Click += openToolStripMenuItem_Click;
        // 
        // createArchiveToolStripMenuItem
        // 
        createArchiveToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { v3ToolStripMenuItem, v1ToolStripMenuItem });
        createArchiveToolStripMenuItem.Name = "createArchiveToolStripMenuItem";
        createArchiveToolStripMenuItem.Size = new Size(180, 22);
        createArchiveToolStripMenuItem.Text = "Create archive";
        // 
        // v1ToolStripMenuItem
        // 
        v1ToolStripMenuItem.Name = "v1ToolStripMenuItem";
        v1ToolStripMenuItem.Size = new Size(180, 22);
        v1ToolStripMenuItem.Text = "V1";
        v1ToolStripMenuItem.Click += v1ToolStripMenuItem_Click;
        // 
        // v3ToolStripMenuItem
        // 
        v3ToolStripMenuItem.Name = "v3ToolStripMenuItem";
        v3ToolStripMenuItem.Size = new Size(180, 22);
        v3ToolStripMenuItem.Text = "V3";
        v3ToolStripMenuItem.Click += v3ToolStripMenuItem_Click;
        // 
        // closeToolStripMenuItem
        // 
        closeToolStripMenuItem.Enabled = false;
        closeToolStripMenuItem.Name = "closeToolStripMenuItem";
        closeToolStripMenuItem.Size = new Size(180, 22);
        closeToolStripMenuItem.Text = "Close archive";
        closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(180, 22);
        exitToolStripMenuItem.Text = "Exit";
        exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
        // 
        // extractAllToolStripMenuItem
        // 
        extractAllToolStripMenuItem.Enabled = false;
        extractAllToolStripMenuItem.Name = "extractAllToolStripMenuItem";
        extractAllToolStripMenuItem.Size = new Size(70, 20);
        extractAllToolStripMenuItem.Text = "Extract all";
        extractAllToolStripMenuItem.Click += extractAllToolStripMenuItem_Click;
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(52, 20);
        aboutToolStripMenuItem.Text = "About";
        // 
        // statusStrip1
        // 
        statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
        statusStrip1.Location = new Point(0, 559);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(770, 22);
        statusStrip1.TabIndex = 1;
        statusStrip1.Text = "statusStrip1";
        // 
        // toolStripStatusLabel1
        // 
        toolStripStatusLabel1.Name = "toolStripStatusLabel1";
        toolStripStatusLabel1.Size = new Size(0, 17);
        // 
        // splitContainer1
        // 
        splitContainer1.Dock = DockStyle.Fill;
        splitContainer1.FixedPanel = FixedPanel.Panel1;
        splitContainer1.Location = new Point(0, 24);
        splitContainer1.Name = "splitContainer1";
        // 
        // splitContainer1.Panel1
        // 
        splitContainer1.Panel1.Controls.Add(textBox1);
        splitContainer1.Panel1.Controls.Add(treeView1);
        // 
        // splitContainer1.Panel2
        // 
        splitContainer1.Panel2.Controls.Add(tabControl1);
        splitContainer1.Size = new Size(770, 535);
        splitContainer1.SplitterDistance = 261;
        splitContainer1.TabIndex = 2;
        // 
        // textBox1
        // 
        textBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        textBox1.Location = new Point(3, 508);
        textBox1.Name = "textBox1";
        textBox1.PlaceholderText = "Regexp filter";
        textBox1.Size = new Size(257, 23);
        textBox1.TabIndex = 1;
        textBox1.TextChanged += textBox1_TextChanged;
        // 
        // treeView1
        // 
        treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        treeView1.ContextMenuStrip = contextMenuStrip1;
        treeView1.HotTracking = true;
        treeView1.Location = new Point(0, 0);
        treeView1.Name = "treeView1";
        treeView1.PathSeparator = "/";
        treeView1.Size = new Size(261, 502);
        treeView1.TabIndex = 0;
        treeView1.AfterSelect += treeView1_AfterSelect;
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.Items.AddRange(new ToolStripItem[] { extractSelectedToolStripMenuItem, expandAllToolStripMenuItem, collapseAllToolStripMenuItem });
        contextMenuStrip1.Name = "contextMenuStrip1";
        contextMenuStrip1.Size = new Size(157, 70);
        // 
        // extractSelectedToolStripMenuItem
        // 
        extractSelectedToolStripMenuItem.Name = "extractSelectedToolStripMenuItem";
        extractSelectedToolStripMenuItem.Size = new Size(156, 22);
        extractSelectedToolStripMenuItem.Text = "Extract selected";
        extractSelectedToolStripMenuItem.Click += extractSelectedToolStripMenuItem_Click;
        // 
        // expandAllToolStripMenuItem
        // 
        expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
        expandAllToolStripMenuItem.Size = new Size(156, 22);
        expandAllToolStripMenuItem.Text = "Expand all";
        expandAllToolStripMenuItem.Click += expandAllToolStripMenuItem_Click;
        // 
        // collapseAllToolStripMenuItem
        // 
        collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
        collapseAllToolStripMenuItem.Size = new Size(156, 22);
        collapseAllToolStripMenuItem.Text = "Collapse all";
        collapseAllToolStripMenuItem.Click += collapseAllToolStripMenuItem_Click;
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(0, 0);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(505, 535);
        tabControl1.TabIndex = 0;
        tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(textBoxPath);
        tabPage1.Controls.Add(textBoxSize);
        tabPage1.Controls.Add(label2);
        tabPage1.Controls.Add(label1);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(3);
        tabPage1.Size = new Size(497, 507);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Info";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // textBoxPath
        // 
        textBoxPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textBoxPath.Location = new Point(62, 3);
        textBoxPath.Name = "textBoxPath";
        textBoxPath.ReadOnly = true;
        textBoxPath.Size = new Size(427, 23);
        textBoxPath.TabIndex = 0;
        // 
        // textBoxSize
        // 
        textBoxSize.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textBoxSize.Location = new Point(62, 32);
        textBoxSize.Name = "textBoxSize";
        textBoxSize.ReadOnly = true;
        textBoxSize.Size = new Size(427, 23);
        textBoxSize.TabIndex = 0;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(6, 32);
        label2.MinimumSize = new Size(50, 23);
        label2.Name = "label2";
        label2.Size = new Size(50, 23);
        label2.TabIndex = 1;
        label2.Text = "Size:";
        label2.TextAlign = ContentAlignment.MiddleRight;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(6, 2);
        label1.MinimumSize = new Size(50, 23);
        label1.Name = "label1";
        label1.Size = new Size(50, 23);
        label1.TabIndex = 1;
        label1.Text = "Path:";
        label1.TextAlign = ContentAlignment.MiddleRight;
        // 
        // tabPage2
        // 
        tabPage2.AutoScroll = true;
        tabPage2.Controls.Add(panel1);
        tabPage2.Location = new Point(4, 24);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(3);
        tabPage2.Size = new Size(497, 507);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Preview";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // panel1
        // 
        panel1.BackgroundImageLayout = ImageLayout.None;
        panel1.Location = new Point(3, 3);
        panel1.Name = "panel1";
        panel1.Size = new Size(451, 472);
        panel1.TabIndex = 0;
        // 
        // openFileDialog1
        // 
        openFileDialog1.FileName = "openFileDialog1";
        openFileDialog1.Filter = "All files|*.*|VX Ace|*.rgss3a|VX|*.rgss2a|XP|*.rgssad";
        // 
        // timer1
        // 
        timer1.Interval = 1000;
        timer1.Tick += timer1_Tick;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(770, 581);
        Controls.Add(splitContainer1);
        Controls.Add(statusStrip1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "Form1";
        Text = "RGSSGui";
        Load += Form1_Load;
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel1.PerformLayout();
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        contextMenuStrip1.ResumeLayout(false);
        tabControl1.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        tabPage2.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem closeToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private StatusStrip statusStrip1;
    private SplitContainer splitContainer1;
    private TreeView treeView1;
    private OpenFileDialog openFileDialog1;
    private ToolStripStatusLabel toolStripStatusLabel1;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private Label label1;
    private TextBox textBoxPath;
    private Label label2;
    private TextBox textBoxSize;
    private Panel panel1;
    private TextBox textBox1;
    private System.Windows.Forms.Timer timer1;
    private ContextMenuStrip contextMenuStrip1;
    private ToolStripMenuItem expandAllToolStripMenuItem;
    private ToolStripMenuItem collapseAllToolStripMenuItem;
    private ToolStripMenuItem extractSelectedToolStripMenuItem;
    private ToolStripMenuItem extractAllToolStripMenuItem;
    private FolderBrowserDialog folderBrowserDialog1;
    private ToolStripMenuItem createArchiveToolStripMenuItem;
    private ToolStripMenuItem v1ToolStripMenuItem;
    private ToolStripMenuItem v3ToolStripMenuItem;
    private SaveFileDialog saveFileDialog1;
}