using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RemoteX.Server.Services;
using RemoteX.Shared;
using RemoteX.Server.ViewModels;
using RemoteX.Shared.Models;

namespace RemoteX.Server.Views;

public partial class MainWindow : Window
{
    private RemoteXServer _server;
    private ServerViewModel _svm;
    public MainWindow()
    {
        InitializeComponent();
        _server = new RemoteXServer(); //Khoi tao server
        _svm = new ServerViewModel();
        this.DataContext = _svm; //Gan ViewModel vao UI
        
        _server.StatusChanged += OnStatusChanged;
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;

        this.Loaded += (s, e) =>
        {
            _server.Start(5000);  // auto chạy
        };
    }

    private void OnStatusChanged(string message)
    {
        Dispatcher.Invoke(() =>
        {
            _svm.StatusText = message;

            if (message.Contains("đang chạy"))
                _svm.StatusColor = _svm.StatusDot = Brushes.Green;
            //if (message.Contains("kết nối"))
            //    _svm.StatusColor = _svm.StatusDot = Brushes.Blue;
            if (message.Contains("Lỗi"))
                _svm.StatusColor = _svm.StatusDot = Brushes.Red;
            else  if (message.Contains("tắt"))
                _svm.StatusColor = _svm.StatusDot = Brushes.Gray;
        });
    }
    private void OnClientConnected(ClientInfo client)
    {
        Dispatcher.Invoke(() =>
        {
            _svm.Clients.Add(client);
        });
    }

    private void OnClientDisconnected(ClientInfo client)
    {
        Dispatcher.Invoke(() =>
        {
            _svm.Clients.Remove(client);
        });
    }
    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        _server.Start();
        //MessageBox.Show("Server đã được bật");
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        _server.Stop();
        //MessageBox.Show("Đã tắt Server");
    }



    //Dưới này là event thu nhỏ, phóng to, close cho cái title bar thôi đừng quan tâm
    //Kéo thả cửa sổ
    private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            this.DragMove();
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
            this.WindowState = WindowState.Normal;
        else
            this.WindowState = WindowState.Maximized;
    }

    // Close window
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        _server.Stop();
        this.Close();
    }
}