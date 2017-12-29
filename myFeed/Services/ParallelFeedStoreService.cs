﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(IFeedStoreService))]
    public sealed class ParallelFeedStoreService : IFeedStoreService
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IFeedFetchService _feedFetchService;
        private readonly ISettingManager _settingManager;
        
        public ParallelFeedStoreService(
            ICategoryManager categoryManager,
            IFeedFetchService feedFetchService,
            ISettingManager settingManager)
        {
            _categoryManager = categoryManager;
            _feedFetchService = feedFetchService;
            _settingManager = settingManager;
        }
        
        public Task<IEnumerable<Article>> LoadAsync(IEnumerable<Channel> channels) => Task.Run(async () =>
        {
            // Remove extra saved articles and save changes based on max articles allowed.
            var maxArticleCount = await _settingManager.GetAsync<int>("MaxArticlesPerFeed");
            var fetchableChannels = channels.ToList();
            await Task.WhenAll(fetchableChannels
                .Where(i => i.Articles.Count > maxArticleCount)
                .Select(i => {
                    i.Articles = i.Articles
                        .OrderByDescending(x => x.PublishedDate)
                        .Take(maxArticleCount).ToList();
                    return _categoryManager.UpdateChannelAsync(i);
                }));

            // Form lookup with trimmed title and feed as keys.
            var existingLookup = fetchableChannels.SelectMany(i => i.Articles)
                .ToLookup(i => (i.Title?.Trim(), i.FeedTitle?.Trim()));

            // Retrieve feed based on single fetcher implementation.
            var fetchTasks = fetchableChannels.Select(i => FetchAsync(i, maxArticleCount));
            var grouppedArticles = await Task.WhenAll(fetchTasks);
            var distinctGroupping = grouppedArticles
                .Select(i => (i.Item1, i.Item2
                    .Where(x => !existingLookup.Contains(
                        (x.Title?.Trim(), x.FeedTitle?.Trim())))
                    .ToArray()))
                .ToList();

            // Save received distinct items into database.
            await Task.WhenAll(distinctGroupping.Select(i =>
            {
                i.Item1.Articles.AddRange(i.Item2);
                return _categoryManager.UpdateChannelAsync(i.Item1);
            }));

            // Return global join with both old and new articles.
            var flatternedArticles = distinctGroupping.SelectMany(i => i.Item2);
            return existingLookup
                .SelectMany(i => i)
                .Concat(flatternedArticles)
                .OrderByDescending(i => i.PublishedDate)
                .ToList()
                .AsEnumerable();
        });

        private Task<Tuple<Channel, IEnumerable<Article>>> FetchAsync(
            Channel fetchableChannel, int maxArticleCount) => Task.Run(async () =>
        {
            // Fetches single feed in a separate thread.
            var uriToFetch = fetchableChannel.Uri;
            var articles = await _feedFetchService.FetchAsync(uriToFetch);
            return new Tuple<Channel, IEnumerable<Article>>(fetchableChannel, articles
                .OrderByDescending(i => i.PublishedDate)
                .Take(maxArticleCount));
        });
    }
}