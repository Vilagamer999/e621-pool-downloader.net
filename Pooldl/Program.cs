using System;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace Pooldl
{
    internal class Program
    {

        static void Main(string[] args)
        {
            userInput();
        }

        private static void userInput()
        {

            Console.WriteLine("Please enter either:\n");
            Console.WriteLine("- Pool URL / ID (e.g. https://e621.net/pools/8804)\n");
            Console.WriteLine("- Pool ID (e.g. 7438) \n");
            Console.WriteLine("- \"lucky\" (Will download a random pool) \n");
            Console.Write("> ");
            string input = Console.ReadLine();

            //check if input is "lucky"
            if (input == "lucky")
            {
                //get random pool id
                Random rnd = new Random();
                int poolID = rnd.Next(1, 32651);
                Console.WriteLine("Downloading pool: " + poolID);
                getPool(poolID);

            }
            else
            {
                //check if input is a pool url
                if (input.Contains("https://e621.net/pools/"))
                {
                    //get pool id from url
                    string poolID = input.Substring(20);
                    getPool(Convert.ToInt32(poolID));
                }
                else if (input.Contains("e621.net/pools/"))
                {
                    //get pool id from url (short)
                    string poolID = input.Substring(12);
                    getPool(Convert.ToInt32(poolID));
                }
                else
                {
                    //input is a pool id
                    getPool(Convert.ToInt32(input));
                }
            }

        }

        private static void getPool(int poolID)
        {
            WebClient client = new WebClient();
            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            try
            {
                string json = client.DownloadString($"https://e621.net/pools/{poolID}.json");

                var pool = JsonSerializer.Deserialize<Pools>(json);

                string title = pool.name.Replace("_", " ");
                string creator = pool.creator_name.Replace("_", " ");

                Console.WriteLine($"Downloading: {title} by {creator}");

                downloadPool(pool.post_ids, pool);
            }
            catch (WebException)
            {
                Console.WriteLine("\nPool not found");
                Thread.Sleep(500);
                Console.Clear();
                userInput();
            }
        }

        private static void downloadPool(int[] post_ids, Pools pool)
        {
            var num = 1;
            WebClient client = new WebClient();
            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            int[] postIDs = new int[pool.post_ids.Length];

            for (int i = 0; i < pool.post_ids.Length; i++)
            {

                postIDs[i] = pool.post_ids[i];
                Console.WriteLine($"Downloading post {i + 1} of {pool.post_ids.Length}");

                client.Headers.Clear();
                client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

                string jsonFile = client.DownloadString($"https://e621.net/posts/{postIDs[i]}.json");

                var response = JsonSerializer.Deserialize<PostResponse>(jsonFile);

                //download file from url


                var FileUrl = response.post.file;

                Console.WriteLine(FileUrl.url);

                Console.WriteLine($"https://e621.net/posts.json?tags=id:{postIDs[i]}");

                client.DownloadFile($"{FileUrl.url}", $"{num}.{FileUrl.ext}");
                num++;

                Thread.Sleep(1000);

                ////skip if value is null
                //if (posts.url is null)
                //{
                //    Console.WriteLine("Skipping");
                //    continue;
                //}
                //else
                //{
                //    //download file, save in pool folder

                //}


            }

        }
    }
}
