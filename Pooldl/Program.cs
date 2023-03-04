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
            Directory.CreateDirectory($"downloaded");
            Directory.CreateDirectory($"log");
            printLogo();
            
            try
            {
                userInput();
            }
            catch (Exception ex)
            {
                File.WriteAllText($"log/{DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}.log", ex.ToString());
                Console.WriteLine("An error has occured (probably invalid or blacklisted pool), please check the log folder.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                userInput();
            }

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
                int poolID = rnd.Next(1, 32651);


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
                    try
                    {
                        //get pool id from url
                        string poolID = input.Substring(23);
                        getPool(Convert.ToInt32(poolID));
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is OverflowException)
                        {
                            Console.WriteLine("\nInvalid input, try again...");
                            Thread.Sleep(1000);
                            Console.Clear();
                            userInput();
                        }

                        throw;
                    }

                }
                else if (input.Contains("e621.net/pools/"))
                {
                    //get pool id from url(short)
                    string poolID = input.Substring(15);

                    try
                    {
                        getPool(Convert.ToInt32(poolID));
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is OverflowException)
                        {
                            Console.WriteLine("\nInvalid input, try again...");
                            Thread.Sleep(1000);
                            Console.Clear();
                            userInput();
                        }

                        throw;
                    }
                }
                else
                {
                    try
                    {
                        //input is a pool id
                        getPool(Convert.ToInt32(input));
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is OverflowException)
                        {
                            Console.WriteLine("\nInvalid input, try again...");
                            Thread.Sleep(1000);
                            Console.Clear();
                            userInput();
                        }

                        throw;
                    }

                }
            }

        }

        private static object removeInvalidChars(string name)
        {
            //jankman says hi
            return name.Replace(":", "").Replace("?", "").Replace("_", " ").Replace("'", "").Replace("*", " ");
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

                Console.WriteLine($"Downloading: {removeInvalidChars(pool.name)} by {removeInvalidChars(pool.creator_name)}");

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

            Directory.CreateDirectory($"downloaded/{removeInvalidChars(pool.name)}");

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
                    client.DownloadFile($"{FileUrl.url}", $"downloaded/{removeInvalidChars(pool.name)}/{num}.{FileUrl.ext}");

                    Thread.Sleep(900);
                    num++;
                }
                catch (WebException)
                {

                    Console.WriteLine("Skipping");
                    continue;

                }
            }

            Console.WriteLine($"\nSaved to: pooldl/downloaded/{removeInvalidChars(pool.name)}");
            Console.Write("\nFinished! Press enter to download another pool...");
            Console.ReadKey();
            Console.Clear();
            userInput();

        }

        private static void printLogo()
        {
            //ASCII art by "jgs" on https://www.asciiart.eu/
            
            Console.Title = "e621-pool-downloader.net";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("===========================================>  \n   _ |ooooooooooooooooooooooooooooooooo|\n  |1\\|~~ _- ~~ --~ ~ ~~-_/\\O_~~ ~~__~~ |\n  |_||~_~ [");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("e621");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("]--~`~  ~_ ~_~~ ~ ~~~_~ |\n   _ |ooooooooooooooooooooooooooooooooo|\n  |2\\|~~ ~-- ~~~ _~~~/\\O ~~~ ~~ ~~ ^^- |\n  |_||  ~~~-- ~~ [");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("pool");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("]~~ ~~ ~^^ ^-- -~|\n   _ |ooooooooooooooooooooooooooooooooo|\n  |3\\|-- ~~~ ^~ -~~~  ^~~ O/\\ ~~--~ _~ |\n  |_|| ~~ ~^~  ^~- ~~`` [");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("downloader");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("]^^~|\n     |ooooooooooooooooooooooooooooooooo|\n=================================[");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("net");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("]=====>\n\n");
            
            //where did the cool ascii logos go? ;-;
        }
    }
}