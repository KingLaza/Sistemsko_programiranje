using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using TreciLab;

namespace WordCountWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string prefix = "http://localhost:5050/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine("Listening for requests on " + prefix);

            while (true)
            {
                ThreadPool.QueueUserWorkItem(ProcessRequest, listener.GetContext());
            }
        }

        static void ProcessRequest(object state)
        {
            Stopwatch stopwatch= Stopwatch.StartNew();
            HttpListenerContext context = (HttpListenerContext)state;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string filename = request.Url.Segments[1];
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
                    string fileContent = File.ReadAllText(filePath);
                    int wordCount = CountWordsStartingWithCapital(fileContent);
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

        static string FindFile(string directory, string filename)       //ovo nzm da li je potrebno da rade vise niti...
        {
            string[] files = Directory.GetFiles(directory, filename, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return files[0];
            }
            return null;
        }

        static int CountWordsStartingWithCapital(string text)           //ovo bi mozda moglo da se optimizuje da ga rade vise niti
        {
            int count = 0;
            string[] words = text.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                if (char.IsUpper(word[0]) && word.Length > 5)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
