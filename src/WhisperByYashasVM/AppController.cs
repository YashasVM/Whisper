using System.Windows;
using System.Windows.Forms;
using WhisperByYashasVM.Services;
using WhisperByYashasVM.UI;
using Application = System.Windows.Application;

namespace WhisperByYashasVM;

public sealed class AppController : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly OverlayWindow _overlayWindow;
    private readonly GlobalShortcutService _shortcutService;
    private readonly AudioCaptureService _audioCapture;
    private readonly VadService _vadService;
    private readonly WhisperService _whisperService;
    private readonly OutputService _outputService;
    private readonly SettingsWindow _settingsWindow;
    private bool _isCapturing;

    public AppController()
    {
        _overlayWindow = new OverlayWindow();
        _shortcutService = new GlobalShortcutService();
        _audioCapture = new AudioCaptureService();
        _vadService = new VadService();
        _whisperService = new WhisperService();
        _outputService = new OutputService();
        _settingsWindow = new SettingsWindow();

        _trayIcon = new NotifyIcon
        {
            Text = "Whisper -By YashasVM",
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = BuildMenu()
        };
    }

    public void Start()
    {
        _audioCapture.LevelChanged += OnAudioLevelChanged;
        _shortcutService.HotkeyPressed += OnHotkeyPressed;
        _shortcutService.HotkeyReleased += OnHotkeyReleased;
        _shortcutService.Start();
        _trayIcon.BalloonTipTitle = "Whisper -By YashasVM";
        _trayIcon.BalloonTipText = "App is running in tray. Hold Win+Y to talk.";
        _trayIcon.ShowBalloonTip(3500);
        _ = _whisperService.EnsureModelAsync();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings", null, (_, _) =>
        {
            if (!_settingsWindow.IsVisible)
            {
                _settingsWindow.Show();
            }
            _settingsWindow.Activate();
        });
        menu.Items.Add("Exit", null, (_, _) => Application.Current.Shutdown());
        return menu;
    }

    private void OnAudioLevelChanged(float rms)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _overlayWindow.UpdateWaveform(rms);
            _overlayWindow.SetSpeechState(_vadService.IsSpeech(rms));
        });
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (_isCapturing)
        {
            return;
        }

        _isCapturing = true;
        Application.Current.Dispatcher.Invoke(() =>
        {
            _overlayWindow.ShowListening();
        });
        _audioCapture.StartCapture();
    }

    private async void OnHotkeyReleased(object? sender, EventArgs e)
    {
        if (!_isCapturing)
        {
            return;
        }

        _isCapturing = false;
        var wavData = _audioCapture.StopCapture();
        if (wavData.Length == 0)
        {
            Application.Current.Dispatcher.Invoke(() => _overlayWindow.HideOverlay());
            return;
        }

        Application.Current.Dispatcher.Invoke(() => _overlayWindow.ShowTranscribing());

        string text;
        try
        {
            text = await _whisperService.TranscribeAsync(wavData);
        }
        catch (Exception ex)
        {
            text = $"Transcription failed: {ex.Message}";
        }

        Application.Current.Dispatcher.Invoke(() => _overlayWindow.ShowFinalText(text));
        await Task.Delay(250);
        _outputService.Commit(text);
        await Task.Delay(350);
        Application.Current.Dispatcher.Invoke(() => _overlayWindow.HideOverlay());
    }

    public void Dispose()
    {
        _shortcutService.Dispose();
        _audioCapture.Dispose();
        _settingsWindow.ForceClose();
        _overlayWindow.Close();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
    }
}
