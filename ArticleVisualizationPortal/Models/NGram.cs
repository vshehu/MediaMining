using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ArticleVisualizationPortal.Models
{
    public class NGram
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NGramId { get; set; }
        public int ArticleId { get; set; }       
        public string WordList { get; set; }
        public int Frequency { get; set; }
    }

   
}