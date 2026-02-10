using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShiftMouseButton;

internal sealed class HotkeyDialog : Form
{
    private readonly TextBox _keyTextBox;
    private readonly CheckBox _ctrlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly Label _previewLabel;
    private uint _virtualKey;

    public Hotkey SelectedHotkey { get; private set; }

    public HotkeyDialog(Hotkey current)
    {
        Text = "Hotkey";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 190);

        var help = new Label
        {
            Text = "Choose modifiers and press a key.\r\nExample: Ctrl + Alt + M",
            AutoSize = true,
            Location = new Point(12, 12),
        };

        _ctrlCheckBox = new CheckBox { Text = "Ctrl", AutoSize = true, Location = new Point(12, 55) };
        _altCheckBox = new CheckBox { Text = "Alt", AutoSize = true, Location = new Point(80, 55) };
        _shiftCheckBox = new CheckBox { Text = "Shift", AutoSize = true, Location = new Point(140, 55) };
        _winCheckBox = new CheckBox { Text = "Win", AutoSize = true, Location = new Point(210, 55) };

        var keyLabel = new Label { Text = "Key:", AutoSize = true, Location = new Point(12, 88) };
        _keyTextBox = new TextBox
        {
            Location = new Point(50, 85),
            Size = new Size(120, 23),
            ReadOnly = true,
            TabStop = true,
        };
        _keyTextBox.KeyDown += KeyTextBox_KeyDown;

        _previewLabel = new Label
        {
            AutoSize = true,
            Location = new Point(12, 120),
        };

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(190, 150), Size = new Size(75, 28) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(273, 150), Size = new Size(75, 28) };

        AcceptButton = ok;
        CancelButton = cancel;

        Controls.Add(help);
        Controls.Add(_ctrlCheckBox);
        Controls.Add(_altCheckBox);
        Controls.Add(_shiftCheckBox);
        Controls.Add(_winCheckBox);
        Controls.Add(keyLabel);
        Controls.Add(_keyTextBox);
        Controls.Add(_previewLabel);
        Controls.Add(ok);
        Controls.Add(cancel);

        _ctrlCheckBox.CheckedChanged += (_, _) => UpdatePreview();
        _altCheckBox.CheckedChanged += (_, _) => UpdatePreview();
        _shiftCheckBox.CheckedChanged += (_, _) => UpdatePreview();
        _winCheckBox.CheckedChanged += (_, _) => UpdatePreview();

        // Initialize from current hotkey
        _ctrlCheckBox.Checked = current.Modifiers.HasFlag(HotkeyModifiers.Control);
        _altCheckBox.Checked = current.Modifiers.HasFlag(HotkeyModifiers.Alt);
        _shiftCheckBox.Checked = current.Modifiers.HasFlag(HotkeyModifiers.Shift);
        _winCheckBox.Checked = current.Modifiers.HasFlag(HotkeyModifiers.Win);
        _virtualKey = current.VirtualKey;
        _keyTextBox.Text = VirtualKeyHelpers.Format(_virtualKey);

        UpdatePreview();

        FormClosing += (_, e) =>
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }

            var hotkey = BuildHotkey();
            if (!hotkey.IsValid)
            {
                MessageBox.Show("Please choose a non-modifier key.", "Invalid Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            SelectedHotkey = hotkey;
        };
    }

    private void KeyTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        // Don't accept modifiers as the key.
        uint vk = (uint)e.KeyCode;
        if (VirtualKeyHelpers.IsModifierVirtualKey(vk))
        {
            e.SuppressKeyPress = true;
            return;
        }

        _virtualKey = vk;
        _keyTextBox.Text = VirtualKeyHelpers.Format(_virtualKey);
        UpdatePreview();
        e.SuppressKeyPress = true;
    }

    private Hotkey BuildHotkey()
    {
        HotkeyModifiers mods = HotkeyModifiers.None;
        if (_ctrlCheckBox.Checked) mods |= HotkeyModifiers.Control;
        if (_altCheckBox.Checked) mods |= HotkeyModifiers.Alt;
        if (_shiftCheckBox.Checked) mods |= HotkeyModifiers.Shift;
        if (_winCheckBox.Checked) mods |= HotkeyModifiers.Win;
        return new Hotkey(mods, _virtualKey);
    }

    private void UpdatePreview()
    {
        var hk = BuildHotkey();
        _previewLabel.Text = $"Preview: {hk}";
    }
}

