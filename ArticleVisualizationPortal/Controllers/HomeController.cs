using ArticleVisualizationPortal.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ArticleVisualizationPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DocumentUploader()
        {
            return View();
        }

        public ActionResult TrendVisualization()
        { 
            return View();
        }

        public ActionResult CloudVisualization()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetTrends(string date_from, string date_to,  string word)
        {

            string[] cols = word.Split(',');


            DateTime start = Convert.ToDateTime(date_from);
            DateTime end = Convert.ToDateTime(date_to);
            ArticleDBContext context = new ArticleDBContext();

            
            var words = from wf in context.WordFrequenies
                        join w in context.Words on wf.WordId equals w.WordId
                        join a in context.Articles on wf.ArticleId equals a.ArticleId
                        where   (a.DateRetrieved <= end) &&
                                (a.DateRetrieved >= start) &&
                                (w.Text.ToLower() == word.ToLower())
                        select new {a.DateRetrieved, w.Text, wf.Frequency };

            var res = from wf in words
                      group wf by new { wf.DateRetrieved, wf.Text }  into g
                      select new
                      {
                          Date = g.FirstOrDefault().DateRetrieved,
                          Frequency = g.Sum(x => x.Frequency)
                      };

            var ret = new object[res.Count() + 1];

            ret[0] = new[] { "Data", word };

            int cnt = 1;
                foreach (var record in res)
                {
                    ret[cnt] = new object[] { record.Date.ToShortDateString(), record.Frequency };
                    cnt++;
                }
            return new JsonResult()
            {
                Data = ret,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        [HttpGet]
        public JsonResult GetAuthors(string media_name)
        {
            ArticleDBContext context = new ArticleDBContext();
            var authors = from a in context.Articles
                          where a.MediaName == media_name
                          select new { a.Author };
            List<string> ret = new List<string>();
            foreach (var record in authors)
            {
                if (!ret.Contains(record.Author) && !String.IsNullOrEmpty(record.Author.Trim()))
                    ret.Add(record.Author);
            }

            return new JsonResult()
            {
                Data = ret,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult GetCloudData(string media_name, string date_from, string date_to, string author_name, int word)
        {
            List<string> ret = new List<string>();
            DateTime start = Convert.ToDateTime(date_from);
            DateTime end = Convert.ToDateTime(date_to);
            ArticleDBContext context = new ArticleDBContext();
            if (word == 1)
            {
                var words = from wf in context.WordFrequenies
                            join w in context.Words on wf.WordId equals w.WordId
                            join a in context.Articles on wf.ArticleId equals a.ArticleId
                            where (a.MediaName == media_name) &&
                                    (a.DateRetrieved <= end) &&
                                    (a.DateRetrieved >= start) &&
                                    w.Text.Length > 3 &&
                                    (author_name == "0" || a.Author == author_name)
                            select new { w.Text, wf.Frequency };

                var res = from wf in words
                          group wf by wf.Text into g
                          select new
                          {
                              Word = g.FirstOrDefault().Text,
                              Frequency = g.Sum(x => x.Frequency)
                          };


                foreach (var record in res)
                {
                    for (int i = 0; i < record.Frequency; i++)
                    {
                        ret.Add(record.Word.ToLower());
                    }
                }


            }
            else //ne kete rast marim fjalet ndryshe. Do ti mar prej frazave duke injoruar fjalet e shkurtra
            {
                var phrases  = from phrase in context.Phrases
                               join art in context.Articles on phrase.ArticleId equals art.ArticleId
                               where (art.MediaName == media_name) &&
                                    (art.DateRetrieved <= end) &&
                                     (art.DateRetrieved >= start) &&
                                     (author_name == "0" || art.Author == author_name)
                                     select new {phrase.Text};

                char[] delim = {',',' '};
                foreach (var p in phrases)
                {
                    string[] words = p.Text.Split(delim);
                    string ngram;

                    if (words.Where(x => x.Trim().Length > 3).Count() <= word)
                    {
                        ngram = string.Join(" ", words.Where(x => x.Length > 3));
                        ret.Add(ngram);
                    }
                    else
                    { 
                        //vetem fjalet me te medha se 3 shkronja
                        for (int i = 0; i < words.Where(x => x.Trim().Length > 3).Count(); i++)
                        {
                            try
                            {
                                if (word == 2)
                                {                                     
                                    ngram = words[i] + " " + words[i + 1];
                                    ret.Add(ngram);                                    
                                }
                                else
                                {
                                     
                                        ngram = words[i] + " " + words[i + 1] + " " + words[i + 2];
                                        ret.Add(ngram);
                                    
                                }
                            }
                            catch (Exception ex)
                            { 
                            }
                           
                        }
                    }

                }
 
            
            }


            return new JsonResult()
            {
                Data = ret,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost] 
        public ActionResult AddArticle( string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                ViewBag.Message = "Ju lutem shtoeni adresen a artikullit!";
                return View();
            }
          
           
            //e lexojme permbajtjen e faqes se dhene 
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string permbajtja = reader.ReadToEnd();
            reader.Close();
            response.Close();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(permbajtja);
              string paHtml = "";

              ArticleDBContext context = new ArticleDBContext();


              Article a = new Article();
              
            //Te shekulli permbajtja eshte brenda klases tekst
         
                if (url.ToLower().Contains("shekulli"))
                {
                    paHtml = doc.DocumentNode.SelectNodes("//div[@class='tekst']").First().InnerText;
                     
                    a.Author  = doc.DocumentNode.SelectNodes("//ul[@class='li-info1lajmi']").Descendants()
                                .Where(x => x.Name == "li" && x.ChildNodes.FirstOrDefault().Name == "a").FirstOrDefault().InnerText;

                    string data = doc.DocumentNode.SelectNodes("//ul[@class='li-info1lajmi']").Descendants()
                                .Where(x => x.Name == "li").ToList()[1].InnerText;


                    int dita, muaji, viti;
                    dita = Convert.ToInt32(data.Split(',')[0].Split(' ')[0]);
                    Helper.Helper h = new Helper.Helper();

                    muaji = h.Muajt[data.Split(',')[0].Split(' ')[1]];
                    viti = Convert.ToInt32(data.Split(',')[0].Split(' ')[2]);
                    a.DateRetrieved = new DateTime(viti, muaji, dita);
                    a.MediaName = "Shekulli";
                }
                else if (url.ToLower().Contains("gazeta-shqip")) 
                {
                    paHtml = doc.DocumentNode.SelectNodes("//div[@class='td-ss-main-content']").First().InnerText;
                    a.Author = doc.DocumentNode.SelectNodes("//div[@class='td-post-author-name']")[0].InnerText.Replace("Nga", "").Replace("-", "");
                    a.MediaName = "Gazeta Shqip";

                    string data = doc.DocumentNode.SelectNodes("//div[@class='td-post-date']")[0].InnerText;

                    int dita, muaji, viti;
                    dita = Convert.ToInt32(data.Split(' ')[0]);
                    Helper.Helper h = new Helper.Helper();

                    muaji = h.Muajt[data.Split(' ')[1]];
                    viti = Convert.ToInt32(data.Split(' ')[2]);
                    a.DateRetrieved = new DateTime(viti, muaji, dita);

                }
                else if (url.ToLower().Contains("mapo")) 
                {
                  paHtml = doc.GetElementbyId("content").InnerText;
                  a.Author = "Mapo";
                  a.MediaName = "Mapo";
                }
                else //if (url.ToLower().Contains("sot.com.al")) //kjo pjese te c'komentohet nese shtohen mediume tjera
                {
                  paHtml = doc.GetElementbyId("block-system-main").InnerText;
                  a.MediaName = "Gazeta Sot";
                  //Sunday, July 26, 2015
                  string data = doc.DocumentNode.SelectNodes("//span[@class='date-display-single']")[0].InnerText.Split('-')[0];

                  a.Author = "Gazeta Sot";

                  int dita, muaji, viti;
                  dita = Convert.ToInt32(data.Split(',')[1].Split(' ')[2]);
                  Helper.Helper h = new Helper.Helper();

                  muaji = h.Muajt[data.Split(',')[1].Split(' ')[1]];
                  viti = Convert.ToInt32(data.Split(',')[2]);
                  a.DateRetrieved = new DateTime(viti, muaji, dita);


                }
              
            a.Url = url;
            context.Articles.Add(a);

            context.SaveChanges();

            //Pastrojme hapesirat e shprazeta
            var regex = new Regex(
               "(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)",
               RegexOptions.Singleline | RegexOptions.IgnoreCase
            );

            paHtml = regex.Replace(paHtml, "");
            paHtml = paHtml.Replace('\n', ' ');
            paHtml = paHtml.Replace('\t', ' ');

            

            //nxjerim fjalite. Supozojme se fjalite ndahen me !, ., ;, ?
            char[] delim_fjalite = {'.', '!', '?', ';' };
            string[] fjalite = paHtml.Split(delim_fjalite);
            char[] delim_fjalet = { '“', '”', ' ', '.', '!', '?', ';', '"', '}', '{', '(', ')', '-', '=', '+', '*', '"', ';', ':', '!', '?','<', '>', ',' };

            foreach (string fjali in fjalite)
            {
                Phrase f = new Phrase();
                string[] words = fjali.Split(delim_fjalet);
                f.Text = String.Join(" ", words.Where(x => x.Length > 3));

                f.ArticleId = a.ArticleId;
             

                foreach (char c in delim_fjalet)
                {
                    f.Text = f.Text.Replace(c, ' ');
                }
                
                context.Phrases.Add(f);
            }

            //nxjerim fjalet. Supozojme se fjalet ndahen si fjalite, por edhe me hapesire te shprazet dhe presje
           
            string[] fjalet = paHtml.Split(delim_fjalet);
            foreach (string fjale in fjalet.Where(x=>x.Length > 3))
            {
                //nese fjala nuk ekziston ne databaze
                if (context.Words.Where(x => x.Text.ToLower() == fjale.ToLower()).Count() == 0)
                {
                    Word w = new Word();
                    w.Text = fjale.ToLower();

                    context.Words.Add(w);
                    context.SaveChanges();

                    WordFrequency wf = new WordFrequency();
                    wf.ArticleId = a.ArticleId;
                    wf.WordId = w.WordId;
                    wf.Frequency = 1;
                    context.WordFrequenies.Add(wf);
                    context.SaveChanges();
                }
                else
                {
                    Word w = context.Words.Where(x => x.Text.ToLower() == fjale.ToLower()).First();
                    WordFrequency wf = context.WordFrequenies.Where(x => x.WordId == w.WordId && x.ArticleId == a.ArticleId).FirstOrDefault();

                    if (wf != null)
                    {
                        wf.Frequency++;
                        context.SaveChanges();
                    }
                    else
                    {
                       WordFrequency wfNew = new WordFrequency();
                       wfNew.ArticleId = a.ArticleId;
                       wfNew.WordId = w.WordId;
                       wfNew.Frequency = 1;
                       context.WordFrequenies.Add(wfNew);
                       context.SaveChanges();
                    }
                }
            }

            //regjistrojme ne db
           
       

            return View("DocumentUploader");

        }
    }
}
