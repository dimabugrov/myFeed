﻿using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Platform
{
    public interface INotificationService
    {
        Task SendNotifications(IEnumerable<Article> articles);
    }
}