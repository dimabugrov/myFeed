﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using myFeed.Services.Abstractions;
using myFeed.ViewModels.Extensions;

namespace myFeed.Views.Uwp.Services
{
    public sealed class UwpLocator : Locator<
        UwpTranslationsService,
        UwpPlatformService,
        UwpDialogService,
        UwpFilePickerService>
    {
        public static UwpLocator Current => (UwpLocator)Application.Current.Resources["Locator"];

        public Task<T> GetSetting<T>(string key) where T : IConvertible => Resolve<ISettingsService>().Get<T>(key);
    }
}
