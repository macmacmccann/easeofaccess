using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace main_interface
{
    public sealed partial class HandMovementAgent_Panel : Page
    {
        private string _gestureState = "none";
        private CancellationTokenSource _cts;
        private Process _pythonProcess;
        private bool _gesturesEnabled = false;

        public HandMovementAgent_Panel()
        {
            InitializeComponent();
            DesignGlobalCode.HeaderColour(Headertop);
            // Pipe must be ready before Python tries to connect
            StartPipeListener();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        // ── Master toggle ────────────────────────────────────────────────────

        private void GesturesEnabledToggled(object sender, RoutedEventArgs e)
        {
            _gesturesEnabled = GesturesEnabledToggle.IsOn;
            HeaderColour(Headertop);
            Debug.WriteLine($"[HandMovement] Gestures enabled: {_gesturesEnabled}");

            if (_gesturesEnabled)
            {
                // Re-start listener + Python when toggling back on
                StartPipeListener();
            }
            else
            {
                StopPythonProcess();
                StatusLabel.Text = "Stopped.";
            }
        }

        private void StopPythonProcess()
        {
            // Cancel the pipe listener loop first
            _cts?.Cancel();
            _cts = null;

            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill(entireProcessTree: true); // kill Python + any child processes it spawned
                    _pythonProcess.WaitForExit(2000);
                    Debug.WriteLine("[HandMovement] Python process killed.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HandMovement] Kill failed: {ex.Message}");
            }
            finally
            {
                _pythonProcess?.Dispose();
                _pythonProcess = null;
            }
        }

        // ── Python process ───────────────────────────────────────────────────

        private void StartPythonProcess()
        {
            var repoRoot  = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\..\"));
            var pythonExe = Path.Combine(repoRoot, "mapping_gestures", ".venv", "Scripts", "python.exe");
            var script    = Path.Combine(repoRoot, "mapping_gestures", "main.py");
            var workDir   = Path.Combine(repoRoot, "mapping_gestures");

            Debug.WriteLine($"[HandMovement] BaseDirectory : {AppContext.BaseDirectory}");
            Debug.WriteLine($"[HandMovement] repoRoot      : {repoRoot}");
            Debug.WriteLine($"[HandMovement] pythonExe     : {pythonExe}  exists={File.Exists(pythonExe)}");
            Debug.WriteLine($"[HandMovement] script        : {script}  exists={File.Exists(script)}");

            if (!File.Exists(pythonExe) || !File.Exists(script))
            {
                Debug.WriteLine("[HandMovement] ERROR: paths above are wrong — Python will not start.");
                DispatcherQueue.TryEnqueue(() =>
                    StatusLabel.Text = "ERROR: could not find python or main.py — check debug output.");
                return;
            }

            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = pythonExe,
                    Arguments              = $"\"{script}\"",
                    WorkingDirectory       = workDir,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                }
            };

            _pythonProcess.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) Debug.WriteLine($"[Python] {e.Data}");
            };
            _pythonProcess.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) Debug.WriteLine($"[Python ERR] {e.Data}");
            };

            _pythonProcess.Start();
            _pythonProcess.BeginOutputReadLine();
            _pythonProcess.BeginErrorReadLine();
            Debug.WriteLine($"[HandMovement] Python process started. PID={_pythonProcess.Id}");

            // Find the cv2 camera window once Python has had time to open it,
            // then style it as a borderless always-on-top tool window
            _ = FindAndStylePythonWindow();
        }

        private async Task FindAndStylePythonWindow()
        {
            // Give Python a couple of seconds to open its camera window
            await Task.Delay(2500);

            if (_pythonProcess == null || _pythonProcess.HasExited) return;

            // cv2.imshow title is "hand tracking" — look it up directly by name
            IntPtr cameraHwnd = FindWindow(null, "hand tracking");

            if (cameraHwnd == IntPtr.Zero)
            {
                Debug.WriteLine("[HandMovement] Camera window not found — Python may not have opened one.");
                return;
            }

            // Hide from taskbar
            int exStyle  = GetWindowLongW(cameraHwnd, GWL_EXSTYLE);
            exStyle     &= ~WS_EX_APPWINDOW;
            exStyle     |=  WS_EX_TOOLWINDOW;
            SetWindowLongW(cameraHwnd, GWL_EXSTYLE, exStyle);

            // Remove titlebar and resize border
            int style  = GetWindowLongW(cameraHwnd, GWL_STYLE);
            style     &= ~(WS_CAPTION | WS_THICKFRAME);
            SetWindowLongW(cameraHwnd, GWL_STYLE, style);

            // Always on top, refresh frame so style change takes effect
            SetWindowPos(cameraHwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);

            Debug.WriteLine($"[HandMovement] Camera window styled. HWND={cameraHwnd}");
        }

        // ── Pipe listener ────────────────────────────────────────────────────

        private void StartPipeListener()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenLoop(_cts.Token));
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var pipe = new NamedPipeServerStream(
                    "gestures",
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                Debug.WriteLine("[HandMovement] Pipe server created — waiting for Python to connect...");
                DispatcherQueue.TryEnqueue(() =>
                    StatusLabel.Text = "Waiting for Python to connect...");

                // Pipe is ready — now safe to launch Python
                if (_pythonProcess == null)
                    StartPythonProcess();

                await pipe.WaitForConnectionAsync(token);

                Debug.WriteLine("[HandMovement] Python connected to pipe.");
                DispatcherQueue.TryEnqueue(() =>
                    StatusLabel.Text = "Connected — reading gestures.");

                using var reader = new StreamReader(pipe);
                string line;
                while ((line = await reader.ReadLineAsync(token)) != null)
                {
                    HandleGesture(line.Trim());
                }
                // Python disconnected — loop recreates pipe and waits again
            }
        }

        // ── Gesture dispatch ─────────────────────────────────────────────────

        private void HandleGesture(string gesture)
        {
            _gestureState = gesture;

            DispatcherQueue.TryEnqueue(() =>
            {
                GestureLabel.Text = _gestureState;

                // Master switch — do nothing if gestures are disabled
                if (!_gesturesEnabled) return;

                if (gesture == "open_hand"   && OpenHandToggle.IsOn)   MinimizeWindow();
                else if (gesture == "fist"   && FistToggle.IsOn)       MaximizeWindow();
                else if (gesture == "pointing" && PointingToggle.IsOn) LeftClick();
                else if (gesture == "peace"  && PeaceToggle.IsOn)      VolumeUp();
                else if (gesture == "pinky"  && PinkyToggle.IsOn)      VolumeDown();
                else if (gesture == "swipe_left"  && SwipeLeftToggle.IsOn)  PrevDesktop();
                else if (gesture == "swipe_right" && SwipeRightToggle.IsOn) NextDesktop();
            });
        }

        // ── Actions (Win32) ──────────────────────────────────────────────────

        private void MinimizeWindow()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return;
            ShowWindow(hwnd, SW_MINIMIZE);
            Debug.WriteLine("[HandMovement] Action: minimize window");
        }

        private void MaximizeWindow()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return;
            ShowWindow(hwnd, SW_MAXIMIZE);
            Debug.WriteLine("[HandMovement] Action: maximize window");
        }

        private void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP,   0, 0, 0, 0);
            Debug.WriteLine("[HandMovement] Action: left click");
        }

        private void VolumeUp()
        {
            keybd_event(VK_VOLUME_UP, 0, 0, 0);
            keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_KEYUP, 0);
            Debug.WriteLine("[HandMovement] Action: volume up");
        }

        private void VolumeDown()
        {
            keybd_event(VK_VOLUME_DOWN, 0, 0, 0);
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYUP, 0);
            Debug.WriteLine("[HandMovement] Action: volume down");
        }

        private void PrevDesktop()
        {
            // Ctrl + Win + Left
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_LWIN,    0, 0, 0);
            keybd_event(VK_LEFT,    0, 0, 0);
            keybd_event(VK_LEFT,    0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN,    0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            Debug.WriteLine("[HandMovement] Action: previous desktop");
        }

        private void NextDesktop()
        {
            // Ctrl + Win + Right
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_LWIN,    0, 0, 0);
            keybd_event(VK_RIGHT,   0, 0, 0);
            keybd_event(VK_RIGHT,   0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN,    0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            Debug.WriteLine("[HandMovement] Action: next desktop");
        }

        // ── Design helpers ───────────────────────────────────────────────────

        public void HeaderColour(Border targetBorder)
        {
            var onBrush  = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
            var offBrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            targetBorder.Background = _gesturesEnabled ? onBrush : offBrush;
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerEntered(sender, e);

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerExited(sender, e);

        // ── Win32 imports ────────────────────────────────────────────────────

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        static extern int GetWindowLongW(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const int  GWL_EXSTYLE    = -20;
        const int  GWL_STYLE      = -16;
        const int  WS_EX_TOOLWINDOW = 0x80;
        const int  WS_EX_APPWINDOW  = 0x40000;
        const int  WS_CAPTION       = 0x00C00000;
        const int  WS_THICKFRAME    = 0x00040000;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOMOVE      = 0x0002;
        const uint SWP_NOSIZE      = 0x0001;
        const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        const int SW_MINIMIZE  = 6;
        const int SW_MAXIMIZE  = 3;

        const byte VK_VOLUME_DOWN = 0xAE;
        const byte VK_VOLUME_UP   = 0xAF;
        const byte VK_CONTROL     = 0x11;
        const byte VK_LWIN        = 0x5B;
        const byte VK_LEFT        = 0x25;
        const byte VK_RIGHT       = 0x27;

        const uint KEYEVENTF_KEYUP    = 0x0002;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP   = 0x0004;
    }
}
