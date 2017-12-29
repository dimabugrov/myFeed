﻿using System;
using System.Collections.Generic;

namespace myFeed.Models
{
    public sealed class Channel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Uri { get; set; }
        
        public bool Notify { get; set; }
        
        public List<Article> Articles { get; set; }
        
        public Channel() => Articles = new List<Article>();
    }
}