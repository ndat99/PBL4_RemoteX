using RemoteX.Client.ViewModels;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using RemoteX.Client.Services;

namespace RemoteX.Client.Views
{
    public partial class MainWindow : Window
    {
        private ClientNetworkManager _clientNetwork;
        private ClientViewModel _cvm;
        private NetworkConfig _config;

        private bool isChatExpanded = false;
        private double compactHeight = 400;  // Chiều cao khi đóng chat
        private double expandedHeight = 700; // Chiều cao khi mở chat

        // Dán hàm này vào bên trong lớp MainWindow
        public static T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && predicate(typedChild))
                {
                    return typedChild;
                }
                else
                {
                    var result = FindVisualChild<T>(child, predicate);
                    if (result != null) return result;
                }
            }
            return null;
        }

        // Theo dõi các file đang nhận
        private Dictionary<Guid, FileStream> _receivedFiles = new Dictionary<Guid, FileStream>();
        private Dictionary<Guid, string> _mysentFiles = new Dictionary<Guid, string>();
        public MainWindow()
        {
            InitializeComponent();

            _clientNetwork = new ClientNetworkManager();
            _cvm = new ClientViewModel(_clientNetwork);
            _cvm.PartnerConnected += partnerId =>
            {
                System.Diagnostics.Debug.WriteLine($"[MAIN] PartnerConnected event received: {partnerId}");
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"[MAIN] Creating RemoteWindow");
                    var remoteWindow = new RemoteWindow(_clientNetwork, partnerId);
                    System.Diagnostics.Debug.WriteLine($"[MAIN] Showing RemoteWindow");
                    remoteWindow.Show();
                });
            };

            _clientNetwork.FileChunkReceived += chunk =>
            {
                //Khi nhan duoc mot manh file, ghi nos vao FileStream tuong ung
                if (_receivedFiles.TryGetValue(chunk.FileID, out FileStream fs))
                {
                    try
                    {
                        fs.Write(chunk.Data, 0, chunk.Data.Length);

                        if (chunk.IsLastChunk)
                        {
                            fs.Close();
                            _receivedFiles.Remove(chunk.FileID);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                var fileMsgContainer = FindVisualChild<ContentPresenter>(svChatBox, presenter =>
                                (presenter.Content as FileMessage)?.FileID == chunk.FileID);

                                if (fileMsgContainer != null)
                                {
                                    var btnDown = FindVisualChild<System.Windows.Controls.Button>(fileMsgContainer, btn => btn.Name == "btnDownload");
                                    var btnFolder = FindVisualChild<System.Windows.Controls.Button>(fileMsgContainer, btn => btn.Name == "btnOpenFolder");
                                    if (btnDown != null)
                                    {
                                        //button.Content = "Done!!";
                                    }
                                    if (btnFolder != null) {
                                        btnFolder.Visibility = Visibility.Visible;
                                    }
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[FILE WRITE ERROR] {ex.Message}");
                    }
                }
            };
            _clientNetwork.FileAcceptReceived += acceptMsg =>
            {
                if(_mysentFiles.TryGetValue(acceptMsg.FileID, out string filePath))
                {
                    Task.Run(() => SendFileChunks(filePath, acceptMsg.FileID, acceptMsg.From));
                    _mysentFiles.Remove(acceptMsg.FileID);
                }
            }; 
                this.DataContext = _cvm;
       }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Căn vị trí màn hình thôi
            this.Left = (SystemParameters.PrimaryScreenWidth - this.ActualWidth) / 2;
            var top = (SystemParameters.PrimaryScreenHeight - this.ActualHeight) / 2 - 150;
            this.Top = Math.Max(0, top);

            LoadConfig();
            //await Task.Run(() => _clientNetwork.ConnectAsync("127.0.0.1", 5000));
            try
            {
                await Task.Run(() => _clientNetwork.ConnectAsync(_config.IP, 5000));

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Không thể kết nối tới Server: {ex.Message}");
            }

            txtMessage.KeyDown += (s, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    btnSend_Click(s, args);
                    args.Handled = true; //Ngăn Enter tạo xuống dòng
                }
            };
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

            try
            {
                await Task.Run(() =>
                {
                    _cvm.SendConnectRequest(targetId, password);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConnectRequest Error] {ex.Message}");
                System.Windows.MessageBox.Show($"Lỗi gửi yêu cầu: {ex.Message}");
            }
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

        // Xử lý gửi File
        private void btnFile_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(_cvm.PartnerId)) return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileInfo = new FileInfo(filePath);

                var fileMessage = new FileMessage
                {
                    From = _cvm.InfoVM.ClientInfo.Id,
                    To = _cvm.PartnerId,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    FileID = Guid.NewGuid(),
                    IsMine = true,
                    Timestamp = DateTime.Now,

                    LocalFilePath = filePath
                };

                _clientNetwork.Send(fileMessage);

                _cvm.ChatVM.AddMessage(fileMessage);

                _mysentFiles[fileMessage.FileID] = filePath;
            }
        }

        private void SendFileChunks(string filePath, Guid FileId, string PartnerId)
        {
            try
            {
                const int chunkSize = 64 * 1024;
                byte[] buffer = new byte[chunkSize];

                using (var fileStream = new System.IO.FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var chunk = new FileChunk
                        {
                            FileID = FileId,
                            Data = buffer.Take(bytesRead).ToArray(),
                            IsLastChunk = (fileStream.Position == fileStream.Length),
                            From = _cvm.InfoVM.ClientInfo.Id,
                            To = _cvm.PartnerId
                        };
                        _clientNetwork.Send(chunk);
                        Task.Delay(1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FILE SEND ERROR] {ex.Message}");
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null || !(btn.DataContext is FileMessage fileMessage)) return;

            if (!string.IsNullOrEmpty(fileMessage.LocalFilePath))
            {
                try
                {
                    string folderPath = System.IO.Path.GetDirectoryName(fileMessage.LocalFilePath);

                    if (Directory.Exists(folderPath)) {
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{ex.Message}");
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null || btn.Tag == null) return;
            var fileId = (Guid)btn.Tag;
            var fileMessage = _cvm.ChatVM.Messages.OfType<FileMessage>().FirstOrDefault(f => f.FileID == fileId);
            if(fileMessage == null) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = fileMessage.FileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                fileMessage.LocalFilePath = saveFileDialog.FileName;
                var fileStream = new System.IO.FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
                _receivedFiles[fileId] = fileStream;

                btn.IsEnabled = false;
                btn.Content = "✔";

                var acceptMsg = new FileAcceptMessage
                {
                    FileID = fileId,
                    From = _cvm.InfoVM.ClientInfo.Id,
                    To = fileMessage.From // Gửi lại cho người đã gửi file
                };
                _clientNetwork.Send(acceptMsg);
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