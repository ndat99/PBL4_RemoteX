using System.Windows;
using System.Windows.Input;
using RemoteX.Client.ViewModels;
using RemoteX.Client.Controllers;
using RemoteX.Core.Utils;
using System.Text.Json;
using System.IO;
using System.Windows.Media.Animation;

namespace RemoteX.Client.Views
{
    public partial class MainWindow : Window
    {
        private ClientController _client;
        private ClientViewModel _cvm;
        private NetworkConfig _config;

        private bool isChatExpanded = false;
        private double compactHeight = 400;  // Chiều cao khi đóng chat
        private double expandedHeight = 700; // Chiều cao khi mở chat
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Căn vị trí màn hình thôi
            this.Left = (SystemParameters.PrimaryScreenWidth - this.ActualWidth) / 2;
            var top = (SystemParameters.PrimaryScreenHeight - this.ActualHeight) / 2 - 150;
            this.Top = Math.Max(0, top);

            LoadConfig();
            //Thread.Sleep(1000); // đợi 1s rồi connect
            //_client.Connect("127.0.0.1", 5000);
            _client.Connect(_config.IP, 5000);

            txtMessage.KeyDown += (s, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    btnSend_Click(s, args);
                    args.Handled = true; //Ngăn Enter tạo xuống dòng
                }
            };
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string targetId = txtPartnerID.Text;
            string password = txtPartnerPass.Text;

            if (string.IsNullOrEmpty(targetId) || string.IsNullOrEmpty(password))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập ID và Password");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[CONNECT] Sending request to {targetId} with password {password}");

            new Thread(() =>
            {
                _cvm.SendConnectRequest(targetId, password);
            }).Start();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Gui tin nhan");
            if (string.IsNullOrEmpty(_cvm.PartnerId))
                return;

            string messageText = txtMessage.Text?.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            _cvm.ChatVM.SendMessage(_cvm.InfoVM.ClientInfo?.Id, _cvm.PartnerId, messageText);

            txtMessage.Clear();
            txtMessage.Focus();

            //auto scroll
            if (svChatBox != null)
                svChatBox.ScrollToBottom();
        }

        private void LoadConfig()
        {
            var path = "config.json";

            if (!File.Exists(path))
            {
                var defaultConfig = new NetworkConfig
                {
                    IP = "127.0.0.1"
                };
                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }

            var fileContent = File.ReadAllText(path);
            _config = JsonSerializer.Deserialize<NetworkConfig>(fileContent);
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

        // Close window
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Event handler cho nút toggle chat
        private void btnToggleChat_Click(object sender, RoutedEventArgs e)
        {
            if (isChatExpanded)
            {
                // Ẩn nội dung chat
                CollapseChat();
            }
            else
            {
                // Hiện nội dung chat
                ExpandChat();
            }
        }

        private void ExpandChat()
        {
            // Hiện ChatPanel (chứa chatbox và input)
            ChatPanel.Visibility = Visibility.Visible;
            // Đổi content button
            txtToggleText.Content = "Ẩn ▲";
            // Animation mở rộng window nếu cần
            var currentHeight = this.ActualHeight;
            var targetHeight = Math.Max(currentHeight, 650);
            if (currentHeight < targetHeight)
            {
                var windowAnimation = new DoubleAnimation
                {
                    From = currentHeight,
                    To = targetHeight,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                this.BeginAnimation(Window.HeightProperty, windowAnimation);
            }
            isChatExpanded = true;
        }

        private void CollapseChat()
        {
            // Ẩn ChatPanel (chứa chatbox và input)
            ChatPanel.Visibility = Visibility.Collapsed;
            // Đổi content button
            txtToggleText.Content = "Hiển thị ▼";
            // Animation thu gọn window
            var currentHeight = this.ActualHeight;
            var targetHeight = 470; // Chiều cao chỉ đủ cho header, control panels và chat header
            var windowAnimation = new DoubleAnimation
            {
                From = currentHeight,
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            this.BeginAnimation(Window.HeightProperty, windowAnimation);
            isChatExpanded = false;
        }

    }
}