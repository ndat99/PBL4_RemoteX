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
using RemoteX.Client.Services;
using RemoteX.Client.ViewModels;
using RemoteX.Shared.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RemoteX.Shared.Utils;

namespace RemoteX.Client.Views
{
    public partial class MainWindow : Window
    {
        private RemoteXClient _client;
        private MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _client = new RemoteXClient(); ;
            _vm = new MainViewModel();
            this.DataContext = _vm;

            _client.StatusChanged += OnStatusChanged;
            _client.ClientConnected += OnClientConnected;
        }

        private void OnStatusChanged(string message)
        {
            Dispatcher.Invoke(() =>
            {
                _vm.ClientVM.StatusText = message;

                if (message.Contains("Lỗi"))
                    _vm.ClientVM.StatusColor = Brushes.Red;
                else if (message.Contains("kết nối"))
                    _vm.ClientVM.StatusColor = Brushes.Green;
            });
        }

        private void OnClientConnected(ClientInfo clientInfo)
        {
            Dispatcher.Invoke(() =>
            {
                _vm.ClientVM.ClientInfo = clientInfo;
            });
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) // <-- async void
        {
            await Task.Delay(1500);                 // đợi 1.5s rồi connect
            _client.Connect("localhost", 5000);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {

        }

        // Kéo thả cửa sổ bằng title bar
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

        // Maximize window
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