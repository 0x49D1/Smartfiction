﻿using System;
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

        private ObservableCollection<Story> _history;
        public ObservableCollection<Story> History
        {
            get { return _history; }
            set
            {
                _history = value;
                NotifyPropertyChanged("History");
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
                StoryRepository.RemoveStory(title, true);
                StoryRepository.AddNewStory(title, publishedDate, link, details, true, StoreFavorit);
                Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromMilliseconds(50));
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
