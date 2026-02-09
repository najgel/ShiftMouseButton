using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShiftMouseButton
{
    internal static class Program
    {
        // Windows API declarations
        [DllImport("user32.dll")]
        public static extern bool SwapMouseButton(bool fSwap);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int SM_SWAPBUTTON = 23;
        public const int WM_HOTKEY = 0x0312;
        public const int HOTKEY_ID = 1;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_ALT = 0x0001;
        public const uint VK_M = 0x4D;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var startupService = new StartupService(() => Application.ExecutablePath);
            Application.Run(new MouseButtonSwitcher(startupService));
        }
    }

    public class MouseButtonSwitcher : Form
    {
        private readonly IStartupService _startupService;
        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenu;
        private ToolStripMenuItem? startupMenuItem;
        private const int HOTKEY_ID = 1;

        public MouseButtonSwitcher(IStartupService startupService)
        {
            _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
            InitializeComponent();
            RegisterHotkey();
            UpdateStartupMenuState();
        }

        private void InitializeComponent()
        {
            // Hide the form
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new System.Drawing.Size(0, 0);

            // Create context menu
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Swap Mouse Buttons (Ctrl+Alt+M)", null, OnSwapClick);
            contextMenu.Items.Add("-");
            startupMenuItem = new ToolStripMenuItem("Run at Startup", null, OnStartupToggleClick);
            contextMenu.Items.Add(startupMenuItem);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, OnExitClick);

            // Create system tray icon
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = "Mouse Button Switcher - Ctrl+Alt+M to swap";
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SwapMouseButtons();
            }
        }

        private void RegisterHotkey()
        {
            bool registered = Program.RegisterHotKey(
                this.Handle,
                HOTKEY_ID,
                Program.MOD_CONTROL | Program.MOD_ALT,
                Program.VK_M
            );

            if (!registered)
            {
                MessageBox.Show(
                    "Failed to register hotkey Ctrl+Alt+M. It may already be in use.",
                    "Hotkey Registration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Program.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                SwapMouseButtons();
            }
            base.WndProc(ref m);
        }

        private void OnSwapClick(object? sender, EventArgs e)
        {
            SwapMouseButtons();
        }

        private void OnExitClick(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnStartupToggleClick(object? sender, EventArgs e)
        {
            bool isEnabled = _startupService.IsStartupEnabled();
            try
            {
                _startupService.SetStartupEnabled(!isEnabled);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UpdateStartupMenuState();

            string message = !isEnabled
                ? "Application will now start with Windows."
                : "Application will no longer start with Windows.";

            if (notifyIcon != null)
            {
                notifyIcon.BalloonTipTitle = "Startup Settings";
                notifyIcon.BalloonTipText = message;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(2000);
            }
        }

        private void UpdateStartupMenuState()
        {
            if (startupMenuItem != null)
            {
                bool isEnabled = _startupService.IsStartupEnabled();
                startupMenuItem.Checked = isEnabled;
                startupMenuItem.Text = isEnabled ? "Run at Startup ✓" : "Run at Startup";
            }
        }

        private void SwapMouseButtons()
        {
            int currentMetric = Program.GetSystemMetrics(Program.SM_SWAPBUTTON);
            bool isSwapped = MouseSwapState.IsSwapped(currentMetric);

            // Swap the buttons
            Program.SwapMouseButton(!isSwapped);

            int newMetric = Program.GetSystemMetrics(Program.SM_SWAPBUTTON);
            string primaryButton = MouseSwapState.GetPrimaryButtonName(newMetric);

            if (notifyIcon != null)
            {
                notifyIcon.BalloonTipTitle = "Mouse Button Swapped";
                notifyIcon.BalloonTipText = $"Primary mouse button is now: {primaryButton}";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(2000);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Unregister hotkey
            Program.UnregisterHotKey(this.Handle, HOTKEY_ID);

            // Clean up system tray icon
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }

            if (contextMenu != null)
            {
                contextMenu.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}
