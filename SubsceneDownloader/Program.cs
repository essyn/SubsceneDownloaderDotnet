using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace fds
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var exists = File.Exists("links.txt");

            if (!exists)
            {
                Console.WriteLine("Please create a links.txt file");
                Console.ReadKey();
            }
            else
            {
                var links = System.IO.File.ReadAllLines("links.txt");

                foreach (var item in links)
                {
                    try
                    {
                        GetItem(item);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            //GetItem();
        }
        private static void GetItem(string url)
        {
            try
            {
                var aa = CallUrl(url).Result;
                var n = GetLink(aa);
                var m = GetName(aa).Trim().Replace(" ", "_");
                DownloadItem(n, m);
                Console.WriteLine(n + "downloaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("couldnt download");
            }

        }
        private static string GetName(string html)
        {
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);
            var name = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='name']");
            if (name != null)
            {
                var itemname = name.InnerHtml;
                return itemname;
            }
            return "";
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "other");
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }
        private static string GetLink(string html)
        {
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);
            var btnEl = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='downloadButton']");
            if (btnEl != null)
            {
                var downloadUrl = btnEl.GetAttributeValue("href", string.Empty);
                return downloadUrl;
            }
            return "";
        }
        private static void DownloadItem(string url, string folderName)
        {
            HttpClient wc = new();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            wc.DefaultRequestHeaders.Accept.Clear();
            wc.DefaultRequestHeaders.Add("User-Agent", "other");
            var adr = new Uri("https://subscene.com" + url);
            var ds = wc.GetAsync(adr).Result;
            var item = ds.Content.ReadAsByteArrayAsync().Result;
            var fileName = ds.Content.Headers.GetValues("Content-Disposition").First();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "downloads", folderName);
            Directory.CreateDirectory(path);
            File.WriteAllBytes(Path.Combine(path, fileName.Trim()), item);
            ZipFile.ExtractToDirectory(Path.Combine(path, fileName.Trim()), path, true);
            File.Delete(Path.Combine(path, fileName.Trim()));
        }
    }
}




