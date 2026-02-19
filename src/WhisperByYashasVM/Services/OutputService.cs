using System.Windows.Forms;

namespace WhisperByYashasVM.Services;

public sealed class OutputService
{
    public void Commit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Clipboard.SetText(text);
            SendKeys.SendWait("^v");
        });
    }
}
