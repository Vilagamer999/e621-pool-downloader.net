using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading;

namespace Pooldl
{
    internal class Program
    {
        //Create a WebClient object to make HTTP requests
        private static readonly WebClient client = new WebClient();

        static void Main(string[] args)
        {
            Directory.CreateDirectory($"downloaded");
            Directory.CreateDirectory($"log");
            Console.Title = "e621-pool-downloader.net";

            try
            {
                userInput();
            }
            catch (Exception ex)
            {
                //If an exception is thrown, log the error to a file and prompt the user to check the log folder for details
                File.WriteAllText($"log/{DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")}.log", ex.ToString());
                Console.WriteLine("An error has occured (probably invalid or blacklisted pool), please check the log folder.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                Main(args);
            }

        }

        //Prompt the user to input a pool URL or ID, or type 'lucky' to download a random pool
        private static void userInput()
        {
            printLogo();

            Console.WriteLine("Please enter either:\n");
            Console.WriteLine("- Pool URL / ID (e.g. https://e621.net/pools/8804)\n");
            Console.WriteLine("- Pool ID (e.g. 7438) \n");
            Console.WriteLine("- \"lucky\" (Will download a random pool) \n");
            Console.Write("> ");
            string input = Console.ReadLine().ToLower();

            //Check if the input is "lucky"
            if (input == "lucky" || input == "l")
            {
                //Keep generating random pool IDs until a valid one is found
                bool isValidPoolID = false;
                Random rnd = new Random();
                int poolID = 0;

                while (!isValidPoolID)
                {
                    poolID = rnd.Next(1, 50000);
                    isValidPoolID = checkIfValid(poolID);
                }

                Console.WriteLine("\nDownloading pool: " + poolID);

                //Download the pool with the specified ID
                getPool(poolID);
            }
            else
            {
                if (input.Contains("https://e621.net/pools/") || input.Contains("e621.net/pools/"))
                {
                    string poolID = input.Replace("https://e621.net/pools/", "").Replace("e621.net/pools/", "");
                    try
                    {
                        getPool(Convert.ToInt32(poolID));
                    }
                    catch (Exception ex)
                    {
                        HandleInvalidInput();
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        getPool(Convert.ToInt32(input));
                    }
                    catch (Exception ex)
                    {
                        HandleInvalidInput();
                        throw;
                    }
                }
            }

            void HandleInvalidInput()
            {
                Console.WriteLine("\nInvalid input, try again...");
                Thread.Sleep(1000);
                Console.Clear();
                userInput();
            }


        }

        //Remove all invalid characters from the file name so that windows doesn't throw a fit
        private static string removeInvalidChars(string name)
        {
            return Regex.Replace(name, @"[:?_'*\\]", " ");
        }


        //Check if the given lucky pool id is valid
        private static bool checkIfValid(int poolID)
        {
            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            try
            {
                //Try to download the pool json, return true if it exists
                string json = client.DownloadString($"https://e621.net/pools/{poolID}.json");
                JsonSerializer.Deserialize<Pools>(json);

                return true;
            }
            catch (WebException)
            {
                //Print an error message if the pool does not exist or is invalid and return false
                Console.WriteLine("Random number invalid, retrying...");
                Thread.Sleep(1000);

                return false;
            }
        }

        //Download the given pool data from the e621.net API
        private static void getPool(int poolID)
        {
            //Clear the client headers and set a new special user-agent header
            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            try
            {
                //Download the pool data as a JSON string
                string json = client.DownloadString($"https://e621.net/pools/{poolID}.json");

                //Deserialize the JSON string into a Pools object
                var pool = JsonSerializer.Deserialize<Pools>(json);

                Console.WriteLine($"Downloading: {removeInvalidChars(pool.name)} by {removeInvalidChars(pool.creator_name)}");

                downloadPool(pool.post_ids, pool);
            }
            catch (WebException)
            {
                //Write an error message to the console if pool is not found
                Console.WriteLine("\nPool not found...");
                Thread.Sleep(1000);
                Console.Clear();
                userInput();
            }

        }

        //Download the individual images one by one using the pool data above
        private static void downloadPool(int[] post_ids, Pools pool)
        {

            client.Headers.Clear();
            client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

            Directory.CreateDirectory($"downloaded/{removeInvalidChars(pool.name)}");

            //Create an array to store the post IDs from the pool
            var postIDs = new int[pool.post_ids.Length];

            //Loop through each post ID in the pool
            for (int i = 0; i < pool.post_ids.Length; i++)
            {
                client.Headers.Clear();
                client.Headers.Add("user-agent", "PoolDownloaderNET/0.01 (by NotVila on e621)");

                postIDs[i] = pool.post_ids[i];
                Console.WriteLine($"\nDownloading post {i + 1} of {pool.post_ids.Length}");

                //Download the post data as a JSON string
                string jsonFile = client.DownloadString($"https://e621.net/posts/{postIDs[i]}.json");

                //Deserializethe the JSON string into a PostResponse object and get the file URL
                var FileUrl = JsonSerializer.Deserialize<PostResponse>(jsonFile).post.file;

                Console.WriteLine(FileUrl.url);

                try
                {
                    //Download the file from the file URL and save it in the downloaded directory
                    client.DownloadFile($"{FileUrl.url}", $"downloaded/{removeInvalidChars(pool.name)}/{i + 1}.{FileUrl.ext}");

                    Thread.Sleep(900);
                }
                catch (Exception)
                {
                    //Write a skipping message to the console and continue with the next post ID if download fails
                    Console.WriteLine("Skipping");
                    continue;

                }
            }

            // Write a message to the console to show the downloaded files directory path
            Console.WriteLine($"\nSaved to: pooldl/downloaded/{removeInvalidChars(pool.name)}");
            Console.Write("\nFinished! Press enter to download another pool...");
            Console.ReadKey();
            Console.Clear();
            userInput();

        }

        private static void printLogo()
        {
            //ASCII art by "jgs" on https://www.asciiart.eu/
            //(Modified by yours truly)

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

            //Where did the cool ascii logos go? ;-;
        }
    }
}