using System.Windows;

namespace RGSSGui;

public partial class ProgressWindow : Window
{
    private readonly Action<ProgressWindow> _shownAction;
    private readonly Action _abortAction;

    public ProgressWindow(Action<ProgressWindow> shownAction, Action abortAction)
    {
        InitializeComponent();
        _shownAction = shownAction;
        _abortAction = abortAction;
    }

    public void SetProgress(int current, int max, string path)
    {
        progressBar.Maximum = max;
        progressBar.Value = current;
        countLabel.Text = $"{current} / {max}";
        if (path.Length > 0 && current != max)
            pathLabel.Text = path;
    }

    public void Invoke(Action action) => Dispatcher.Invoke(action);

    private void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(() => _shownAction?.Invoke(this));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => _abortAction?.Invoke();
}
