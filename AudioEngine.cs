using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Sakshi.Death
{
    #region --- COM INTERFACES ---
    [ComImport]
    [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IVirtualDesktopManager
    {
        [PreserveSig] int IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow, [MarshalAs(UnmanagedType.Bool)] out bool onCurrentDesktop);
        [PreserveSig] int GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId);
        [PreserveSig] int MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
    }
    #endregion

    public static class AudioEngine
    {
        #region --- WIN32 IMPORTS ---
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PlaySound(byte[] pszSound, IntPtr hmod, uint fdwSound);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const byte VK_MEDIA_STOP = 0xB2;
        public const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_RESTORE = 9;

        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const int VK_SPACE = 0x20;

        public const uint WM_APPCOMMAND = 0x0319;
        public const int APPCOMMAND_MEDIA_PLAY = 46;
        public const int APPCOMMAND_MEDIA_PAUSE = 47;
        public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        #endregion

        private static readonly IVirtualDesktopManager? _desktopManager;
        private static readonly System.Collections.Generic.List<GlobalSystemMediaTransportControlsSession> _pausedGsmtcSessions = new();
        private static readonly System.Collections.Generic.List<IntPtr> _pausedWin32Windows = new();

        static AudioEngine()
        {
            try
            {
                Type? type = Type.GetTypeFromCLSID(new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A"));
                if (type != null)
                {
                    _desktopManager = (IVirtualDesktopManager?)Activator.CreateInstance(type);
                }
            }
            catch
            {
                _desktopManager = null;
            }
        }

        #region --- TARGETED MEDIA FILTERING & PAUSE / RESTORE ---
        /// <summary>
        /// Identifies whether a media source belongs strictly to Browsers, Windows Media Player, or VLC.
        /// Ignores ffmpeg, Python scripts, terminal sounds, system alerts, Discord, etc.
        /// </summary>
        public static bool IsTargetMediaApp(string? sourceAppId)
        {
            if (string.IsNullOrWhiteSpace(sourceAppId)) return false;
            string id = sourceAppId.ToLowerInvariant();

            // 1. Browsers
            if (id.Contains("chrome") ||
                id.Contains("msedge") ||
                id.Contains("firefox") ||
                id.Contains("brave") ||
                id.Contains("opera") ||
                id.Contains("vivaldi") ||
                id.Contains("arc") ||
                id.Contains("zen") ||
                id.Contains("waterfox"))
            {
                return true;
            }

            // 2. Windows Media Player (Classic wmplayer & Modern Windows 11 Media Player / Groove)
            if (id.Contains("wmplayer") ||
                id.Contains("zunemusic") ||
                id.Contains("media.player") ||
                id.Contains("mediaplayer"))
            {
                return true;
            }

            // 3. VLC Media Player
            if (id.Contains("vlc"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Temporarily pauses strictly targeted media sessions (Browsers, WMP, VLC).
        /// Records paused sessions in memory so they can be accurately restored on exit.
        /// </summary>
        public static async Task PauseActiveMediaAsync()
        {
            _pausedGsmtcSessions.Clear();
            _pausedWin32Windows.Clear();

            // 1. If operator is actively focused on VLC or classic WMP, send pause command
            try
            {
                IntPtr priorFg = GetForegroundWindow();
                if (priorFg != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(priorFg, out uint fgPid);
                    using var fgProc = Process.GetProcessById((int)fgPid);
                    string procName = fgProc.ProcessName.ToLowerInvariant();
                    if (procName.Contains("vlc") || procName.Contains("wmplayer"))
                    {
                        SendMessage(priorFg, WM_APPCOMMAND, priorFg, (IntPtr)(APPCOMMAND_MEDIA_PAUSE << 16));
                        _pausedWin32Windows.Add(priorFg);
                    }
                }
            }
            catch { }

            // 2. Modern WinRT GSMTC Media Sessions (YouTube in Chrome/Edge/Firefox, WMP, VLC)
            try
            {
                var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                if (sessionManager != null)
                {
                    var sessions = sessionManager.GetSessions();
                    foreach (var session in sessions)
                    {
                        try
                        {
                            if (!IsTargetMediaApp(session.SourceAppUserModelId))
                            {
                                continue; // Strictly protect ffmpeg, Python, system sounds, etc.
                            }

                            var playbackInfo = session.GetPlaybackInfo();
                            if (playbackInfo != null &&
                                playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                            {
                                bool paused = await session.TryPauseAsync();
                                if (paused)
                                {
                                    _pausedGsmtcSessions.Add(session);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // 3. Classic Win32 Desktop Players: VLC and WMP ONLY
            PauseTargetDesktopPlayers();
        }

        private static void PauseTargetDesktopPlayers()
        {
            try
            {
                string[] playerProcessNames = { "vlc", "wmplayer" };
                foreach (string procName in playerProcessNames)
                {
                    var procs = Process.GetProcessesByName(procName);
                    foreach (var proc in procs)
                    {
                        try
                        {
                            if (proc.MainWindowHandle != IntPtr.Zero && !_pausedWin32Windows.Contains(proc.MainWindowHandle))
                            {
                                SendMessage(proc.MainWindowHandle, WM_APPCOMMAND, proc.MainWindowHandle, (IntPtr)(APPCOMMAND_MEDIA_PAUSE << 16));
                                _pausedWin32Windows.Add(proc.MainWindowHandle);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Restores media playback for sessions that were actively playing and paused by Death.
        /// </summary>
        public static async Task RestorePausedMediaAsync()
        {
            // 1. Resume WinRT GSMTC sessions that were paused by Death
            foreach (var session in _pausedGsmtcSessions)
            {
                try
                {
                    await session.TryPlayAsync();
                }
                catch { }
            }
            _pausedGsmtcSessions.Clear();

            // 2. Resume Classic Win32 Desktop Players (VLC, WMP)
            foreach (var hWnd in _pausedWin32Windows)
            {
                try
                {
                    SendMessage(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)(APPCOMMAND_MEDIA_PLAY << 16));
                }
                catch { }
            }
            _pausedWin32Windows.Clear();
        }
        #endregion



        #region --- PCM SOUND SYNTHESIZER ---
        /// <summary>
        /// Synthesizes and plays an in-memory pure sine wave WAV buffer.
        /// Volume is calibrated (default 0.15) to sit subtly below the ambient whistle.
        /// </summary>
        public static void PlayBeep(double frequency, int durationMs, double volume = 0.15)
        {
            try
            {
                int sampleRate = 44100;
                int numSamples = (int)(sampleRate * (durationMs / 1000.0));
                byte[] waveBuffer = new byte[44 + numSamples * 2];

                // RIFF Header
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, waveBuffer, 0, 4);
                int fileSize = 36 + numSamples * 2;
                Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, waveBuffer, 4, 4);
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, waveBuffer, 8, 4);

                // Subchunk 1 (fmt )
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, waveBuffer, 12, 4);
                int subchunk1Size = 16;
                Buffer.BlockCopy(BitConverter.GetBytes(subchunk1Size), 0, waveBuffer, 16, 4);
                short audioFormat = 1; // PCM
                Buffer.BlockCopy(BitConverter.GetBytes(audioFormat), 0, waveBuffer, 20, 2);
                short numChannels = 1; // Mono
                Buffer.BlockCopy(BitConverter.GetBytes(numChannels), 0, waveBuffer, 22, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(sampleRate), 0, waveBuffer, 24, 4);
                int byteRate = sampleRate * 2;
                Buffer.BlockCopy(BitConverter.GetBytes(byteRate), 0, waveBuffer, 28, 4);
                short blockAlign = 2;
                Buffer.BlockCopy(BitConverter.GetBytes(blockAlign), 0, waveBuffer, 32, 2);
                short bitsPerSample = 16;
                Buffer.BlockCopy(BitConverter.GetBytes(bitsPerSample), 0, waveBuffer, 34, 2);

                // Subchunk 2 (data)
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, waveBuffer, 36, 4);
                int subchunk2Size = numSamples * 2;
                Buffer.BlockCopy(BitConverter.GetBytes(subchunk2Size), 0, waveBuffer, 40, 4);

                // Sine wave samples
                double t = 0.0;
                double dt = 2.0 * Math.PI * frequency / sampleRate;
                double amplitude = 32767.0 * volume;
                for (int i = 0; i < numSamples; i++)
                {
                    short sample = (short)(amplitude * Math.Sin(t));
                    Buffer.BlockCopy(BitConverter.GetBytes(sample), 0, waveBuffer, 44 + i * 2, 2);
                    t += dt;
                }

                // SND_MEMORY = 0x0004, SND_NODEFAULT = 0x0002
                PlaySound(waveBuffer, IntPtr.Zero, 6);
            }
            catch { }
        }

        public static void PlayBeepAsync(double frequency, int durationMs, double volume = 0.15)
        {
            ThreadPool.QueueUserWorkItem(_ => PlayBeep(frequency, durationMs, volume));
        }

        /// <summary>
        /// Mechanical clock tick-tock. Calibrated slightly lower than whistle volume (0.15 vs 0.40).
        /// </summary>
        public static void PlayTick(bool isEven)
        {
            double freq = isEven ? 1800.0 : 1500.0;
            PlayBeepAsync(freq, 15, 0.15);
        }

        /// <summary>
        /// Single resonant release chime when countdown completes (1 time only, 1200Hz).
        /// </summary>
        public static void PlaySingleEndChime()
        {
            PlayBeepAsync(1200.0, 300, 0.30);
        }
        #endregion

        #region --- TOPMOST & VIRTUAL DESKTOP INTEGRITY ---
        public static bool IsOnCurrentDesktop(IntPtr hWnd)
        {
            if (_desktopManager == null) return true;
            try
            {
                int hr = _desktopManager.IsWindowOnCurrentVirtualDesktop(hWnd, out bool onCurrent);
                if (hr == 0) return onCurrent;
            }
            catch { }
            return true;
        }

        public static void MoveToDesktop(IntPtr hWnd, Guid desktopId)
        {
            if (_desktopManager == null) return;
            try
            {
                _desktopManager.MoveWindowToDesktop(hWnd, ref desktopId);
            }
            catch { }
        }

        /// <summary>
        /// Enforces topmost placement, foreground lock bypass, and hardware full-screen rect
        /// to guarantee the Windows 11 taskbar never bleeds over the lockdown overlay.
        /// </summary>
        public static void EnforceTopmost(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;
            try
            {
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);

                IntPtr fgWnd = GetForegroundWindow();
                if (fgWnd != hWnd)
                {
                    uint fgThread = GetWindowThreadProcessId(fgWnd, out _);
                    uint curThread = GetCurrentThreadId();

                    if (fgThread != curThread && fgThread != 0)
                    {
                        AttachThreadInput(curThread, fgThread, true);
                        BringWindowToTop(hWnd);
                        ShowWindow(hWnd, SW_SHOWNORMAL);
                        SetForegroundWindow(hWnd);
                        AttachThreadInput(curThread, fgThread, false);
                    }
                    else
                    {
                        BringWindowToTop(hWnd);
                        ShowWindow(hWnd, SW_SHOWNORMAL);
                        SetForegroundWindow(hWnd);
                    }
                }

                // Explicitly lock window to raw physical display bounds over the taskbar
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, screenW, screenH, SWP_SHOWWINDOW);
            }
            catch { }
        }
        #endregion
    }
}
