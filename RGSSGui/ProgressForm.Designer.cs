namespace RGSSGui;

partial class ProgressForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        progressBar1 = new ProgressBar();
        label1 = new Label();
        label3 = new Label();
        button1 = new Button();
        SuspendLayout();
        // 
        // progressBar1
        // 
        progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar1.Location = new Point(12, 12);
        progressBar1.MarqueeAnimationSpeed = 1;
        progressBar1.Name = "progressBar1";
        progressBar1.Size = new Size(343, 23);
        progressBar1.Step = 1;
        progressBar1.TabIndex = 0;
        // 
        // label1
        // 
        label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        label1.Location = new Point(12, 38);
        label1.Name = "label1";
        label1.Size = new Size(343, 23);
        label1.TabIndex = 1;
        label1.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // label3
        // 
        label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        label3.Location = new Point(12, 64);
        label3.Name = "label3";
        label3.Size = new Size(343, 23);
        label3.TabIndex = 1;
        label3.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // button1
        // 
        button1.Anchor = AnchorStyles.Bottom;
        button1.Location = new Point(140, 90);
        button1.Name = "button1";
        button1.Size = new Size(75, 23);
        button1.TabIndex = 2;
        button1.Text = "Cancel";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // ProgressForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(367, 122);
        Controls.Add(button1);
        Controls.Add(label3);
        Controls.Add(label1);
        Controls.Add(progressBar1);
        Name = "ProgressForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Please wait...";
        Shown += ProgressForm_Shown;
        ResumeLayout(false);
    }

    #endregion

    private ProgressBar progressBar1;
    private Label label1;
    private Label label3;
    private Button button1;
}