﻿using System;
using System.Data.Linq.Mapping;

namespace Smartfiction.Model
{
    [Table]
    public class Story
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int StoryID { get; set; }

        [Column(CanBeNull = false)]
        public string Title { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DatePublished { get; set; }

        [Column(CanBeNull = false)]
        public string Link { get; set; }

        [Column(CanBeNull = true, DbType = "NTEXT")]
        public string Details { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }
    }
}
