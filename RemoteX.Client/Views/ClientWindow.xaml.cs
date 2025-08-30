using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RemoteX.Shared;
using RemoteX.Core;
using RemoteX.Client.Services;

namespace RemoteX.Client.Views
{
    public partial class MainWindow : Window
    {
        private RemoteXClient _client;
        public MainWindow()
        {
            InitializeComponent();
            _client = new RemoteXClient(); ;
            _client.StatusChanged += OnStatusChanged;
        }

        private void OnStatusChanged(string message)
        {
            MessageBox.Show(message);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _client.Connect("127.0.0.1", 5000);
        }




        // Cho phép kéo thả cửa sổ bằng title bar
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Minimize window
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize/Restore window
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // Close window
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            RemoteWindow w = new RemoteWindow();
            w.Show();
        }
    }
}