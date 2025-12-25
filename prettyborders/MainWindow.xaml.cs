using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Forms;
using prettyborders.Win32;

namespace prettyborders
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;

        private bool _isBorderVisible = false;


        private bool _borderEnabled = true;
        // private bool _isBorderVisible = false;

        private NotifyIcon? _trayIcon;

        private new const int BorderThickness = 12;

        public MainWindow()
        {
            InitializeComponent();

            MakeClickThrough();
            InitializeTray();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _timer.Tick += SyncOverlay;
            _timer.Start();
        }

        private void SyncOverlay(object? sender, EventArgs e)
        {
            // 🔒 Border disabled → ensure hidden once
            if (!_borderEnabled)
            {
                HideBorder();
                return;
            }

            IntPtr foreground = NativeMethods.GetForegroundWindow();
            IntPtr selfHwnd = new WindowInteropHelper(this).Handle;

            // Invalid / self window
            if (foreground == IntPtr.Zero || foreground == selfHwnd)
            {
                HideBorder();
                return;
            }

            if (!NativeMethods.IsWindow(foreground) || NativeMethods.IsIconic(foreground))
            {
                HideBorder();
                return;
            }

            NativeMethods.GetWindowRect(foreground, out RECT rect);

            if (rect.Width < 100 || rect.Height < 100)
            {
                HideBorder();
                return;
            }

            bool isMaximized = NativeMethods.IsZoomed(foreground);

            ShowBorder();

            if (isMaximized)
            {
                Left = rect.Left;
                Top = rect.Top;
                Width = rect.Width;
                Height = rect.Height;
            }
            else
            {
                Left = rect.Left - BorderThickness;
                Top = rect.Top - BorderThickness;
                Width = rect.Width + BorderThickness * 2;
                Height = rect.Height + BorderThickness * 2;
            }
        }

        // ---------- Visibility helpers (CRITICAL) ----------

        private void ShowBorder()
        {
            if (_isBorderVisible)
                return;

            Show();
            _isBorderVisible = true;
        }

        private void HideBorder()
        {
            if (!_isBorderVisible)
                return;

            Hide();
            _isBorderVisible = false;
        }

        // ---------------------------------------------------

        private void MakeClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int styles = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);

            NativeMethods.SetWindowLong(
                hwnd,
                NativeMethods.GWL_EXSTYLE,
                styles | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT
            );
        }

        private void InitializeTray()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "PrettyBorders"
            };

            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Disable Border");
            toggleItem.Click += (s, e) =>
            {
                _borderEnabled = !_borderEnabled;
                toggleItem.Text = _borderEnabled ? "Disable Border" : "Enable Border";

                if (!_borderEnabled)
                    HideBorder();
            };

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                _trayIcon!.Visible = false;
                _trayIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            };

            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
        }
    }
}
