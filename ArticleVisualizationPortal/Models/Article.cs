using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ArticleVisualizationPortal.Models
{
    public class Article
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArticleId { get; set; }
        public String MediaName { get; set; }
        public String Url { get; set; }
        public String Author { get; set; }
        public DateTime DateRetrieved { get; set; }
    }

  
}