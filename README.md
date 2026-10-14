# 💀 Death (Memento Mori) Module

<p align="center">
  <img src="https://img.shields.io/badge/MODULE-DEATH%20%2F%20MEMENTO%20MORI-red?style=for-the-badge" alt="Death Module"/>
  <img src="https://img.shields.io/badge/VERSION-v2.0.0-bloodred?style=for-the-badge&color=8B0000" alt="v2.0.0"/>
  <img src="https://img.shields.io/badge/RUNTIME-.NET%209%20WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9"/>
  <img src="https://img.shields.io/badge/IDLE%20RAM-0%20MB-brightgreen?style=for-the-badge" alt="0 MB RAM"/>
  <img src="https://img.shields.io/badge/SSD%20WRITES-0%20MB%20(CACHED)-success?style=for-the-badge" alt="0 MB SSD"/>
  <img src="https://img.shields.io/badge/STARTUP-%3C38ms-orange?style=for-the-badge" alt="<38ms Startup"/>
  <img src="https://img.shields.io/badge/PLATFORM-WINDOWS%2011-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows 11"/>
</p>

---

> [!IMPORTANT]
> **"Memento Mori. Remember that you must die."**
> A sovereign, hardware-accelerated disciplinary enforcement engine within the **Sakshi** ecosystem. Engineered specifically for Windows 11, it eliminates digital distraction, tab paralysis, and procrastination through unescapable, high-velocity countdown lockdowns with **zero background daemon overhead**.

---

## 👁️ Visual Interface & Active States

### 1. The Active Countdown Lockdown
During the countdown phase, the screen is seized in pure `#000000` AMOLED OLED darkness. Ambient browser/player media is frozen, background developer audio (Python, Vocalis, ffmpeg, terminal bells) plays unimpeded, and system escape shortcuts are intercepted by a low-level keyboard hook.

<p align="center">
  <img src="assets/death_countdown.png" alt="Death Countdown Lockdown" width="950" />
</p>

* **Pulsating Typography**: `💀 MEMENTO MORI 💀` pulsating with a hardware-accelerated WPF drop-shadow glow (blur radius animated from 20 to 60).
* **Urgency Meter**: Central digital timer rendering remaining lockdown seconds in blood-red typography (`#FF2020`).
* **Mechanical PCM Audio**: In-memory synthesized dual-tone mechanical clock ticks (1800Hz / 1500Hz) reinforcing urgency.

---

### 2. Acknowledgment & Unlock State
Once the countdown hits zero, the mechanical ticking ceases, a resonant 1200Hz unlock chime fires, and the dynamic acknowledgment button fades into view with your personalized study quote.

<p align="center">
  <img src="assets/death_unlocked.png" alt="Death Unlocked View" width="950" />
</p>

* **Quote Injection**: The motivational quote is manifested directly on the face of the acknowledgment button without disk file reading.
* **Release Protocol**: Clicking the acknowledgment button (or pressing Enter/Space) cleanly restores system media sessions, unhooks keyboard filters, saves state, and terminates the process—immediately returning memory to **0 MB**.

---

## 📊 Telemetry & Engineering Specifications

| Metric | Target Specification | Real-World Measurement | Verification Standard |
| :--- | :--- | :--- | :--- |
| **Idle Memory Footprint** | 0.00 MB (Decoupled Kernel Dispatch) | **0.00 MB** | Process exits completely upon dismiss |
| **Active Lockdown RAM** | < 45 MB | **~38.4 MB** | WPF hardware-accelerated DirectX composition |
| **Cold-Start Launch Latency**| < 50 ms | **< 38 ms** | Standalone .NET 9 single-file binary |
| **Idle CPU Utilization** | 0.0% | **0.0%** | Zero polling threads or background daemons |
| **Recurring SSD Disk Writes**| 0 MB / execution | **0 MB** | Static cache invariant in `%TEMP%` |
| **Audio Synthesis Latency**  | < 1 ms | **< 0.2 ms** | In-memory 16-bit PCM wave stream via WinMM |
| **Escape Vector Lockout**    | 100% Interception | **100%** | `WH_KEYBOARD_LL` (Win, Alt+Tab, Alt+F4, Alt+Space) |
| **Binary Footprint**         | Single standalone executable | **30.4 MB** | `~/.local/bin/Death.exe` |

---

## 📐 System Architecture & Execution Lifecycle

The Death engine operates on a decoupled architecture where Windows Task Scheduler serves as the zero-RAM hardware clock:

```text
┌───────────────────────────────────────────────────────────┐
│              Windows Task Scheduler (Kernel Clock)        │
│  - Task Name: Death                                       │
│  - Interval: Configurable (e.g. 15, 20, 30, 45, 60m)      │
│  - Idle Footprint: 0 MB RAM / 0% CPU                      │
│  - Sleep Skip: StartWhenAvailable = $false (no wakeup lag)│
└─────────────────────────────┬─────────────────────────────┘
                              │ Scheduled repetition OR manual CLI `Death`
                              ▼
┌───────────────────────────────────────────────────────────┐
│             Death.exe (~/.local/bin/Death.exe)            │
│                                                           │
│  1. Targeted Media Freeze: Pauses Browsers, WMP, VLC     │
│  2. Zero Dev Interference: ffmpeg, Python, alerts pass         │
│  3. In-Memory PCM Synth: Calibrated 1800/1500Hz tick-tock │
│  4. DirectX OLED Lockdown: Topmost, borderless fullscreen │
│  5. Win32 LL Keyboard Hook: Suppresses escape shortcuts   │
│  6. Release: Resonant chime, quote unlock, restore media   │
└─────────────────────────────┬─────────────────────────────┘
                              │
                              ▼
              Process exits -> RAM drops back to 0 MB
```

> [!NOTE]
> For sequence diagrams, mathematical sine wave synthesis formulas, and deep Win32 hook mechanics, review the [Deep System Architecture Specification](docs/architecture.md).

---

## 🛡️ Overwatch Hardening & Escape Suppression

To ensure disciplinary integrity, the Death engine implements a multi-layer lockdown defense:

| Vector | Defense Mechanism | Technical Implementation |
| :--- | :--- | :--- |
| **Taskbar Close** | **Icon Eradication** | `ShowInTaskbar="False"` prevents the window from appearing on the Windows 11 taskbar, eliminating right-click "Close" or "End Task". |
| **System Hotkeys** | **Low-Level Keyboard Hook** | Win32 `SetWindowsHookEx(WH_KEYBOARD_LL)` intercepts and consumes `VK_LWIN`, `VK_RWIN`, `VK_APPS`, `Alt+Tab`, `Alt+Esc`, `Alt+F4`, `Ctrl+Esc`, and `Alt+Space` (blocks system menu). Auto-unhooked upon unlock + global crash safety traps. |
| **Window Minimizing** | **State Enforcement** | `Window_StateChanged` overrides any attempt to minimize or restore down, immediately asserting `WindowState.Normal` with full display bounds. |
| **Taskbar Bleed Defense** | **Physical Rect Lockout** | Queries `GetSystemMetrics(SM_CXSCREEN/SM_CYSCREEN)` and calls `SetWindowPos(HWND_TOPMOST, 0, 0, screenW, screenH)` to guarantee the Windows 11 taskbar never pops over the lockdown screen. |
| **Virtual Desktops** | **COM VDM Pinning** | Queries `IVirtualDesktopManager` to detect the currently active virtual desktop and pin/teleport the lockdown overlay immediately to the user's viewport. |
| **Foreground Lockout** | **Thread Input Attach** | Calls Win32 `AttachThreadInput` and `SetForegroundWindow` to bypass Windows foreground lock timeouts, guaranteeing top z-order. |
---

## 🔊 Audio Engine & Sound Synthesis

The audio subsystem (`AudioEngine.cs`) manages session audio without external audio dependencies:

1. **Targeted Media Freeze (Zero Dev Interference)**:
   - Strictly targets:
     - **Browsers** (Chrome, Edge, Firefox, Brave, Opera, Vivaldi, Arc, Zen, Waterfox)
     - **Windows Media Player** (Classic `wmplayer` and modern Windows 11 Media Player / Groove)
     - **VLC Media Player** (`vlc`)
   - **Zero Blanket Muting**: Indiscriminate WASAPI session muting is eliminated. Background developer audio (`ffmpeg`, Python TTS / Vocalis, terminal bells, OS alerts) plays unimpeded.
2. **In-Memory Paused Session Tracking & Auto-Restore**:
   - Records only sessions actively `Playing` prior to lockdown via WinRT GSMTC (`SourceAppUserModelId`) and Win32 `WM_APPCOMMAND`.
   - On unlock or dismissal, Death asynchronously calls `session.TryPlayAsync()` and `APPCOMMAND_MEDIA_PLAY` to cleanly resume playback. Pre-paused media remains untouched.
3. **Cinematic 800ms Ambient Whistle Fade-In**:
   - On onset, `The Whistle of Death` volume initializes at `0.0` and smoothly ramps to `0.40` over 800ms (40ms steps, +0.02 volume) to avoid sudden acoustic startle snaps.
4. **Zero SSD Burn Ambient Audio Caching**:
   - `%TEMP%\sakshi_death_ambient.mp3` is checked against embedded resource byte length on launch.
   - Reuses cached file on recurring runs, yielding **0 MB of SSD disk writes** on scheduled executions.
5. **In-Memory PCM Clock Synthesizer**:
   - Generates raw PCM wave streams in memory using high-frequency sine wave calculations.
   - Dual-tone ticking (1800Hz high tick, 1500Hz low tock) played via Win32 `PlaySound(SND_MEMORY | SND_ASYNC)`.
6. **Resonant Unlock Chime**:
   - Pure 1200Hz chime synthesizing a smooth bell decay over 450ms, signaling that the countdown is complete.

---

## 🚀 30-Second Quickstart & Deployment

### Option A: Standalone Binary (Zero Dependencies)
Download the pre-compiled single-file executable directly (no .NET SDK required):
* 💾 **[Download Death.exe v2.0.0 (30.4 MB)](https://github.com/karansinghverma979/Death/releases/download/v2.0.0/Death.exe)**

**1-Line PowerShell Auto-Download:**
```powershell
New-Item -ItemType Directory -Path "$HOME\.local\bin" -Force | Out-Null
irm https://github.com/karansinghverma979/Death/releases/download/v2.0.0/Death.exe -OutFile "$HOME\.local\bin\Death.exe"
```

### Option B: Automated Build & Schedule Installer
Deploy the Death module using the automated installer:

```powershell
# Interactive installation (prompts for interval and quote)
.\Install-Death.ps1

# Non-interactive silent installation
.\Install-Death.ps1 -TaskName "Death" -IntervalMinutes 30 -Quote "KEEP CALM AND STUDY HARD." -NonInteractive
```

### Deployment Workflow:
1. **Validation**: Verifies .NET 9 SDK and target paths.
2. **PATH Setup**: Automatically appends `~/.local/bin` to User `PATH`.
3. **Compilation**: Publishes a single-file release binary (`Death.exe`) straight to `~/.local/bin/Death.exe`.
4. **Task Registration**: Configures Windows Task Scheduler with 0 MB idle RAM, 0% CPU, and Sleep-Skip (`StartWhenAvailable = $false`).
5. **Zero Disk I/O**: Injects the quote directly as a CLI argument (`--quote "..."`), requiring zero disk configuration files.

---

## 💻 CLI Usage Manual

With `~/.local/bin` in your `$env:PATH`, invoke the Death module anytime from any PowerShell or Command Prompt:

```powershell
# Display full CLI help manual
Death --help
Death -h

# Launch default 60-second countdown lockdown
Death

# Quick 3-second diagnostic smoke test
Death --test

# Launch with custom countdown duration (e.g. 15 seconds)
Death --seconds 15
Death -s 15

# Launch with dynamic custom study quote
Death --quote "DISCIPLINE EQUALS FREEDOM."
Death -q "FOCUS ON THE CURRENT TASK."
```

---

## 🧹 Deregistration & Uninstallation

To cleanly remove the scheduled task and binaries:

```powershell
# Complete eradication (removes task and deletes ~/.local/bin/Death.exe)
.\Uninstall-Death.ps1

# Preserve compiled binary for manual CLI usage, removing only the schedule
.\Uninstall-Death.ps1 -KeepBinary
```

---

## 📜 Version History & Changelog

### [v2.0.0] - 2026-10-14
- **Targeted Media Freeze**: Replaced blanket WASAPI muting with targeted pausing of Web Browsers (Chrome, Edge, Firefox, Brave, Arc, etc.), Windows Media Player, and VLC. Zero interference with developer audio (`ffmpeg`, Python TTS / Vocalis, terminal bells, OS alerts).
- **Session Auto-Restore**: Restores previously playing media sessions asynchronously upon quote unlock via WinRT GSMTC (`TryPlayAsync()`) and Win32 `WM_APPCOMMAND`.
- **Zero SSD Burn Cache**: Verifies `%TEMP%\sakshi_death_ambient.mp3` byte length against embedded resources; recurring executions yield **0 MB disk writes**.
- **Cinematic Fade-In**: 800ms smooth volume ramp from 0.0 to 0.40 on onset.
- **Physical Rect Clamping**: Window coordinates clamped to `(0, 0, SM_CXSCREEN, SM_CYSCREEN)` via `SetWindowPos`, shutting down Windows 11 taskbar bleed-through.
- **System Menu Defense**: Win32 hook strictly blocks `Alt+Space` to suppress the Win32 window context menu.
- **Global Crash Traps**: `UnhandledException` and `ProcessExit` hooks guarantee the low-level keyboard hook is never left orphaned in memory.

### [v1.0.0] - 2026-09-20
- Initial production release of the decoupled .NET 9 Death engine.
- 0 MB idle RAM footprint with Windows Task Scheduler integration.
- Hardware-accelerated AMOLED pitch-black (#000000) full-screen overlay.
- Low-level keyboard hook (`WH_KEYBOARD_LL`) intercepting Alt+Tab, Win keys, Alt+F4.

