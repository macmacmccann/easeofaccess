using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HandMovementAgent_Panel : Page
    {
        // Set at compile time, updated at runtime when the pipe sends a change
        private string _gestureState = "none";
        private CancellationTokenSource _cts;
        private Process _pythonProcess;

        public HandMovementAgent_Panel()
        {
            InitializeComponent();
            // Pipe must be created BEFORE Python starts, otherwise Python's open() fails
            StartPipeListener();
        }

        private void StartPythonProcess()
        {
            // Walk up from the exe to the repo root, then into mapping_gestures
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
                    RedirectStandardOutput = true,   // capture Python stdout
                    RedirectStandardError  = true,   // capture Python stderr (tracebacks)
                }
            };

            // Forward Python stdout → Debug output
            _pythonProcess.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) Debug.WriteLine($"[Python] {e.Data}");
            };

            // Forward Python stderr → Debug output
            _pythonProcess.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) Debug.WriteLine($"[Python ERR] {e.Data}");
            };

            _pythonProcess.Start();
            _pythonProcess.BeginOutputReadLine();
            _pythonProcess.BeginErrorReadLine();
            Debug.WriteLine($"[HandMovement] Python process started. PID={_pythonProcess.Id}");
        }

        private void StartPipeListener()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenLoop(_cts.Token));
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Recreate server stream each time Python reconnects.
                // Byte mode matches Python's open(pipe, 'wb') byte-stream writes.
                using var pipe = new NamedPipeServerStream(
                    "gestures",
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                Debug.WriteLine("[HandMovement] Pipe server created — waiting for Python to connect...");

                DispatcherQueue.TryEnqueue(() =>
                    StatusLabel.Text = "Waiting for Python to connect...");

                // Start Python only on the first iteration — pipe is ready now
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
                // python disconnected — loop restarts and waits for next connection
            }
        }

        private void HandleGesture(string gesture)
        {
            _gestureState = gesture;

            // UI updates must be dispatched back to the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                // Test: show the raw gesture string on screen so you can verify it works
                GestureLabel.Text = _gestureState;

                if (_gestureState == "pinky")
                {
                    // logic placeholder
                }
                else if (_gestureState == "pointing")
                {
                    // logic placeholder
                }
                else if (_gestureState == "peace")
                {
                    // logic placeholder
                }
                else if (_gestureState == "fist")
                {
                    // logic placeholder
                }
                else if (_gestureState == "open_hand")
                {
                    // logic placeholder
                }
                else if (_gestureState == "swipe_left")
                {
                    // logic placeholder
                }
                else if (_gestureState == "swipe_right")
                {
                    // logic placeholder
                }
            });
        }
    }
}
