using RemoteX.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace RemoteX.Client.ViewModels
{
    public class RemoteViewModel : BaseViewModel
    {
        private BitmapImage _screen;
        public BitmapImage ScreenView
        {
            get => _screen;
            set 
            { _screen = value;
               OnPropertyChanged(nameof(ScreenView)); 
            }
        }

        public RemoteViewModel()
        {
            //Để test thử hiển thị như nào thôi
            var bmp = new BitmapImage(new Uri("pack://application:,,,/Views/Screenshot.png"));
            ScreenView = bmp;
        }
    }

}
