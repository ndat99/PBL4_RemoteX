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
using RemoteX.Server.Services;
using RemoteX.Server.ViewModels;
using RemoteX.Shared;

namespace RemoteX.Server.Views;

public partial class MainWindow : Window
{
    private ServerViewModel _svm;
    private RemoteXServer _server;
    public MainWindow()
    {
        InitializeComponent();
        _svm = new ServerViewModel();
        this.DataContext = _svm;

        //_server = new RemoteXServer();
        //_server.StatusChanged += (msg) =>
        //    Dispatcher.Invoke(() => _svm.Status = msg);
        _server = new RemoteXServer();
        _server.ClientConnected += (clientInfo) =>
            Dispatcher.Invoke(() => _svm.Clients.Add(clientInfo));

        _server.Start(4000);
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

    // Maximizewindow
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
        MainWindow w = new MainWindow();
        w.Show();
    }

}