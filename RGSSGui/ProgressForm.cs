namespace RGSSGui;

public partial class ProgressForm : Form
{
    private readonly Action<ProgressForm> _shownAction;
    private readonly Action _abortAction;

    public ProgressForm(Action<ProgressForm> shownAction, Action abortAction)
    {
        InitializeComponent();
        _shownAction = shownAction;
        _abortAction = abortAction;
    }

    public void SetProgress(int current, int max, string path)
    {
        if (progressBar1.Maximum != max)
            progressBar1.Maximum = max;

        // hack animation
        progressBar1.Value = current;
        progressBar1.Value = current - 1;
        progressBar1.Value = current;
            
        // progressBar1.Value = max;
        label3.Text = $"{current} / {max}";

        if (current == max)
            return;

        if (path.Length > 0)
            label1.Text = path;
    }

    private void ProgressForm_Shown(object sender, EventArgs e) => Task.Run(() => _shownAction?.Invoke(this));

    private void button1_Click(object sender, EventArgs e) => _abortAction?.Invoke();
}