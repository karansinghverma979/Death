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

### 2. 🔇 WASAPI & Audio Engine Integrity
- **Low-Latency Synthesis**: Mechanical ticking audio is synthesized in-memory via raw PCM / WASAPI streams without disk I/O bottlenecks.
- **Graceful Restoration**: System master audio must be cleanly restored to its original state prior to process termination.

### 3. 🛡️ Low-Level Input Interception Discipline
- **Lockdown Security**: Escape keys and system interruptions are governed by a low-level Win32 keyboard hook (`WH_KEYBOARD_LL`).
- **Unhook Guarantee**: The hook MUST be unhooked (`UnhookWindowsHookEx`) in all exit paths (normal completion, exception handlers, cancellation) to prevent dead keyboard locks.

### 4. 🎨 AMOLED Pitch Black Visual Language (#000000)
- **OLED Immersion**: Strict `#000000` pitch black full-screen backdrop with blood-red digital urgency typography (`#FF2020`).
- **Hardware Acceleration**: Use GPU-rendered DropShadow animations without CPU thread blocking.

### 5. 🛣️ Zero Absolute Machine Path Invariant
- **Dynamic Resolution**: Resolve directories dynamically via `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` or relative project paths.
