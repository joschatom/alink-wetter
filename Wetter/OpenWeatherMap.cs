using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wetter.Reflection;
using static Wetter.OpenWeatherMap;

namespace Wetter
{

    [Serializable]
    public class WeatherInfo : IReflectable
    {
#pragma warning disable IDE1006 // Naming Styles

        public struct Weather : IReflectable
        {
            public uint id;
            public string main;
            public string description { get; set; }
            public string icon;

        }



        public struct Main : IReflectable
        {

            public double Temp { get; set; }
            public double feels_like { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
            public double pressure { get; set; }
            public double humidity { get; set; }
            public double sea_level { get; set; }
            public double grnd_level { get; set; }

        }

        public struct Rain : IReflectable
        {
            [JsonProperty("1h")]
            public double _1h { get; set; }

        }
        public struct Clouds : IReflectable
        {
            public int all { get; set; }

        }
        public struct Wind : IReflectable
        {
            public double Speed { get; set; }
            public double Deg { get; set; }
            public double Gust { get; set; }

        }
        public struct Sys : IReflectable
        {
            public int type;
            public int id;
            public string country { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime sunrise { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime sunset { get; set; }
        }


        public Coordinates coord { get; set; }
        public Main main { get; set; }
        public Rain rain { get; set; }
        public Clouds clouds { get; set; }
        public Wind wind { get; set; }
        public Sys sys { get; set; }
        [JsonProperty("base")]
        public required string baseKey { get; set; }
        public required List<Weather> weather { get; set; }
        public required int visibility { get; set; }
        [JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public required DateTime dt { get; set; }
        public required int timezone { get; set; }
        public required int id;
        public required string name { get; set; }
        public required int cod;


        public string UTCTimezone => timezone
            < 0 ? $"UTC-{Math.Abs(TimeSpan.FromSeconds(timezone).Hours)}"
            : $"UTC+{TimeSpan.FromSeconds(timezone).Hours}";

        public TimeOnly LastUpdated => TimeOnly.FromDateTime(Localize(dt));

        public TimeOnly LocalSunrise => TimeOnly.FromDateTime(Localize(sys.sunrise));
        public TimeOnly LocalSunset => TimeOnly.FromDateTime(Localize(sys.sunset));

        public DateTime Localize(DateTime date) => date.AddSeconds(timezone);

        public BitmapImage Icon => new BitmapImage(new($"https://openweathermap.org/img/wn/{weather[0].icon}@4x.png"),
            new());
    }

    public class OpenWeatherMap
    {

        private const string APP_ID = "cb3c0e2b0a686064660539730a67c741";
        private HttpClient Client = new();

        [Serializable]
        public struct Coordinates: IReflectable {
            public double lat, lon;

            public override readonly string ToString()
            {
                return $"{lat}, {lon}";
            }
        }

        public OpenWeatherMap()
        {
            
        }

        [Serializable]
        public class GeoLocation: IReflectable
        {
            public string? zip;
            public string name;
            [JsonProperty("lat")] double _latRaw;
            [JsonProperty("lon")] double _lonRaw;
            [Newtonsoft.Json.JsonIgnore]
            public Coordinates coords { get => new Coordinates { lat = _latRaw, lon = _lonRaw }; }

            public string country;
            public string? state;
        }




        internal async Task<T?> GetAsJson<T>(string url)
            => await Client.GetStringAsync($"{url}&appid={APP_ID}")
                .ContinueWith((r) => JsonConvert.DeserializeObject<T>(r.Result));
        public async Task<GeoLocation?> GetLocationByZip(string zip, string country)
            => await GetAsJson<GeoLocation>($"http://api.openweathermap.org/geo/1.0/zip?zip={zip},{country}");
        public async Task<GeoLocation[]?> GetLocationByQuery(string query)
            => await GetAsJson<GeoLocation[]>($"http://api.openweathermap.org/geo/1.0/direct?q={query}&limit=5");
        public async Task<WeatherInfo?> GetCurrentWeatherByCoords(Coordinates coords)
          => await GetAsJson<WeatherInfo>($"https://api.openweathermap.org/data/2.5/weather?lat={coords.lat}&lon={coords.lon}&units=metric");
    }
}
