using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Projekat3.Models;
using Projekat3.Services;

namespace Projekat3.Server
{
    public class WeatherServer
    {
        private const int Port = 10889;

        private readonly string[] _prefixes = {
            $"http://127.0.0.1:{Port}/"
        };

        private readonly HttpListener _listener = new();
        private readonly WeatherService _weatherService;
        private IDisposable? _subscription;

        public WeatherServer(WeatherService weatherService)
        {
            foreach (var prefix in _prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _weatherService = weatherService;
            _listener.Start();
            Console.WriteLine($"Listening at: \n{String.Join("\n", _listener.Prefixes)}");
        }

        public void Init()
        {
            if (_subscription != null)
                return;

            _subscription = GetRequestStream().Distinct().Subscribe(
                onNext: (context) => {
                    if (context != null)
                        DealWithRequest(context);
                    else
                        Console.WriteLine("Empty context");
                },
                onError: (err) => Console.WriteLine($"Server error due to: {err.Message}"));
        }

        private void DealWithRequest(HttpListenerContext? context)
        {
            if (context == null)
                return;

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");

            if (request.HttpMethod == "GET")        //input
            {
                SendResponse(ShowResponse.ShowResponsePage(), response);
            }
            else if (request.HttpMethod == "POST")      //process
            {
                string location = null;
                List<string> errorDetails = new List<string>();

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    var formData = reader.ReadToEnd();
                    var parsedData = System.Web.HttpUtility.ParseQueryString(formData);
                    string latitudeStr = parsedData["latitude"];
                    string longitudeStr = parsedData["longitude"];

                    if (!string.IsNullOrWhiteSpace(latitudeStr) && !string.IsNullOrWhiteSpace(longitudeStr))
                    {
                        if (double.TryParse(latitudeStr, out double latitude) && double.TryParse(longitudeStr, out double longitude))
                        {
                            if (latitude < -90 || latitude > 90)
                            {
                                errorDetails.Add("Latitude must be between -90 and 90.");
                            }
                            if (longitude < -180 || longitude > 180)
                            {
                                errorDetails.Add("Longitude must be between -180 and 180.");
                            }
                            if (errorDetails.Count == 0)
                            {
                                location = $"{latitude},{longitude}";
                            }
                        }
                        else
                        {
                            errorDetails.Add("Both latitude and longitude must be valid numbers.");
                        }
                    }
                    else
                    {
                        errorDetails.Add("Input both latitude and longitude.");
                    }
                }

                if (errorDetails.Count > 0)
                {
                    SendResponse(ShowResponse.ShowResponsePage(errorMessage: "Invalid input.", errorDetails: errorDetails.ToArray()), response);
                    return;
                }

                _weatherService.GetWeatherInfo(new HashSet<string> { location }).Subscribe(
                    info =>
                    {
                        if (info != null)
                        {
                            SendResponse(ShowResponse.ShowResponsePage(weatherInfo: info), response);
                        }
                    },
                    exception =>
                    {
                        SendResponse(ShowResponse.ShowResponsePage(errorMessage: "Error processing location.", errorDetails: new[] { exception.Message }), response);
                    });
            }
        }

        private void SendResponse(byte[] buffer, HttpListenerResponse response)
        {
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private IObservable<HttpListenerContext?> GetRequestStream()
        {
            return Observable.Create<HttpListenerContext?>(async (observer) =>
            {
                while (true)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        observer.OnNext(context);
                    }
                    catch (HttpListenerException ec)
                    {
                        observer.OnError(ec);
                        return;
                    }
                    catch (Exception)
                    {
                        observer.OnNext(null);
                    }
                }
            }).ObserveOn(TaskPoolScheduler.Default);
        }
    }
}
