using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;

namespace Smartfiction.ViewModel
{
    public class MainModel
    {
        public ObservableCollection<ItemModel> FeedItems  {get; set;}
    }
}
