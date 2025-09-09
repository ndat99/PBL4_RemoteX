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
using RemoteX.Core;
using RemoteX.Server.ViewModels;
using RemoteX.Core.Models;

namespace RemoteX.Server.Views;

public partial class MainWindow : Window
{
    private ServerViewModel _svm;
    public MainWindow()
    {
        InitializeComponent();
        _svm = new ServerViewModel();
        this.DataContext = _svm; //Gan ViewModel vao UI

    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        _svm.StartServer();
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        _svm.StopServer();
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

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        _svm.StopServer();
        this.Close();
    }
}