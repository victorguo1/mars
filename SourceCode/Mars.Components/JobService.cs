﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mars.Components
{
    public class JobService
    {
        static public List<Job> GetJobs(string keywords, string where = "toronto")
        {
            string link = $"https://www.indeed.ca/jobs?q={keywords}&l={where}";
            List<Job> jobList = new List<Job>();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(link).Result;
                string html = response.Content.ReadAsStringAsync().Result;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Any(o => o.Name == "data-tn-component") && d.Attributes["data-tn-component"].Value.Contains("organicJob"));

                foreach (HtmlNode node in nodes)
                {
                    string title = GetTextFromNode(node, "h2/a");
                    string company = GetTextFromNode(node, "span[@class='company']");
                    string location = GetTextFromNode(node, "span[@class='location']");
                    string summary = GetTextFromNode(node, "table/tr/td/div/span[@class='summary']");
                    string url = node.SelectSingleNode("h2/a").Attributes["href"].Value;

                    jobList.Add(new Job(title, company, location, summary, url));
                }
            }

            return jobList;
        }

        static private string TrimAndRemoveNewLine(string text)
        {
            return Regex.Replace(text.Trim(), @"\t|\n|\r", "");
        }

        static private string GetTextFromNode(HtmlNode node, string xPath)
        {
            return TrimAndRemoveNewLine(node.SelectSingleNode(xPath).InnerText);
        }
    }
}