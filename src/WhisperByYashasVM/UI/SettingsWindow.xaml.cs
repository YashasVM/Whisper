using System.Windows;

namespace WhisperByYashasVM.UI;

public partial class SettingsWindow : Window
{
    private bool _allowClose;

    public SettingsWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }
        e.Cancel = true;
        Hide();
    }
}
