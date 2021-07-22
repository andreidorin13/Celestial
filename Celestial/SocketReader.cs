using GalaSoft.MvvmLight.Messaging;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Celestial {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct Vec3 {
        public float x, y, z;
    }

    public sealed class SocketReader {
        private readonly string _addr;
        private readonly int _port;
        private readonly TcpClient _tcp;

        private void Read() {
            var task = _tcp.BeginConnect(_addr, _port, null, null);
            task.AsyncWaitHandle.WaitOne(5000); // Wait 5 seconds for socket to open

            var stream = _tcp.GetStream();
            var buf = new byte[12];
            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);

            while (true) {
                try {
                    stream.Read(buf, 0, buf.Length);
                } catch (Exception) {
                    Environment.Exit(0);
                }
                
                var vec = (Vec3)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Vec3));
                Messenger.Default.Send(vec);
            }
        }

        public void StartRead() => Task.Run(() => Read());

        public SocketReader(string addr, int port) {
            _addr = addr;
            _port = port;
            _tcp = new TcpClient();
        }
    }
}
