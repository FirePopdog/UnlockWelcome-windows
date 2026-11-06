# UnlockWelcome

**A cinematic welcome animation for Windows when you unlock your PC.**

UnlockWelcome runs quietly in the system tray and listens for the Windows session-unlock notification. When your session is unlocked, it presents a short, elegant welcome animation over the desktop.

[English](#english) · [繁體中文](#繁體中文)

## English

### Features

- Detects Windows session unlock events through the Windows Terminal Services API.
- Cinematic dark visual style with animated particles and a subtle glow.
- Customizable greeting, subtitle, and animation duration.
- Optional launch at Windows sign-in.
- System tray controls and animation preview.
- Local settings only; no analytics or network calls.
- Normal per-user operation does not require administrator privileges.

### Requirements

- Windows 10 or Windows 11 (x64)
- .NET 8 Desktop Runtime for framework-dependent builds
- The GitHub Actions release package is self-contained and does not require a separately installed .NET runtime.

### Build from source

    dotnet restore
    dotnet build -c Release

To publish a self-contained x64 build:

    dotnet publish UnlockWelcome.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

### Run

Launch UnlockWelcome.exe. The app appears in the system tray. Right-click the tray icon to preview the animation, open settings, or exit.

### Autostart

Enable "登入 Windows 時自動啟動" in Settings. This adds a per-user entry under HKCU\Software\Microsoft\Windows\CurrentVersion\Run.

### Privacy

UnlockWelcome does not collect analytics, transmit personal data, or require an internet connection at runtime. Settings are stored in the current user's AppData folder.

## 繁體中文

### 專案介紹

UnlockWelcome 是一款 Windows 桌面解鎖歡迎動畫工具。程式會在背景常駐於系統匣，監聽 Windows 工作階段解鎖通知；當使用者解鎖電腦時，便會顯示電影感歡迎動畫。

### 功能

- 自動偵測 Windows 解鎖事件。
- 深色電影感畫面、粒子動畫與柔和光暈。
- 自訂主標題、副標題及動畫顯示時間。
- 可選擇登入 Windows 時自動啟動。
- 系統匣操作、動畫預覽與設定介面。
- 不蒐集分析資料、不連線傳送個人資料。
- 一般個人使用不需要系統管理員權限。

### 編譯方式

需要 Windows 10 / 11 x64 與 .NET 8 SDK。

    dotnet restore
    dotnet build -c Release

建立免安裝 .NET Runtime 的獨立版本：

    dotnet publish UnlockWelcome.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

### 使用方式

執行 UnlockWelcome.exe 後，程式會常駐於系統匣。右鍵點擊圖示即可預覽動畫、開啟設定或結束程式。

在設定中啟用「登入 Windows 時自動啟動」，即可讓程式在登入後自動執行。

### 隱私

UnlockWelcome 不包含遙測、分析或執行時網路連線。設定檔僅儲存在目前 Windows 使用者的 AppData 資料夾。

## License

MIT. See [LICENSE](LICENSE).
