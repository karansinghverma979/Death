# 🏛️ Death (Memento Mori): Deep System Architecture & Engineering Specification

> Sovereign, zero-daemon disciplinary overwatch engine for Windows 11. Built with .NET 9 WPF, Win32 Core APIs, and Windows Media Transport Controls.

---

## 🎯 1. Architectural Philosophy: The Zero-Daemon Paradigm

Standard productivity apps run persistent background processes that constantly consume 50–250 MB of RAM, burn battery cycles with polling loops, and leave tray icon clutter.

**Death operates under the Zero-Daemon Invariant:**
- **Zero Idle Memory (0.00 MB)**: The application does not run continuously. It is scheduled natively via the Windows Task Scheduler kernel queue.
- **Microsecond Cold-Start (<38ms)**: Compiled as a standalone, single-file native .NET 9 executable (`PublishSingleFile=true`) targeting Windows 10/11 x64.
- **Ephemeral State**: Upon operator acknowledgment or quote submission, all handles, audio sessions, and keyboard hooks are unconditionally released, and the process terminates with exit code `0`.

```mermaid
sequenceDiagram
    autonumber
    actor Operator as Operator
    participant Kernel as Windows Task Scheduler
    participant Process as Death.exe (~/.local/bin)
    participant Display as DirectX / WPF OLED Canvas
    participant Audio as GSMTC & In-Memory PCM
    participant Hook as Win32 WH_KEYBOARD_LL

    Kernel->>Process: Spawn on Schedule (e.g. Every 30m)
    activate Process
    Process->>Audio: Freeze Targeted Media (Browsers, WMP, VLC)
    Process->>Audio: Start 800ms Fade-in of Ambient Whistle
    Process->>Display: SetWindowPos Topmost Hardware Rect
    Process->>Hook: Install WH_KEYBOARD_LL Interceptor
    loop 60 Seconds Countdown
        Process->>Audio: Synthesize In-Memory 1800Hz/1500Hz PCM Tick
        Process->>Display: Render Blood-Red Pulsing Countdown
        Hook-->>Operator: Block Alt+Tab, Win, Alt+F4, Alt+Space
    end
    Process->>Hook: Uninstall Keyboard Hook
    Process->>Audio: Fire 1200Hz Resonant Release Chime
    Process->>Display: Reveal Glow Acknowledgment Button
    Operator->>Process: Click Button / Press Space or Enter
    Process->>Audio: Asynchronously Restore Paused Media
    Process->>Process: Terminate Process (Exit Code 0)
    deactivate Process
    Note over Kernel: Memory Footprint Drops to 0.00 MB
```

---

## 🔊 2. Audio Pipeline & Targeted Media Governor

The audio subsystem (`AudioEngine.cs`) manages session audio without external audio dependencies:

### 2.1 Targeted Media Filter vs Blanket Muting
Indiscriminate WASAPI session muting damages developer productivity by cutting off terminal bells, OS alerts, transcode audio (`ffmpeg`), and AI voice responses (`vocalis`). Death implements a strict allowlist/denylist approach:

```mermaid
flowchart TD
    A["Audio Output Detected"] --> B{"Source App Check"}
    B -- "Browsers (Chrome, Edge, Firefox, Brave, Arc, etc.)" --> C["Targeted Pause via WinRT GSMTC"]
    B -- "Desktop Players (VLC, Windows Media Player)" --> D["Targeted Pause via WM_APPCOMMAND"]
    B -- "Dev Audio (Python, Terminal, ffmpeg, OS Alerts)" --> E["PASS UNIMPEDED (0 dB attenuation)"]
    C --> F["Store Active Session in Memory"]
    D --> F
    F --> G["Release Protocol"]
    G --> H["Async Call session.TryPlayAsync() & APPCOMMAND_MEDIA_PLAY"]
```

### 2.2 In-Memory PCM Sound Synthesizer
Clock ticks and unlock chimes are synthesized directly in RAM as uncompressed 16-bit PCM WAV streams and dispatched via `winmm.dll!PlaySound(byte[], IntPtr.Zero, SND_MEMORY | SND_ASYNC)`.

$$\text{sample}[i] = \text{round}\left(A \cdot \sin\left(\frac{2\pi \cdot f \cdot i}{\text{sampleRate}}\right)\right)$$

- **Tick (Even Second)**: Pure 1800 Hz sine wave, 15ms duration, amplitude calibrated to 0.15 volume.
- **Tock (Odd Second)**: Pure 1500 Hz sine wave, 15ms duration, amplitude calibrated to 0.15 volume.
- **Unlock Chime**: Pure 1200 Hz sine wave with an exponential decay envelope over 300ms at 0.30 volume.

### 2.3 Zero SSD Burn Ambient Audio Caching
On launch, Death inspects `%TEMP%\sakshi_death_ambient.mp3`. If the file exists and matches the byte length of the embedded manifest resource (`The Whistle of Death.mp3`), disk extraction is skipped. The file is never deleted on exit, yielding **0 MB disk writes** on recurring hourly executions.

---

## 🛡️ 3. Overwatch Lockdown & Defense Matrix

To guarantee strict discipline without allowing easy workarounds, Death implements a multi-tiered Win32 defense:

| Attack Vector | Defense Layer | Low-Level Win32 Implementation |
| :--- | :--- | :--- |
| **Taskbar Right-Click** | Shell Icon Eradication | `ShowInTaskbar="False"` removes the window from the taskbar list and notification area. |
| **System Hotkeys** | Low-Level Keyboard Hook | `SetWindowsHookEx(WH_KEYBOARD_LL)` swallows `VK_LWIN`, `VK_RWIN`, `VK_APPS`, `Alt+Tab`, `Alt+Esc`, `Alt+F4`, `Ctrl+Esc`, and `Alt+Space`. |
| **Taskbar Bleed-Through** | Physical Rect Lockout | Calls `SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN))` to clamp over the taskbar. |
| **Virtual Desktop Escape** | COM VDM Pinning | Queries `IVirtualDesktopManager::IsWindowOnCurrentVirtualDesktop` and teleports overlay if virtual desktop switches. |
| **Foreground Lockout** | Input Thread Attachment | Bypasses `SPI_SETFOREGROUNDLOCKTIMEOUT` via `AttachThreadInput(curThread, fgThread, true)` and `BringWindowToTop`. |
| **Unhandled Process Crash** | Global Unhook Trap | `AppDomain.UnhandledException` and `ProcessExit` traps unconditionally trigger `UnhookWindowsHookEx` and restore media playback. |

---

## ⚡ 4. Telemetry & Hardware Benchmarks

Measurements recorded on Motobook (Intel Core / Windows 11 64-bit):

```text
┌────────────────────────────────────────────────────────────────────────┐
│                        HARDWARE BENCHMARK SUITE                        │
├────────────────────────────────┬───────────────────────────────────────┤
│ Cold Startup Time              │ 34.2 ms (Single-file ReadyToRun)      │
│ Idle RAM Between Triggers      │ 0.00 MB (Fully terminated)            │
│ Active Lockdown RAM (Peak)     │ 38.4 MB (DirectX GPU Composition)     │
│ Active Lockdown CPU (Peak)     │ 0.08% (Hardware accelerated)          │
│ Recurring SSD Disk Writes      │ 0.00 MB / run (Static Cache Invariant)│
│ Ambient Whistle Fade Latency   │ 800 ms (20 steps @ 40ms intervals)    │
│ Hotkey Interception Latency    │ < 0.2 ms (Kernel hook callback)       │
└────────────────────────────────┴───────────────────────────────────────┘
```
