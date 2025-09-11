using System.Windows;
using System.Windows.Input;
using RemoteX.Client.ViewModels;
using RemoteX.Client.Controllers;

namespace RemoteX.Client.Views
{
    public partial class MainWindow : Window
    {
        private ClientController _client;
        private ClientViewModel _cvm;
        public MainWindow()
        {
            InitializeComponent();

            _client = new ClientController();
            _cvm = new ClientViewModel(_client);
            _cvm.PartnerConnected += partnerId =>
            {
                System.Diagnostics.Debug.WriteLine($"[MAIN] PartnerConnected event received: {partnerId}");
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"[MAIN] Creating RemoteWindow");
                    var remoteWindow = new RemoteWindow(_client, partnerId);
                    System.Diagnostics.Debug.WriteLine($"[MAIN] Showing RemoteWindow");
                    remoteWindow.Show();
                });
            };

            this.DataContext = _cvm;

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //await Task.Delay(1000); // đợi 1s rồi connect
            await _client.Connect("localhost", 5000);
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string targetId = txtPartnerID.Text;
            string password = txtPartnerPass.Text;

            if (string.IsNullOrEmpty(targetId) || string.IsNullOrEmpty(password))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập ID và Password");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[CONNECT] Sending request to {targetId} with password {password}");

            await _cvm.SendConnectRequestAsync(targetId, password);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Gui tin nhan");
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
    }
}