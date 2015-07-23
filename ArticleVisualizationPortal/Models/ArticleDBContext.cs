using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ArticleVisualizationPortal.Models
{
    public class ArticleDBContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<NGram> NGrams { get; set; }
        public DbSet<Phrase> Phrases { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<WordFrequency> WordFrequenies { get; set; }
    }
}