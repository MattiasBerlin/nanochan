using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NanoChan
{
    public delegate void ClipboardHandler(string text);

    public class ClipboardListener
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private IntPtr windowHandle;
        private Window Window;
        private ClipboardHandler Callback;

        public event EventHandler ClipboardUpdate;

        public ClipboardListener(Window window, ClipboardHandler callback)
        {
            Window = window;
            Callback = callback;
        }

        // Start listening to clipboard events. Cannot be called before the window is initialized!
        public void Start()
        {
            windowHandle = new WindowInteropHelper(Window).EnsureHandle();
            HwndSource.FromHwnd(windowHandle)?.AddHook(HwndHandler);
            NativeMethods.AddClipboardFormatListener(windowHandle);
        }

        public void Stop()
        {
            NativeMethods.RemoveClipboardFormatListener(windowHandle);
        }

        private static readonly DependencyProperty ClipboardUpdateCommandProperty =
            DependencyProperty.Register("ClipboardUpdateCommand", typeof(ICommand), typeof(Window), new FrameworkPropertyMetadata(null));

        private ICommand ClipboardUpdateCommand
        {
            get { return (ICommand)Window.GetValue(ClipboardUpdateCommandProperty); }
            set { Window.SetValue(ClipboardUpdateCommandProperty, value); }
        }

        protected void OnClipboardUpdate()
        {
            string text = "";
            // Since WPF doesn't retry like WinForms does
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    text = Clipboard.GetText();
                    break;
                }
                catch { }
                System.Threading.Thread.Sleep(50);
            }
            if (text != "")
            {
                Callback(text);
            }
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                // fire event
                this.ClipboardUpdate?.Invoke(this, new EventArgs());
                // execute command
                if (this.ClipboardUpdateCommand?.CanExecute(null) ?? false)
                {
                    this.ClipboardUpdateCommand?.Execute(null);
                }
                // call virtual method
                OnClipboardUpdate();
            }
            handled = false;
            return IntPtr.Zero;
        }


        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        }
    }
}
