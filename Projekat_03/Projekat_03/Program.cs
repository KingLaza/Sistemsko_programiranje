using System.Timers;
using System.Diagnostics;
using Projekat3.Services;
using Timer = System.Timers.Timer;

namespace Projekat3
{
    class Program
    {
        static void Main(string[] args)
        {
            WeatherService weatherService = new WeatherService();
            
            Server.WeatherServer webServer = new(weatherService);
            webServer.Init();
            
            Console.ReadKey();
            
        }
    }
}
