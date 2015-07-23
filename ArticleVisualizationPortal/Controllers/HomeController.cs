using ArticleVisualizationPortal.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
                      group wf by wf.Text into g
                      select new
                      {
                          Date = g.FirstOrDefault().DateRetrieved,
                          Frequency = g.Sum(x => x.Frequency)
                      };

            var ret = new object[res.Count() + 1];

            ret[0] = new[] { "Data", word };
            for (int i = 0; i < res.Count(); i++ )
                foreach (var record in res)
                {
                    ret[i + 1] = new object[] { record.Date.ToShortDateString(), record.Frequency };
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
        public ActionResult AddArticle(string media_name, string url, string author, string date_retrieved)
        {
            if (String.IsNullOrEmpty(url))
            {
                ViewBag.Message = "Ju lutem shtoeni adresen a artikullit!";                 
            }
            else if (String.IsNullOrEmpty(media_name))
            {
                ViewBag.Message = "Ju lutem shkruajeni emrin e mediumit!";
            }
            else
            {
                ViewBag.Message = "Artikulli i regjistrua me sukses!";
            }
            ArticleDBContext context = new ArticleDBContext();
            Article a = new Article();
            a.Author = author;
            a.DateRetrieved = Convert.ToDateTime(date_retrieved);
            a.MediaName = media_name;
            a.Url = url;
            context.Articles.Add(a);

            context.SaveChanges();

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
            
            //Te shekulli permbajtja eshte brenda klases tekst
            switch (media_name)
	        {
                case "Shekulli":
                    paHtml = doc.DocumentNode.SelectNodes("//div[@class='tekst']").First().InnerText;
                    break;
                case "Shqip":
                    paHtml = doc.DocumentNode.SelectNodes("//div[@class='td-ss-main-content']").First().InnerText;
                    break;
                case "Mapo":
                    paHtml = doc.GetElementbyId("content").InnerText;
                    break;
                case "Sot":
                    paHtml = doc.GetElementbyId("block-system-main").InnerText;
                    break;
		        default:
                break;
	        }


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
            char[] delim_fjalet = { '“', '”', ' ', '.', '!', '?', ';', '"', '}', '{', '(', ')', '-', '=', '+', '*', '"', ';', ':', '!', '?','<', '>' };

            foreach (string fjali in fjalite)
            {
                Phrase f = new Phrase();
                f.ArticleId = a.ArticleId;
                f.Text = fjali.ToLower();

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
                        wf = new WordFrequency();
                        wf.ArticleId = a.ArticleId;
                        wf.WordId = w.WordId;
                        wf.Frequency = 1;
                    }
                }
            }

            //regjistrojme ne db
           
            context.SaveChanges();

            return View("DocumentUploader");

        }
    }
}
