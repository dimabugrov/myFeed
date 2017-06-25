﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions.Mvvm;
using myFeed.Extensions.Mvvm.Implementation;
using myFeed.FeedModels.Models;
using myFeed.Sources.Controls;

namespace myFeed.Sources
{
    /// <summary>
    /// Represents source category view model.
    /// </summary>
    public class SourceCategoryViewModel : ViewModelBase
    {
        private readonly FeedCategoryModel _model;
        private readonly SourcesPageViewModel _parent;
        public SourceCategoryViewModel(FeedCategoryModel model, SourcesPageViewModel parent)
        {
            (_parent, _model, Title.Value) = (parent, model, model.Title);
            _model.Websites
                .Select(i => new SourceItemViewModel(i, this))
                .ToList()
                .ForEach(Items.Add);
        }

        #region Properties

        /// <summary>
        /// Grouping title.
        /// </summary>
        public IObservableProperty<string> Title { get; } = 
            new ObservableProperty<string>(string.Empty);

        /// <summary>
        /// New source Uri for 2-way binding TextBox.
        /// </summary>
        public IObservableProperty<string> NewSourceUri { get; } =
            new ObservableProperty<string>(string.Empty);

        /// <summary>
        /// Inner items collection.
        /// </summary>
        public ObservableCollection<SourceItemViewModel> Items { get; } = 
            new ObservableCollection<SourceItemViewModel>();

        /// <summary>
        /// Is category showing?
        /// </summary>
        public IObservableProperty<bool> IsContentPresent { get; } =
            new ObservableProperty<bool>(false);

        #endregion

        #region Methods

        /// <summary>
        /// Adds new source to this category.
        /// </summary>
        public async void AddSource()
        {
            // Return if uri's invalid.
            var sourceUri = NewSourceUri.Value;
            NewSourceUri.Value = string.Empty; 
            if (string.IsNullOrWhiteSpace(sourceUri) ||
                !Uri.IsWellFormedUriString(sourceUri, UriKind.Absolute))
                return;

            // Add model.
            var model = new SourceItemModel(sourceUri, true);
            var success = await SourcesManager.GetInstance().AddSource(model, Title.Value);
            if (!success) return;

            // Update UI.
            Items.Add(new SourceItemViewModel(model, this));
        }

        /// <summary>
        /// Removes the entire category.
        /// </summary>
        public async void RemoveCategory()
        {
            // Prepare dialog.
            var resourceLoader = new ResourceLoader();
            var dialog = new MessageDialog(
                resourceLoader.GetString("DeleteCategory"),
                resourceLoader.GetString("DeleteElement")
            );

            // Show dialog with commands.
            dialog.Commands.Add(new UICommand(resourceLoader.GetString("Delete"), async args =>
            {
                var success = await SourcesManager.GetInstance().DeleteCategory(_model);
                if (success) _parent.Items.Remove(this);
            }));
            dialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel")));
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Renames the category.
        /// </summary>
        public async void RenameCategory()
        {
            // Show rename dialog and get results.
            var renameDialog = new RenameDialog();
            if (await renameDialog.ShowAsync() != ContentDialogResult.Primary) return;

            // Return if new namae is empty.
            var newName = renameDialog.GetNewCategoryTitle();
            if (string.IsNullOrWhiteSpace(newName)) return;

            // Rename category.
            var success = await SourcesManager.GetInstance().RenameCategory(_model, newName);
            if (success) _model.Title = Title.Value = newName;
        }

        /// <summary>
        /// Returns category.
        /// </summary>
        public FeedCategoryModel GetCategory() => _model;

        #endregion
    }
}
