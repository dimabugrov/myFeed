﻿using System.Linq;
using myFeed.Extensions.Mvvm;
using myFeed.Extensions.Mvvm.Implementation;
using myFeed.FeedModels.Models;

namespace myFeed.Feed
{
    /// <summary>
    /// Represents feed items collection view model.
    /// </summary>
    public class FeedViewModel : DeeplyObservableCollection<FeedItemViewModel>
    {
        private readonly FeedCategoryModel _category;
        public FeedViewModel(FeedCategoryModel category) => _category = category;

        #region Properties

        /// <summary>
        /// Indicates if fetcher is loading data right now.
        /// </summary>
        public IObservableProperty<bool> IsLoading { get; } = 
            new ObservableProperty<bool>(true);

        /// <summary>
        /// Indicates if the collection is empty.
        /// </summary>
        public IObservableProperty<bool> IsFeedEmpty { get; } = 
            new ObservableProperty<bool>(false);

        #endregion

        #region Methods

        /// <summary>
        /// Fetches data.
        /// </summary>
        public async void FetchAsync()
        {
            // Clear and reset to defaults.
            IsLoading.Value = true;

            // Retrieve feed.
            var manager = FeedManager.GetInstance();
            var orderedViewModels = await manager.RetrieveFeedAsync(_category);
            Clear();
            orderedViewModels.ToList().ForEach(Add);

            // Toggle notifiers.
            IsFeedEmpty.Value = Count == 0;
            IsLoading.Value = false;
        }

        /// <summary>
        /// Navigates user to sources page if he is new to the app.
        /// </summary>
        public void NavigateToSources() => 
            Navigation.NavigationPage
            .NavigationFrame
            .Navigate(typeof(Sources.SourcesPage));

        #endregion
    }
}
