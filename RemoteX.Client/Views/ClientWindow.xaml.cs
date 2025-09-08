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
using RemoteX.Client.Services;
using RemoteX.Client.ViewModels;
using RemoteX.Core.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RemoteX.Core.Utils;

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
            _vm = new MainViewModel(_client);
            this.DataContext = _vm;

            _client.StatusChanged += OnStatusChanged;
            _client.ClientConnected += OnClientConnected;
            _client.MessageReceived += OnMessageReceived;
            //string _currentPartnerId = txtPartnerID.Text;
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

        private void OnMessageReceived(ChatMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                msg.IsMine = false;
                _vm.ChatVM.Messages.Add(msg);

                // Nếu server confirm kết nối thì lưu PartnerID
                if (msg.SenderID == "Server" && msg.Message.Contains("Kết nối tới"))
                {
                    _vm.ClientVM.CurrentPartnerId = msg.ReceiverID; // A lưu B
                }
                else if (msg.SenderID == "Server" && msg.Message.Contains("được kết nối"))
                {
                    _vm.ClientVM.CurrentPartnerId = msg.ReceiverID; // B lưu A
                }
            });
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) // <-- async void
        {
            await Task.Delay(1000);                 // đợi 1s rồi connect
            _client.Connect("localhost", 5000);
            //_client.StartListening();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string partnerId = txtPartnerID.Text.Trim();
            string partnerPass = txtPartnerPass.Text.Trim();

            if (string.IsNullOrEmpty(partnerId) || string.IsNullOrEmpty(partnerPass))
            {
                MessageBox.Show("Hãy nhập ID và mật khẩu đối tác!");
                return;
            }

            // Gửi ConnectRequest lên server
            var request = new ConnectRequest
            {
                SenderID = _client.ClientInfo.Id,  //ID của máy A
                TargetID = partnerId,
                Password = partnerPass
            };
            _client.SendMessage(request);

            RemoteWindow w = new RemoteWindow();
            w.Show();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string partnerId = _vm.ClientVM.CurrentPartnerId;
            string message = _vm.ChatVM.InputMessage?.Trim();


            if (string.IsNullOrEmpty(partnerId))
            {
                MessageBox.Show("Chưa kết nối tới đối tác nào!");
                return;
            }

            var msg = new ChatMessage
            {
                SenderID = _vm.ClientVM.ClientInfo.Id,
                ReceiverID = partnerId,
                Message = message,
                IsMine = true
            };

            _client.SendMessage(msg);

            _vm.ChatVM.Messages.Add(msg);

            //_vm.ChatVM.SendMessage(_vm.ClientVM.ClientInfo.Id, partnerId);

            txtMessage.Clear();
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