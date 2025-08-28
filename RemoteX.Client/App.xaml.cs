using RemoteX.Core.Network;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RemoteX.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var client = new RemoteTcpClient();
        await client.ConnectAsync("127.0.0.1", 4000);
    }
}

