using System.Collections.ObjectModel;
using System.ComponentModel;
using Smartfiction.Model;

namespace Smartfiction.ViewModel
{
    public class MainViewModel:INotifyPropertyChanged
    {
        public ObservableCollection<ContentItem> FeedItems  {get; set;}

        private ObservableCollection<Story> _favorits;
        public ObservableCollection<Story> Favorits
        {
            get { return _favorits; }
            set
            {
                _favorits = value;
                NotifyPropertyChanged("Favorits");
            }
        }


        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
