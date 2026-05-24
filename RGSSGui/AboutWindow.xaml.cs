using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace RGSSGui;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "?";
        versionLabel.Text = $"RGSSGui {version}";
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void OK_Click(object sender, RoutedEventArgs e) => Close();
}
