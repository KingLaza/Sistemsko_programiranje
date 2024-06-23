using System.IO;
using System.Text;
using Projekat3.Models;

namespace Projekat3.Server
{
    public static class ShowResponse
    {
        public static byte[] ShowResponsePage(WeatherInfo? weatherInfo = null, string? errorMessage = null, string[]? errorDetails = null)
        {
            string htmlPage = File.ReadAllText("../../../Frontend/index.html");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                htmlPage = htmlPage.Replace("{{errorMessage}}", errorMessage);
                htmlPage = htmlPage.Replace("{{errorDetails}}", SerializeToJson(errorDetails));
            }
            else
            {
                htmlPage = htmlPage.Replace("{{errorMessage}}", "");
                htmlPage = htmlPage.Replace("{{errorDetails}}", "[]");
            }

            if (weatherInfo != null)
            {
                htmlPage = htmlPage.Replace("{{weatherInfo}}", SerializeToJson(weatherInfo));
            }
            else
            {
                htmlPage = htmlPage.Replace("{{weatherInfo}}", "{}");
            }

            return Encoding.UTF8.GetBytes(htmlPage);
        }

        private static string SerializeToJson(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}
