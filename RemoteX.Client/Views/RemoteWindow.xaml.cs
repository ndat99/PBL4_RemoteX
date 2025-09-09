using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RemoteX.Core;
using RemoteX.Client.ViewModels;

namespace RemoteX.Client.Views
{
    public partial class RemoteWindow : Window
    {
        private RemoteViewModel _rvm;
        public RemoteWindow()
        {
            InitializeComponent();
            _rvm = new RemoteViewModel();
            this.DataContext = _rvm;
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
