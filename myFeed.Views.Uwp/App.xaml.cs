﻿using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Views.Uwp.Views;

namespace myFeed.Views.Uwp
{
    public sealed partial class App : Application
    {
        public App() => InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureDefaultViewIsPresent();
            if (Guid.TryParse(e.TileId, out var guid))
                OpenArticleViewForPinnedArticleUsingGuid(guid);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            EnsureDefaultViewIsPresent();
            if (args.Kind == ActivationKind.ToastNotification &&
                args is IToastNotificationActivatedEventArgs e &&
                Guid.TryParse(e.Argument, out var guid)) 
                OpenArticleViewForPinnedArticleUsingGuid(guid);
        }

        private void EnsureDefaultViewIsPresent()
        {
            UnhandledException += (_, args) => Debug.WriteLine(args.Message);
            if (Window.Current.Content == null) Window.Current.Content = new Frame();
            var frame = (Frame)Window.Current.Content;
            if (frame.Content == null) frame.Navigate(typeof(MenuView));
            Window.Current.Activate();
        }

        private async void OpenArticleViewForPinnedArticleUsingGuid(Guid id)
        {
            await new MessageDialog(id.ToString()).ShowAsync();
        }
    }
}