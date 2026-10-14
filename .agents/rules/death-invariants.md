# 💀 Death Workspace Rules & Architectural Invariants

> **Project**: Death / Memento Mori (High-Velocity Disciplinary Lockdown Engine)  
> **Framework**: .NET 9.0 WPF Native Windows Subsystem  
> **Author**: Karan Singh Verma

---

## 🏛️ Core Principles & Invariants

### 1. ⚡ Zero Background Daemon Invariant (0 MB Idle Footprint)
- **Decoupled Execution**: The engine must never run as a persistent background daemon.
- **Kernel Scheduling**: Timing intervals are managed exclusively via Windows Task Scheduler (`\Death`).
- **Complete Process Exit**: Upon completion or operator acknowledgment, the process must release all audio handles, unhook keyboard intercepts, and terminate cleanly (0 MB RAM footprint).
- **High Priority Dispatch**: Run with `ProcessPriorityClass.High` at startup to eliminate focus lag or animation jitter under heavy dev compilation loads.

### 2. 🎵 Targeted Audio Takeover & Auto-Restoration Invariant
- **Targeted Scope**: Pause ONLY **Browsers** (Chrome, Edge, Firefox, Brave, Opera, etc.), **Windows Media Player** (Classic & Win11 Store App), and **VLC**.
- **Zero Dev Interference**: NEVER execute blanket WASAPI session muting. Audio streams from developer processes (`ffmpeg`, Python TTS / Vocalis, terminal bells, and Windows OS alerts) must play unimpeded.
- **State-Preserving Restoration**: Record sessions actively playing prior to lockdown; cleanly resume them via `TryPlayAsync()` and `APPCOMMAND_MEDIA_PLAY` upon exit. Pre-existing paused media remains untouched.
- **Low-Latency Synthesis**: Mechanical ticking audio is synthesized in-memory via raw PCM streams without disk I/O bottlenecks.
- **Cinematic Fade-In**: The ambient whistle eases in over 800ms (linear 40ms volume ramp from 0.0 to 0.40) to prevent jarring acoustic startle snaps.

### 3. 💾 Zero SSD Burn Invariant (Ambient Audio Caching)
- **Static Cache Discipline**: `%TEMP%\sakshi_death_ambient.mp3` is checked against embedded resource byte length on launch.
- **Zero Redundant Writes**: If the file exists and the byte length matches, disk extraction is skipped entirely. Recurring runs generate **0 MB of disk writes**, protecting SSD health over long-term task execution.

### 4. 🛡️ Low-Level Input Interception Discipline & Global Crash Traps
- **Lockdown Security**: Win32 low-level keyboard hook (`WH_KEYBOARD_LL`) strictly consumes:
  - `VK_LWIN`, `VK_RWIN`, `VK_APPS`
  - `Alt+Tab`, `Alt+Esc`, `Alt+F4`, `Ctrl+Esc`
  - `Alt+Space` (blocks Win32 window system context menu)
- **Global Crash Traps (Unhook Guarantee)**: Low-level keyboard hook unhooking and media restoration are hooked directly to `AppDomain.CurrentDomain.UnhandledException`, `DispatcherUnhandledException`, and `AppDomain.CurrentDomain.ProcessExit` to guarantee dead-keyboard immunity even in unhandled crash scenarios.

### 5. 🎨 AMOLED Pitch Black Visual Language (#000000)
- **OLED Immersion**: Strict `#000000` pitch black full-screen backdrop with blood-red digital urgency typography (`#FF2020`).
- **Hardware Acceleration**: Use GPU-rendered DropShadow animations without CPU thread blocking.
- **Taskbar Bleed Defense**: Clamps the window explicitly to physical display coordinates (`(0, 0, SM_CXSCREEN, SM_CYSCREEN)`) via Win32 `SetWindowPos(HWND_TOPMOST)`, preventing the Windows 11 shell taskbar from bleeding through during notification flashes or tray badge events.

### 6. 🛣️ Zero Absolute Machine Path Invariant
- **Dynamic Resolution**: Resolve directories dynamically via `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` or relative project paths. Never hardcode local user directories.
