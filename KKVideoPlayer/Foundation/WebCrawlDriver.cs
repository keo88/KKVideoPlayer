namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Brotli;
    using HtmlAgilityPack;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    ///  Crawl information of the video from the web.
    /// </summary>
    public class WebCrawlDriver : IDisposable
    {
        private const string JAVLIBURL = "https://www.javlibrary.com/en/";
        private const string JAVDBURL = "https://javdb.com/";
        private const string JAVBUSURL = "http://javbus.com/ko/";
        private readonly HttpClient client;
        private readonly HttpClientHandler handler;
        private ChromeDriverService driverService = null;
        private ChromeOptions options = null;
        private ChromeDriver driver;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCrawlDriver"/> class.
        /// </summary>
        public WebCrawlDriver()
        {
            try
            {
                driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;

                options = new ChromeOptions();
                options.AddArgument("disable-gpu");

                // options.AddArgument("headless");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }

            handler = new();
            handler.AllowAutoRedirect = true;
            client = new HttpClient(handler);

            // client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36");
            client.DefaultRequestHeaders.Add("cache-control", "max-age=0");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\\\"Google Chrome\\\";v=\\\"95\\\", \\\"Chromium\\\";v=\\\"95\\\", \\\";Not A Brand\\\";v=\\\"99\\\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\\\"Windows\\\"");
            client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
            client.DefaultRequestHeaders.Add("accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "none");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
            client.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
            client.DefaultRequestHeaders.Add("accept-encoding", "gzip,deflate,br");
            client.DefaultRequestHeaders.Add("accept-language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7,ja;q=0.6");
        }

        /// <summary>
        ///  Navigtate to Javlib, then try to search for dvdid.
        /// </summary>
        /// <param name="dvdid">Dvd Id to search.</param>
        /// <param name="v">Video to modify.</param>
        /// <returns>Video entry is returned.</returns>
        public bool NavigateJavlib(string dvdid, VideoEntry v)
        {
            try
            {
                driver = new ChromeDriver(driverService, options);

                driver.Navigate().GoToUrl(JAVLIBURL);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // Find adult warning message. If exists, click accept button.
                List<IWebElement> find_results = new List<IWebElement>();
                find_results.AddRange(driver.FindElements(By.XPath("//*[@id='adultwarningprompt']/p/input[1]")));

                if (find_results.Count > 0)
                {
                    find_results[0].Click();
                }

                IWebElement searchBox = driver.FindElement(By.XPath("//*[@id='idsearchbox']"));
                searchBox.SendKeys(dvdid);
                IWebElement element = driver.FindElement(By.XPath("//*[@id='idsearchbutton']"));
                element.Click();

                // Check if the search returns multiple videos. If it does, return the first.
                find_results = new List<IWebElement>();
                find_results.AddRange(driver.FindElements(By.ClassName("video")));

                if (find_results.Count > 0)
                {
                    find_results[0].Click();
                }

                find_results = new List<IWebElement>();
                find_results.AddRange(driver.FindElements(By.XPath("//*[@id='video_jacket_info']")));

                if (find_results.Count > 0)
                {
                    IWebElement table = find_results[0];

                    IWebElement date_elem = table.FindElement(By.XPath("//*[@id='video_date']/table/tbody/tr/td[2]"));
                    v.ReleaseDate = DateTime.ParseExact(date_elem.Text, "yyyy-MM-dd", null);

                    IWebElement director_div = table.FindElement(By.XPath("//*[@id='video_director']"));
                    List<IWebElement> director_elems = new();
                    director_elems.AddRange(director_div.FindElements(By.TagName("a")));
                    director_elems.ForEach((IWebElement ele) => v.Directors.Add(ele.Text));

                    IWebElement maker_div = table.FindElement(By.XPath("//*[@id='video_maker']"));
                    List<IWebElement> maker_elems = new();
                    maker_elems.AddRange(maker_div.FindElements(By.TagName("a")));
                    maker_elems.ForEach((IWebElement ele) => v.Companies.Add(ele.Text));

                    IWebElement label_elem = table.FindElement(By.XPath("//*[@id='video_label']/table/tbody/tr/td[2]"));
                    v.Series = label_elem.Text;

                    IWebElement genre_div = table.FindElement(By.XPath("//*[@id='video_genres']"));
                    List<IWebElement> genre_elems = new();
                    genre_elems.AddRange(genre_div.FindElements(By.TagName("a")));
                    genre_elems.ForEach((IWebElement ele) => v.Genres.Add(ele.Text));

                    IWebElement cast_div = table.FindElement(By.XPath("//*[@id='video_cast']"));
                    List<IWebElement> cast_elems = new();
                    cast_elems.AddRange(cast_div.FindElements(By.TagName("a")));
                    cast_elems.ForEach((IWebElement ele) => v.Actors.Add(ele.Text));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Navigtate to Javlib, then try to search for dvdid.
        /// </summary>
        /// <param name="dvdid">Dvd Id to search.</param>
        /// <param name="v">Video to modify.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> NavigateJavDb(string dvdid, VideoEntry v)
        {
            if (string.IsNullOrWhiteSpace(dvdid))
            {
                return false;
            }

            string dvdid_lower = dvdid.ToLower();
            string dvdid_upper = dvdid.ToUpper();

            string db_url = $"{JAVDBURL}search?q={dvdid_lower}&f=all/";
            string bus_url = $"{JAVBUSURL}{dvdid_upper}/";

            HttpRequestMessage requestUrl = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(bus_url),
            };

            HttpResponseMessage responseUrl = await client.SendAsync(requestUrl, HttpCompletionOption.ResponseContentRead);

            if (responseUrl.RequestMessage.RequestUri.ToString().Contains("http://warning.or.kr") ||
                responseUrl.StatusCode != System.Net.HttpStatusCode.MovedPermanently)
                return false;

            HttpRequestMessage requestRedirect = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = responseUrl.Headers.Location,
            };

            HttpResponseMessage responseRedirect = await client.SendAsync(requestRedirect, HttpCompletionOption.ResponseContentRead);
            if (responseRedirect.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            HttpContent html = responseRedirect.Content;
            string contents;

            using (BrotliStream bs = new BrotliStream(await html.ReadAsStreamAsync(), System.IO.Compression.CompressionMode.Decompress))
            {
                using (MemoryStream ms = new())
                {
                    bs.CopyTo(ms);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        contents = reader.ReadToEnd();
                    }
                }
            }

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contents);
            HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body");
            HtmlNode movieNode = bodyNode.SelectSingleNode("/html/body/div[5]/div[1]/div[2]");
            HtmlNode titleNode = bodyNode.SelectSingleNode("/html/body/div[5]/h3");

            v.Title = titleNode.InnerText;

            string rawText = movieNode.InnerText.Replace("\t", string.Empty).Replace("\r\n", string.Empty).Trim();

            JavBusBreakVideoInfo(v, rawText);

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Dispose the entity.
        /// </summary>
        /// <param name="disposing">.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                driver.Dispose();
                driverService.Dispose();
                client.Dispose();
                handler.Dispose();
            }
        }

        private void JavBusBreakVideoInfo(VideoEntry v, string rawText)
        {
            Match searchDate = Regex.Match(rawText, @"출시일:\s\d{4}-\d{2}-\d{2}");
            if (searchDate.Success)
            {
                DateTime releaseDate = DateTime.Parse(searchDate.Value.Split(":")[1]);
                v.ReleaseDate = releaseDate;
            }

            Match searchDirector = Regex.Match(rawText, @"관리자:.*?(?=\s{2})");
            if (searchDirector.Success)
            {
                List<string> directors = new();
                directors.Add(searchDirector.Value.Trim().Split(":")[1]);
                v.Directors = directors;
            }

            Match searchMaker = Regex.Match(rawText, @"메이커:.*?(?=\s{2})");
            if (searchMaker.Success)
            {
                List<string> makers = new();
                makers.Add(searchMaker.Value.Trim().Split(":")[1]);
                v.Companies = makers;
            }

            Match searchLabel = Regex.Match(rawText, @"라벨:.*?(?=\s{2})");
            if (searchLabel.Success)
            {
                v.Series = searchLabel.Value.Trim();
            }

            Match searchSeries = Regex.Match(rawText, @"시리즈:.*?(?=\s{2})");
            if (searchSeries.Success)
            {
                v.Series = searchSeries.Value.Trim();
            }

            if (rawText.Contains("장르:"))
            {
                string rawGenres = rawText.Split("장르:")[1].Split("별:")[0];
                MatchCollection searchGenres = Regex.Matches(rawGenres, @"\S.*?(?=\s{2})");
                SortedSet<string> genres = new();

                foreach (Match genreMatch in searchGenres)
                {
                    genres.Add(genreMatch.Value);
                }

                v.Genres = genres.ToList();
            }

            if (rawText.Contains("별:"))
            {
                string rawActors = rawText.Split("별:")[1];
                MatchCollection searchActors = Regex.Matches(rawActors, @"\S.*?(?=\s{2})");
                SortedSet<string> actors = new();

                foreach (Match actorMatch in searchActors)
                {
                    actors.Add(actorMatch.Value);
                }

                v.Actors = actors.ToList();
            }
        }
    }
}
