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
using RemoteX.Core;
using RemoteX.Client.ViewModels;
using RemoteX.Client.Controllers;
using System.Threading.Tasks;

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

            this.DataContext = _cvm;

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);                 // đợi 1s rồi connect
            _client.Connect("localhost", 5000);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            RemoteWindow w = new RemoteWindow();
            w.Show();
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