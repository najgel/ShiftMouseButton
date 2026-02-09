' Writes HKCU\...\Run\MouseButtonSwitcher = path (same key/value as the app's "Run at Startup").
Sub SetStartupRegistry()
  Dim check, path, sh, exePath, productCode
  check = Session.Property("WIXUI_EXITDIALOGOPTIONALCHECKBOX")
  If check <> "1" Then Exit Sub

  Set sh = CreateObject("WScript.Shell")
  path = Session.Property("INSTALLPATH")
  ' INSTALLPATH may be empty in UI sequence; fallback to Uninstall key (written by MSI after install).
  If path = "" Or IsNull(path) Then
    On Error Resume Next
    productCode = Session.Property("ProductCode")
    If productCode <> "" Then
      path = sh.RegRead("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" & productCode & "\InstallLocation")
      If IsNull(path) Then path = ""
    End If
    On Error Goto 0
  End If
  If path = "" Or IsNull(path) Then Exit Sub
  If Right(path, 1) <> "\" Then path = path & "\"
  exePath = path & "ShiftMouseButton.exe"
  On Error Resume Next
  sh.RegWrite "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\MouseButtonSwitcher", exePath, "REG_SZ"
  On Error Goto 0
End Sub

' Launches ShiftMouseButton.exe (used when "launch now" is checked on exit dialog).
Sub LaunchApplication()
  Dim path, sh, exePath, productCode
  Set sh = CreateObject("WScript.Shell")
  path = Session.Property("INSTALLPATH")
  If path = "" Or IsNull(path) Then
    On Error Resume Next
    productCode = Session.Property("ProductCode")
    If productCode <> "" Then
      path = sh.RegRead("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" & productCode & "\InstallLocation")
      If IsNull(path) Then path = ""
    End If
    On Error Goto 0
  End If
  If path = "" Or IsNull(path) Then Exit Sub
  If Right(path, 1) <> "\" Then path = path & "\"
  exePath = path & "ShiftMouseButton.exe"
  On Error Resume Next
  sh.Run """" & exePath & """", 1, False
  On Error Goto 0
End Sub

' Removes the Run key on uninstall (same key the app and SetStartupRegistry use).
Sub RemoveStartupRegistry()
  On Error Resume Next
  Dim sh
  Set sh = CreateObject("WScript.Shell")
  sh.RegDelete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\MouseButtonSwitcher"
  On Error Goto 0
End Sub
