using System.Diagnostics;
using System.Windows;

namespace Celestial {
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window {
        public Overlay(Process process) {
            DataContext = new OverlayViewModel(process);
            InitializeComponent();
        }
    }
}
