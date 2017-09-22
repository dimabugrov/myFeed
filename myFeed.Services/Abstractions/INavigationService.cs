﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Provides platform-specific navigation actions.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Brings user into view with view key.
        /// </summary>
        /// <param name="viewKey">View to show.</param>
        Task Navigate(ViewKey viewKey);

        /// <summary>
        /// Brings user into view with parameter.
        /// </summary>
        /// <param name="viewKey">View to show.</param>
        /// <param name="parameter">Parameter to pass.</param>
        Task Navigate(ViewKey viewKey, object parameter);

        /// <summary>
        /// Invoked when view changes.
        /// </summary>
        event EventHandler<ViewKey> Navigated;

        /// <summary>
        /// Returns menu icons for application main menu containing 
        /// platform-type-specific icon codes.
        /// </summary>
        IReadOnlyDictionary<ViewKey, object> Icons { get; }
    }

    /// <summary>
    /// All views enum.
    /// </summary>
    public enum ViewKey
    {
        FeedView,
        FaveView,
        SearchView,
        SourcesView,
        ArticleView,
        SettingsView
    }
}