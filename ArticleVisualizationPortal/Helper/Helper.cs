using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArticleVisualizationPortal.Helper
{
    public class Helper
    {
        public Dictionary<string, int> Muajt  { get; set; }
        

        public Helper()
        {
            Muajt = new Dictionary<string, int>();
            Muajt.Add("Janar", 1);
            Muajt.Add("Shkurt", 2);
            Muajt.Add("Mars", 3);
            Muajt.Add("Prill", 4);
            Muajt.Add("Maj", 5); 
            Muajt.Add("Qershor", 6);
            Muajt.Add("Korrik", 7); 
            Muajt.Add("Gusht", 8);
            Muajt.Add("Shtator", 9);
            Muajt.Add("Tetor", 10);
            Muajt.Add("Nëntor", 11);
            Muajt.Add("Dhjetor", 12);
            Muajt.Add("Nentor", 11);

            Muajt.Add("January", 1);
            Muajt.Add("February", 2);
            Muajt.Add("March", 3);
            Muajt.Add("April", 4);
            Muajt.Add("May", 5);
            Muajt.Add("June", 6);
            Muajt.Add("July", 7);
            Muajt.Add("August", 8);
            Muajt.Add("September", 9);
            Muajt.Add("October", 10);
            Muajt.Add("November", 11);
            Muajt.Add("December", 12);

        }
    }
}