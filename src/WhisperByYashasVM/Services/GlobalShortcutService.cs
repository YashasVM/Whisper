using System.Runtime.InteropServices;
using WhisperByYashasVM.Native;

namespace WhisperByYashasVM.Services;

public sealed class GlobalShortcutService : IDisposable
{
    private NativeMethods.LowLevelKeyboardProc? _proc;
    private IntPtr _hook = IntPtr.Zero;
    private bool _leftWinDown;
    private bool _rightWinDown;
    private bool _isActive;

    public event EventHandler? HotkeyPressed;
    public event EventHandler? HotkeyReleased;

    public void Start()
    {
        _proc = HookCallback;
        _hook = NativeMethods.SetHook(_proc);
    }

    public void Dispose()
    {
        if (_hook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        var keyData = Marshal.PtrToStructure<NativeMethods.KbdLlHookStruct>(lParam);
        int msg = wParam.ToInt32();
        bool keyDown = msg == NativeMethods.WmKeyDown || msg == NativeMethods.WmSysKeyDown;
        bool keyUp = msg == NativeMethods.WmKeyUp || msg == NativeMethods.WmSysKeyUp;

        switch (keyData.vkCode)
        {
            case NativeMethods.VkLWin:
                _leftWinDown = keyDown ? true : keyUp ? false : _leftWinDown;
                break;
            case NativeMethods.VkRWin:
                _rightWinDown = keyDown ? true : keyUp ? false : _rightWinDown;
                break;
            case NativeMethods.VkY:
            {
                if (keyDown && (_leftWinDown || _rightWinDown) && !_isActive)
                {
                    _isActive = true;
                    HotkeyPressed?.Invoke(this, EventArgs.Empty);
                }
                else if (keyUp && _isActive)
                {
                    _isActive = false;
                    HotkeyReleased?.Invoke(this, EventArgs.Empty);
                }

                break;
            }
        }

        if (_isActive && keyUp && (keyData.vkCode == NativeMethods.VkLWin || keyData.vkCode == NativeMethods.VkRWin))
        {
            _isActive = false;
            HotkeyReleased?.Invoke(this, EventArgs.Empty);
        }

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }
}
