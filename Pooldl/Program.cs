using System;
using System.IO;
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
            string input = Console.ReadLine().ToLower();

            //check if input is "lucky"
            if (input == "lucky" || input == "l")
            {

            This_makes_csharp_developers_cry:

                Random rnd = new Random();
                int poolID = rnd.Next(20000, 99999);//32651);


                if (checkIfValid(poolID))
                {
                    Console.WriteLine("\nDownloading pool: " + poolID);

                    getPool(poolID);

                }
                else if (checkIfValid(poolID) == false)
                {
                    goto This_makes_csharp_developers_cry;
                }

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

        private static bool checkIfValid(int poolID)
        {
            WebClient client = new WebClient();
            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            try
            {
                string json = client.DownloadString($"https://e621.net/pools/{poolID}.json");
                JsonSerializer.Deserialize<Pools>(json);

                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Random number invalid, retrying...");
                Thread.Sleep(1000);
                return false;
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
                Thread.Sleep(3000);
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

            Directory.CreateDirectory($"{pool.name}");

            int[] postIDs = new int[pool.post_ids.Length];

            for (int i = 0; i < pool.post_ids.Length; i++)
            {
                client.Headers.Clear();
                client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

                postIDs[i] = pool.post_ids[i];
                Console.WriteLine($"\nDownloading post {i + 1} of {pool.post_ids.Length}");

                string jsonFile = client.DownloadString($"https://e621.net/posts/{postIDs[i]}.json");
                var FileUrl = JsonSerializer.Deserialize<PostResponse>(jsonFile).post.file;

                Console.WriteLine(FileUrl.url);

                try
                {
                    client.DownloadFile($"{FileUrl.url}", $"{pool.name}/{num}.{FileUrl.ext}");

                    Thread.Sleep(900);
                    num++;
                }
                catch (WebException)
                {

                    Console.WriteLine("Skipping");
                    continue;

                }
            }

            Console.WriteLine("\nFinished! Press enter to download another pool...");
            Console.ReadLine();
            Console.Clear();
            userInput();

        }
    }
}