# UnlockWelcome Studio

**Create your own Windows unlock animation.**

UnlockWelcome Studio is a customizable desktop application for Windows 10 and Windows 11. It runs in the system tray, listens for Windows session-unlock notifications, and displays your chosen welcome experience after you unlock your PC.

## Features

### Theme studio
- Six built-in themes: Cinematic, Aurora, Minimal, Neon, Midnight, and Fluent.
- Click a theme card to use it as a starting point.
- Customize the title, subtitle, colors, typography, and alignment.
- Save your own configuration as the active theme.

### Visual customization
- Choose a local image background (PNG, JPG, JPEG, BMP).
- Choose a local video background (MP4, WMV, AVI, MPG; playback depends on Windows codecs).
- Customize background, title, and accent colors using HEX values.
- Choose title font, title size, text alignment, and date/time visibility.

### Animation controls
- Choose Fade, Zoom, or Slide entrance animations.
- Configure display duration or keep the welcome screen open until manually dismissed.
- Enable or disable floating particles, adjust their count and speed, and toggle text glow.
- Display on the primary monitor or all connected monitors.

### Theme portability
- Import and export themes as JSON (.uwtheme.json).
- Share theme settings with other users.
- Image and video assets are referenced by local file path and must be shared separately.

### Windows integration
- Detects session unlock using the Windows Terminal Services API.
- Runs in the system tray and can launch at Windows sign-in.
- Launching the EXE opens the Studio; launching it again brings the existing Studio window to the front.
- Local settings are stored in the user's AppData folder.
- Normal per-user operation does not require administrator privileges.

## Requirements

- Windows 10 or Windows 11 (x64)
- .NET 10 SDK to build from source
- The self-contained release package does not require a separate .NET Desktop Runtime.

## Build

    dotnet restore
    dotnet build UnlockWelcome.csproj -c Release

Publish a self-contained x64 executable:

    dotnet publish UnlockWelcome.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish

## Run

Launch UnlockWelcome.exe to open the Studio. Close the Studio window to leave UnlockWelcome running in the system tray. Use the tray menu to open the Studio, preview the current animation, or exit.

Enable "登入 Windows 後自動在背景啟動" in Program Settings to start listening after you sign in.

## Privacy

UnlockWelcome does not include analytics, telemetry, or runtime network requests. Selected image and video backgrounds are read from local files. Settings are stored locally under the current Windows user profile.

## Known limitations

- Video playback depends on media codecs available on the Windows installation.
- Exported JSON themes include settings and local media paths, not the media files themselves.
- UnlockWelcome displays an animation after Windows has unlocked the desktop; it does not replace or modify the secure Windows lock screen.

## 繁體中文

### 專案介紹

UnlockWelcome Studio 是一款 Windows 解鎖動畫自訂工具。程式會在系統匣常駐並監聽 Windows 工作階段解鎖事件；當使用者解鎖電腦後，就會播放自己設定的歡迎動畫。

### 主題與畫面自訂
- 內建 Cinematic、Aurora、Minimal、Neon、Midnight、Fluent 六種主題。
- 可自訂主標題、副標題、文字顏色、背景色、強調色及文字對齊。
- 可選擇標題字型與大小。
- 可使用自己的 PNG、JPG、JPEG、BMP 背景圖片。
- 可使用 MP4、WMV、AVI、MPG 影片背景，實際支援格式取決於 Windows 媒體編碼器。

### 動畫控制
- 淡入 Fade、縮放 Zoom、滑入 Slide 三種進場動畫。
- 自訂顯示秒數，或設定為手動關閉。
- 自訂粒子效果、數量、速度與文字柔光。
- 支援主要螢幕或所有螢幕。

### 主題匯入與分享
- 可將設定匯出成 JSON 主題檔（.uwtheme.json）。
- 可匯入其他使用者分享的主題。
- 背景圖片與影片會以本機檔案路徑記錄，分享主題時需另外提供素材檔案。

### Windows 整合
- 使用 Windows 工作階段通知偵測解鎖事件。
- 常駐系統匣，可設定登入 Windows 後自動在背景啟動。
- 直接開啟 EXE 會進入動畫工作室；若程式已在背景執行，再次開啟會叫出現有視窗。
- 設定儲存在使用者 AppData 資料夾。
- 一般個人使用不需要系統管理員權限。

### 編譯方式

需要 Windows 10 / 11 x64 與 .NET 10 SDK。

    dotnet restore
    dotnet build UnlockWelcome.csproj -c Release

建立免安裝 .NET Runtime 的獨立 EXE：

    dotnet publish UnlockWelcome.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish

### 隱私與限制

程式不包含遙測、分析或執行時網路請求。背景圖片與影片只會從使用者選擇的本機檔案載入。

注意：UnlockWelcome 是在 Windows 解鎖並進入桌面後顯示動畫，不會修改或取代 Windows 安全鎖定畫面。影片播放取決於 Windows 已安裝的媒體編碼器。

## License

MIT. See LICENSE.
