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

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var startupService = new StartupService(() => Application.ExecutablePath);

            ISettingsService settingsService = new JsonSettingsService();
            Hotkey hotkey = settingsService.LoadHotkey();

            if (HotkeyCli.TryGetHotkeyOverride(args, out var cliHotkey, out var cliError))
            {
                hotkey = cliHotkey;
            }
            else if (!string.IsNullOrWhiteSpace(cliError))
            {
                MessageBox.Show(
                    cliError,
                    "Invalid --hotkey Argument",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            Application.Run(new MouseButtonSwitcher(startupService, settingsService, hotkey));
        }
    }

    public class MouseButtonSwitcher : Form
    {
        private readonly IStartupService _startupService;
        private readonly ISettingsService _settingsService;
        private Hotkey _hotkey;
        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenu;
        private ToolStripMenuItem? startupMenuItem;
        private ToolStripMenuItem? swapMenuItem;
        private const int HOTKEY_ID = 1;

        public MouseButtonSwitcher(IStartupService startupService, ISettingsService settingsService, Hotkey hotkey)
        {
            _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _hotkey = hotkey.IsValid ? hotkey : Hotkey.Default;
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
            swapMenuItem = new ToolStripMenuItem(GetSwapMenuText(), null, OnSwapClick);
            contextMenu.Items.Add(swapMenuItem);
            contextMenu.Items.Add("Hotkey...", null, OnHotkeyClick);
            contextMenu.Items.Add("-");
            startupMenuItem = new ToolStripMenuItem("Run at Startup", null, OnStartupToggleClick);
            contextMenu.Items.Add(startupMenuItem);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, OnExitClick);

            // Create system tray icon
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = GetTrayTooltipText();
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
            if (!TryRegisterHotkey(_hotkey))
            {
                MessageBox.Show(
                    $"Failed to register hotkey {_hotkey}. It may already be in use.",
                    "Hotkey Registration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private bool TryRegisterHotkey(Hotkey hotkey) =>
            Program.RegisterHotKey(this.Handle, HOTKEY_ID, (uint)hotkey.Modifiers, hotkey.VirtualKey);

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

        private void OnHotkeyClick(object? sender, EventArgs e)
        {
            using var dialog = new HotkeyDialog(_hotkey);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ApplyHotkey(dialog.SelectedHotkey, persist: true);
        }

        private void ApplyHotkey(Hotkey newHotkey, bool persist)
        {
            if (!newHotkey.IsValid)
            {
                MessageBox.Show("Invalid hotkey.", "Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hotkey oldHotkey = _hotkey;

            // Unregister previous hotkey (best-effort), then try register the new one.
            Program.UnregisterHotKey(this.Handle, HOTKEY_ID);
            if (!TryRegisterHotkey(newHotkey))
            {
                // Roll back: try to re-register the old hotkey.
                _ = TryRegisterHotkey(oldHotkey);
                MessageBox.Show(
                    $"Failed to register hotkey {newHotkey}. It may already be in use.",
                    "Hotkey Registration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _hotkey = newHotkey;
            UpdateHotkeyUi();

            if (persist)
            {
                try
                {
                    _settingsService.SaveHotkey(newHotkey);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed to save settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (notifyIcon != null)
            {
                notifyIcon.BalloonTipTitle = "Hotkey Updated";
                notifyIcon.BalloonTipText = $"New hotkey: {_hotkey}";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(2000);
            }
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

        private string GetSwapMenuText() => $"Swap Mouse Buttons ({_hotkey})";

        private string GetTrayTooltipText()
        {
            // NotifyIcon.Text has a length limit; keep this short.
            string text = $"ShiftMouseButton - {_hotkey} to swap";
            return text.Length <= 63 ? text : text[..63];
        }

        private void UpdateHotkeyUi()
        {
            if (swapMenuItem != null)
            {
                swapMenuItem.Text = GetSwapMenuText();
            }

            if (notifyIcon != null)
            {
                notifyIcon.Text = GetTrayTooltipText();
            }
        }
    }
}
