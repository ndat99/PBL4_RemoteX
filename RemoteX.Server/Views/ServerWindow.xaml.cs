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

namespace RemoteX.Server.Views;

public partial class MainWindow : Window
{
    private RemoteXServer _server;
    public MainWindow()
    {
        InitializeComponent();
        _server = new RemoteXServer(); //Khoi tao server
        _server.StatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(string status)
    {
        Dispatcher.Invoke(() =>
        {
            txtStatus.Text = status;
        });
    }
    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        _server.Start();
        MessageBox.Show("Server đã được bật");
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        _server.Stop();
        MessageBox.Show("Đã tắt Server");
    }

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
        this.Close();
    }
}