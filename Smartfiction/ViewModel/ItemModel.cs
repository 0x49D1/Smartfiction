using System;
using System.ComponentModel;

namespace Smartfiction.ViewModel
{
    public class ItemModel : INotifyPropertyChanged
    {
        private string itemTitle;
        public string ItemTitle
        {
            get { return itemTitle; }
            set 
            {
                if (value != itemTitle)
                {
                    itemTitle = value;
                    NotifyPropertyChanged("ItemTitle");
                }
            }
        }

        private string itemDetails;
        public string ItemDetails
        {
            get { return itemDetails; }
            set 
            {
                if (value != itemDetails)
                {
                    itemDetails = value;
                    NotifyPropertyChanged("ItemDetails");
                }
            }
        }

        private string itemLink;
        public string ItemLink
        {
            get { return itemLink; }
            set
            {
                if (value != itemLink)
                {
                    itemLink = value;
                    NotifyPropertyChanged("ItemLink");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
