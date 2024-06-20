using System.Diagnostics;

namespace RGSSGui;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        ProcessStartInfo sInfo = new()
        {
            FileName = linkLabel1.Text,
            UseShellExecute = true,
        };

        Process.Start(sInfo);
    }
}