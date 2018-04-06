using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IQVIA_API.Model;

namespace IQVIA_API
{
    // API Information :
    // https://badapi.iqvia.io/swagger/
    // Restrictions : 100 Items per call.

    class Program
    {
        static HttpClient client = new HttpClient();
        const string baseUrl = @"https://badapi.iqvia.io/api/v1/Tweets?";
        const string ReportFile = "tweetfeed.csv";

        static void Main(string[] args)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            RunAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Main Async Logic Loop
        /// </summary>
        /// <returns></returns>
        static async Task RunAsync()
        {
            // The heavy-lifting Task.
            Console.WriteLine("Commencing Tweet fetch operation.");

            List<TweetData> tweetblob = new List<TweetData>();
            bool fetchedAll = false;

            var tsStart = new DateTime(2016, 1, 1, 0, 0, 0).Ticks;
            var tsEnd = new DateTime(2017, 1, 1, 0, 0, 0).Ticks - 1;

            Console.WriteLine("Fetching Tweet batch.");
            while (!fetchedAll)
            {
                string urlPath = CreateUrl(baseUrl, tsStart, tsEnd);

                List<TweetData> tweet = await GetTweetsAsync(urlPath);

                if (tweet != null)
                {
                    tweetblob.AddRange(tweet);

                    // Check if we're done
                    if (tweet.Count == 100)
                    {
                        // Max results, set new base time for next iteration.
                        tsStart = tweet.OrderByDescending(t => t.Stamp).First().Timestamp.Ticks;
                        Console.WriteLine($"Batch full.  Starting next batch at {tsStart}");
                    }
                    else
                    {
                        // Done, Turn flag off.
                        fetchedAll = true;
                        Console.WriteLine($"Impartial Batch pulled.   Pull Complete.");
                    }
                }
                else
                {
                    // Abort due to an API call error.
                    fetchedAll = true;
                }
            }
            
            WritetReportAsync(tweetblob);
            Console.WriteLine($"Proccess Complete.  {tweetblob.Count} tweets pulled.");
        }

        /// <summary>
        /// Serializes the TweetData into a Comma-seperated report.
        /// </summary>
        /// <param name="tweetblob">List of TwitterData objects to serialize</param>
        private static async void WritetReportAsync(List<TweetData> tweetblob)
        {
            StreamWriter sw = new StreamWriter(ReportFile);
            foreach (var tt in tweetblob)
            {
                await sw.WriteLineAsync($"{tt.Stamp},{tt.Id},{tt.Text.Replace("\n", " ")}");
            }
            sw.Close();
        }

        /// <summary>
        /// Creates Fully qualified URL for API call based off the Base URL and Time Stamps passed in
        /// </summary>
        /// <param name="baseUrl">Full Base URL to the API endpoint</param>
        /// <param name="tsstart">Start time of the search, in ticks</param>
        /// <param name="tsend">End time of the search, in ticks</param>
        /// <returns></returns>
        private static string CreateUrl(string baseUrl, long tsstart, long tsend)
        {
            var ts = new DateTime(tsstart);
            var te = new DateTime(tsend);

            string tsStartString = WebUtility.UrlEncode(ts.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
            string tsEndString = WebUtility.UrlEncode(te.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));

            string urlpath = $"{baseUrl}startDate={tsStartString}&endDate={tsEndString}";
            return urlpath;
        }

        /// <summary>
        /// Makes an API call to get a list of tweets using a pre-defined query String
        /// </summary>
        /// <param name="apiUrl">The fully qualified URL to the API endpoint, complete with qurty parameters</param>
        /// <returns>Returns a list of TweetData objects</returns>
        static async Task<List<TweetData>> GetTweetsAsync(string apiUrl)
        {
            List<TweetData> TweetSheet = null;
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var respString = await response.Content.ReadAsStringAsync();

                    TweetSheet = JsonConvert.DeserializeObject<List<TweetData>>(respString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Encountered an issue : {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Encountered an issue : HTTP Status code : {(int)response.StatusCode}:{response.StatusCode}");
            }
            return TweetSheet;
        }
    }
}