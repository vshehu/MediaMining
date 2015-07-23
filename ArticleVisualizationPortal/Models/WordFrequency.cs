using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ArticleVisualizationPortal.Models
{
    public class WordFrequency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WordFrequencyId { get; set; }

        public int WordId { get; set; }
        public int ArticleId { get; set; }
        public int Frequency { get; set; }
    }
}