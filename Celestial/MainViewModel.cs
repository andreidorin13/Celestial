using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Celestial {
    public sealed class MainViewModel : ViewModelBase {
        private const string EXE = "SoTGame";
        private readonly DispatcherTimer _timer;
        private readonly SocketReader _reader;
        private Process _target;

        private Brush _brush;
        public Brush MessageColor {
            get => _brush;
            set => Set(ref _brush, value);
        }

        private string _message;
        public string Message {
            get => _message;
            set => Set(ref _message, value);
        }

        private bool _processFound = false;
        public bool ProcessFound {
            get => _processFound;
            set => Set(ref _processFound, value);
        }

        private string _buttonContent = "Inject";
        public string ButtonContent {
            get => _buttonContent;
            set => Set(ref _buttonContent, value);
        }

        public ICommand Inject { get; set; }

        private void DllInject() {
            var handle = Natives.OpenProcess(
                Natives.PROCESS_CREATE_THREAD     |
                Natives.PROCESS_QUERY_INFORMATION |
                Natives.PROCESS_VM_OPERATION      |
                Natives.PROCESS_VM_READ           |
                Natives.PROCESS_VM_WRITE,
                false,
                _target.Id);

            var loadLibraryAddr = Natives.GetProcAddress(Natives.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            var dll = Path.Combine(Environment.CurrentDirectory, "Astro.dll");
            var malloc = Natives.VirtualAllocEx(handle, IntPtr.Zero, Natives.MAX_PATH, Natives.MEM_COMMIT | Natives.MEM_RESERVE, Natives.PAGE_READWRITE);

            Natives.WriteProcessMemory(handle, malloc, Encoding.Default.GetBytes(dll), (uint)((dll.Length + 1) * Marshal.SizeOf(typeof(char))), out var _);
            Natives.CreateRemoteThread(handle, IntPtr.Zero, 0, loadLibraryAddr, malloc, 0, IntPtr.Zero);
        }

        private bool LookForProcess(string process) {
            var procs = Process.GetProcessesByName(process);

            if (procs.Length <= 0) {
                _target = null;
                MessageColor = Brushes.OrangeRed;
                Message = "Sea of Thieves process not found :(";
                ProcessFound = false;
                return false;
            }

            _target = procs[0];
            MessageColor = Brushes.LightGreen;
            Message = "Found Sea of Thieves process!";
            ProcessFound = true;
            return true;
        }

        public MainViewModel() {
            _reader = new SocketReader("127.0.0.1", 8000);
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Background, (o, e) => LookForProcess(EXE), Dispatcher.CurrentDispatcher) {
                IsEnabled = true
            };

            //var p = Process.GetProcessesByName("SoTGame")[0];
            //var w = new Overlay(p);
            //w.Show();
            //_reader.StartRead();

            Inject = new RelayCommand(() => {
                // Inject DLL
                DllInject();

                // UI updates
                _timer.IsEnabled = false;
                ButtonContent = "Injected!";
                ProcessFound = false;
                Messenger.Default.Send(WindowState.Minimized);

                // Spawn Overlay
                var window = new Overlay(_target);
                window.Show();
                _reader.StartRead();
            });
        }
    }
}
