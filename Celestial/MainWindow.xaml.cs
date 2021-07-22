using GalaSoft.MvvmLight.Messaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Celestial {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            using var ms = new MemoryStream(Properties.Resources.Icon);
            var ico = new NotifyIcon() {
                Icon = new Icon(ms),
                Visible = true
            };
            ico.DoubleClick += (o, s) => {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            Messenger.Default.Register<WindowState>(this, s => WindowState = s);
        }

        private void Window_StateChanged(object sender, System.EventArgs e) {
            if (WindowState == WindowState.Minimized)
                this.Hide();
        }
    }
}
