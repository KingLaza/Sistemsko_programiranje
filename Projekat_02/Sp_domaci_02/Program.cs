using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Projekat_02
{
    //bigBoy - 4000 words
    //fatMan - 100 000 words
    class Program
    {
        public static readonly int clearCacheCount = 5;     //ako imas vremena ulespaj, novi fajl mozda


        public static async Task Main(string[] args)
        {
            string prefix = "http://localhost:5050/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            //listener.Start();
            try{
                listener.Start();
            }
            catch(Exception e){
                Console.Write($"Error starting listener: {e.Message}");
            }
            
            Console.WriteLine("Listening for requests on " + prefix);

            
            var cleanupThread = new Thread(ClearEventually);
            cleanupThread.IsBackground = true;
            cleanupThread.Start();

            async void ClearEventually(){
                while(true){
                    await Task.Delay(TimeSpan.FromMinutes(2));
                    Cache.ClearCache();
                }
            }
            

            while (true)
            {
                //ThreadPool.QueueUserWorkItem(ProcessRequest, listener.GetContext());
                var context = await listener.GetContextAsync();
                await Task.Run(() => ProcessRequest(context));
            }
        }

        static async Task ProcessRequest(object state)
        {
            if (state == null)
                return;
            try {
                Stopwatch stopwatch= Stopwatch.StartNew();
                HttpListenerContext context = (HttpListenerContext)state;
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string filename = "NOTHING";

                if (request.Url.Segments.Length <= 1)               //just tinker this a bit, maybe there are other annomalies
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    string responseString = "Filename is missing in the URL.";
                    Console.WriteLine(responseString);
                    byte[] buffer2 = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer2.Length;
                    response.OutputStream.Write(buffer2, 0, buffer2.Length);
                    response.OutputStream.Close();
                    return;
                }
                    

                if (request.Url.Segments.Length > 1)
                    filename = request.Url.Segments[1];
                string responseBody;
                responseBody = Cache.ReadFromCache(filename);
                if (responseBody == "")
                {
                    string current_folder = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
                    current_folder = Directory.GetParent(current_folder).FullName;
                    current_folder = Directory.GetParent(current_folder).FullName;
                    string filePath = FindFile(current_folder, filename);

                    if (filePath == null)
                    {
                        responseBody = $"File '{filename}' not found.";
                    }
                    else
                    {
                        string fileContent = await File.ReadAllTextAsync(filePath);
                        int wordCount = await CountWords(fileContent);
                        
                        responseBody = $"Word count for file '{filename}': {wordCount}";
                    }
                    Cache.WriteToCache(filename, responseBody);
                }

                stopwatch.Stop();

                StringBuilder sb = new StringBuilder();
                sb.Append(responseBody);
                double vreme = stopwatch.Elapsed.TotalMilliseconds;
                sb.Append("/Proteklo vreme: "+vreme.ToString()+"ms");

                //byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex) {
                Console.WriteLine("An error occured: " + ex.Message);
            }
        }

        static string FindFile(string directory, string filename)       //ovo nzm da li je potrebno da rade vise niti...
        {
            string[] files = Directory.GetFiles(directory, filename, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return files[0];
            }
            return null;
        }        


        static async Task<int> CountWords(string text) {
            int totalSum = 0;
            

            int textLength = text.Length;
            Console.WriteLine("Text length" + textLength);

            int numberOfTasks = textLength / 100000 + 1;


            Console.WriteLine("Number of tasks" + numberOfTasks);

            int chunk;
            chunk = textLength / numberOfTasks;

            Console.WriteLine("Chunk length" + chunk);

            Task<int>[] tasks = new Task<int>[numberOfTasks];

            for (int i = 0; i < numberOfTasks; i++)
            {
                int taskId = i + 1;
                int start = i * chunk;
                int end = (i + 1) * chunk;
                tasks[i] = Task.Run(() => CountWordsStartingWithCapital(text, start, end));
            }

            // Waiting for all the tasks to complete and summing their results
            int[] results = await Task.WhenAll(tasks);
            totalSum = results.Sum();

            Console.WriteLine("Total sum of results: " + totalSum);

            return totalSum;
        }

        static async Task<int> CountWordsStartingWithCapital(string text, int start, int end)           //ovo bi mozda moglo da se optimizuje da ga rade vise niti
        {
            int count = 0;
            for (int i = start; i < end && i < text.Length; i++) {
                int length = 0;
                if (!char.IsUpper(text[i]))
                    continue;
                while (i < end && text[i] != ' ' && text[i] != '\n' && text[i] != '\t'){
                    length++;
                    i++;
                }
                if (length > 5)
                    count++;
            }

            return count;
        }
    }
}
