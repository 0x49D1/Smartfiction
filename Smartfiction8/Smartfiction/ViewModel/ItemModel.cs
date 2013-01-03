using System;
using System.ComponentModel;

namespace Smartfiction.ViewModel
{
    public class ItemModel : INotifyPropertyChanged
    {
        private string itemTitle;
        public string Title
        {
            get { return itemTitle; }
            set 
            {
                if (value != itemTitle)
                {
                    itemTitle = value;
                    NotifyPropertyChanged("Title");
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
        public string Link
        {
            get { return itemLink; }
            set
            {
                if (value != itemLink)
                {
                    itemLink = value;
                    NotifyPropertyChanged("Link");
                }
            }
        }


        private DateTime itemPublishDate;
        public DateTime ItemPublishDate
        {
            get { return itemPublishDate; }
            set
            {
                if (value != itemPublishDate)
                {
                    itemPublishDate = value;
                    NotifyPropertyChanged("ItemPublishDate");
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
