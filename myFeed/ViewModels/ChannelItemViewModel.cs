﻿using System;
using System.Reactive.Linq;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [AddINotifyPropertyChangedInterface]
    [ExportEx(typeof(ChannelItemViewModel))]
    public sealed class ChannelItemViewModel
    {
        public ReactiveCommand Delete { get; }
        public ReactiveCommand Open { get; }
        public ReactiveCommand Copy { get; }

        public bool Notify { get; set; }
        public string Name { get; }
        public string Url { get; }

        public ChannelItemViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            ChannelGroupViewModel parent,
            Category category,
            Channel channel)
        {
            Url = channel.Uri;
            Notify = channel.Notify;
            Name = new Uri(channel.Uri).Host;
            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                parent.Items.Remove(this);
                category.Channels.Remove(channel);
                await categoryManager.UpdateAsync(category);
            });
            Copy = ReactiveCommand.CreateFromTask(() => platformService.CopyTextToClipboard(Url));
            Open = ReactiveCommand.CreateFromTask(
                () => platformService.LaunchUri(new UriBuilder(new Uri(Url)) {Fragment = string.Empty}.Uri),
                this.WhenAnyValue(x => x.Url).Select(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
            );
            this.WhenAnyValue(x => x.Notify)
                .Select(x => channel.Notify = x)
                .Subscribe(async x => await categoryManager.UpdateChannelAsync(channel));
        }
    }
}