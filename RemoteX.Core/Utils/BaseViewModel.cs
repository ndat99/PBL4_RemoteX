using System.ComponentModel;

namespace RemoteX.Core.Utils
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; //event thong bao cho UI
        //Goi khi 1 property thay doi
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
