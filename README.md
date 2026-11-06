# UnlockWelcome

A cinematic welcome animation for Windows, displayed when you unlock your PC.

UnlockWelcome runs quietly in the system tray and listens for the Windows session-unlock notification. When your session is unlocked, it presents a short, elegant welcome animation over the desktop.

## Features

- Detects Windows session unlock events using the Windows Terminal Services API.
- Cinematic dark visual style with animated particles and subtle glow.
- Customizable greeting, subtitle, and animation duration.
- Optional launch at Windows sign-in.
- System tray controls and animation preview.
- No administrator privileges required for normal per-user use.
- Settings are stored locally in the user's AppData folder.

## Requirements

- Windows 10 or Windows 11
- .NET 8 Desktop Runtime (x64)

## Build

    dotnet restore
    dotnet build -c Release
    dotnet publish -c Release -r win-x64 --self-contained false

The published application is located under bin/Release/net8.0-windows/win-x64/publish.

## Run

Launch UnlockWelcome.exe. The app will appear in the system tray. Right-click the tray icon to preview the animation, open settings, or exit.

## Autostart

Enable "登入 Windows 時自動啟動" in Settings. This adds a per-user entry under HKCU\Software\Microsoft\Windows\CurrentVersion\Run.

## Privacy

UnlockWelcome does not collect analytics, transmit personal data, or require an internet connection at runtime.

## License

MIT. See LICENSE.
