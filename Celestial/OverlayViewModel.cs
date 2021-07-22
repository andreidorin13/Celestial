using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Celestial {
    public sealed class OverlayViewModel : ViewModelBase {
        private readonly Process _target;
        private readonly DispatcherTimer _timer;

        private IntPtr _kbhook = IntPtr.Zero;
        public readonly Natives.LowLevelKeyboardProc proc;

        private int _statsOffset = 0;
        private bool _lastModKey = false;

        private Natives.RECT _lastRect = new Natives.RECT {
            Top = -1,
            Bottom = -1,
            Left = -1,
            Right = -1
        };

        private double _top;
        public double Top {
            get => _top;
            set => Set(ref _top, value);
        }

        private double _left;
        public double Left {
            get => _left;
            set => Set(ref _left, value);
        }

        private double _height;
        public double Height {
            get => _height;
            set => Set(ref _height, value);
        }

        private double _width;
        public double Width {
            get => _width;
            set => Set(ref _width, value);
        }

        private Thickness _thicc;
        public Thickness Thicc {
            get => _thicc;
            set => Set(ref _thicc, value);
        }

        public ObservableCollection<string> Stats { get; private set; } = new ObservableCollection<string> { "Awaiting injection" };

        private void TrackWindow() {
            Natives.GetWindowRect(_target.MainWindowHandle, out var rect);
            if (_lastRect.Equals(rect))
                return;

            Top = rect.Top;
            Left = rect.Left;
            Height = rect.Bottom - rect.Top;
            Width = rect.Right - rect.Left;

            if (Top == 0) // Fullscreen
                Thicc = new Thickness(0, 50, 0, 0);
            else // Account for Windows border
                Thicc = new Thickness(10, 80, 0, 0);

            _lastRect = rect;
        }

        private IntPtr CheckHotKey(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)Natives.WM_KEYDOWN) {
                var key = (System.Windows.Forms.Keys)Marshal.ReadInt32(lParam);

                if (key == System.Windows.Forms.Keys.LControlKey)
                    _lastModKey = true;
                else if (key == System.Windows.Forms.Keys.F && _lastModKey) {
                    Stats.Add("");
                    _statsOffset++;
                }

                Debug.WriteLine($"Down: {key}");
            }

            if (nCode >= 0 && wParam == (IntPtr)Natives.WM_KEYUP) {
                var key = (System.Windows.Forms.Keys)Marshal.ReadInt32(lParam);

                if (key == System.Windows.Forms.Keys.LControlKey)
                    _lastModKey = false;

                Debug.WriteLine($"Up: {key}");
            }

            return Natives.CallNextHookEx(_kbhook, nCode, wParam, lParam);
        }
         
        public OverlayViewModel(Process target) {
            _target = target;
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Normal, (o, e) => TrackWindow(), Dispatcher.CurrentDispatcher) {
                IsEnabled = true
            };

            Messenger.Default.Register<Vec3>(this, v => Application.Current.Dispatcher.Invoke(() => Stats[_statsOffset] = $"X:{v.x} Y:{v.y} Z:{v.z}"));

            proc = CheckHotKey;
            _kbhook = Natives.SetWindowsHookEx(Natives.WH_KEYBOARD_LL, proc, Natives.GetModuleHandle("user32.dll"), 0);
        }
    }
}