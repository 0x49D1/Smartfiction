using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Smartfiction.Model;

namespace Smartfiction.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ContentItem> FeedItems { get; set; }

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

        public bool RemoveFavorite(Story story)
        {
            if (StoryRepository.RemoveStory(story))
            {
                Favorits.Remove(story);
                return true;
            }
            return false;
        }

        public bool AddFavorite(string title,
            DateTime publishedDate,
            string link,
            string details)
        {
   try
            {
                StoryRepository.AddNewStory(title, publishedDate, link, details,StoreFavorit);
                return true;
            }
 catch (Exception e)
            {
                
            }
            return false;
        }

 private void StoreFavorit(int insertedID)
        {
            var story = StoryRepository.GetSingleStory(insertedID);
            Favorits.Insert(0, story);
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
