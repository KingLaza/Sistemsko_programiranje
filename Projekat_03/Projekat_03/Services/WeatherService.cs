using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using Projekat3.Models;

namespace Projekat3.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _baseUrl = "https://api.open-meteo.com/v1/forecast";

        public IObservable<WeatherInfo> GetWeatherInfo(HashSet<string> locations)
        {
            var observables = locations.Select(location =>
                Observable.FromAsync(() => FetchWeatherDataAsync(location)));
            return observables.Merge().SubscribeOn(TaskPoolScheduler.Default).ObserveOn(TaskPoolScheduler.Default);
        }

        private async Task<WeatherInfo> FetchWeatherDataAsync(string location)
        {
            var coords = location.Split(',');
            //Console.WriteLine(location);
            if (coords.Length != 2)
            {
                throw new ArgumentException("Location must be in the format 'latitude,longitude'");
            }

            string latitude = coords[0];
            string longitude = coords[1];
            string url = $"{_baseUrl}?latitude={latitude}&longitude={longitude}&hourly=relative_humidity_2m,uv_index,visibility";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseBody);

            var hourlyData = jsonResponse["hourly"];
            if (hourlyData == null) return null;

            var weatherInfo = new WeatherInfo
            {
                Location = location,
                AverageHumidity = hourlyData["relative_humidity_2m"].Average(x => (double)x),
                MinHumidity = hourlyData["relative_humidity_2m"].Min(x => (double)x),
                MaxHumidity = hourlyData["relative_humidity_2m"].Max(x => (double)x),
                AverageVisibility = hourlyData["visibility"].Average(x => (double)x),
                MinVisibility = hourlyData["visibility"].Min(x => (double)x),
                MaxVisibility = hourlyData["visibility"].Max(x => (double)x),
                AverageUVIndex = hourlyData["uv_index"].Average(x => (double)x)
            };

            return weatherInfo;
        }


    }
}
