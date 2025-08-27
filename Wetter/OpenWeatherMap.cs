using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Wetter.Reflection;
using static Wetter.OpenWeatherMap;

namespace Wetter
{
    [Serializable]
    public struct Coordinates : IReflectable
    {
        public double lat, lon;

        public override readonly string ToString()
        {
            return $"{lat}, {lon}";
        }
    }


    public class City: IReflectable
    {
        public int id { get; set; }
        public required string name { get; set; }
        public Coordinates coord { get; set; }
        public required string country { get; set; }
        public int? population { get; set; }
        public int timezone { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public DateTime sunrise { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public DateTime sunset { get; set; }
    }



    public class ForecastSpanData: IReflectable
    {
        public Main main { get; set; }
        public Rain rain { get; set; }
        public Clouds clouds { get; set; }
        public Wind wind { get; set; }
        public object sys { get; set; }
        public required List<Weather> weather { get; set; }
        public required int visibility { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public required DateTime dt { get; set; }
        public required double pop { get; set; }
    }

    public struct ForecastInfo : IReflectable
    {

        public string cod { get; set; }
        public int message { get; set; }
        public int cnt { get; set; }
        [JsonProperty("list")]
        public List<ForecastSpanData> Data { get; set; }
        public City city { get; set; }

      
    }

    public class WeatherForecast: DataSource
    {
        internal int Selector { get; set; }

        public override Main Main => info.Data[Selector].main;

        public override DateTime LastUpdated => Localize(info.Data[Selector].dt);

        public override Wind Wind => info.Data[Selector].wind;

        public override BitmapImage? Icon => new(new($"https://openweathermap.org/img/wn/{info.Data[0].weather[0].icon}@4x.png"),
            new());


        public override string Description => info.Data[0].weather[0].description;

        public override City City => info.city;

        ForecastInfo info;
        Dictionary<int, List<ForecastSpanData>> maped;

        public WeatherForecast(int selector, ForecastInfo info)
        {
            Dictionary<int, List<ForecastSpanData>> forecast = [];

            foreach (var data in info.Data)
            {
                List<ForecastSpanData>? spans;
                if (forecast.TryGetValue(data.dt.DayOfYear - DateTime.Now.DayOfYear, out spans))
                    spans.Add(data);
            }

            Selector = selector;
            this.info = info;
            this.maped = forecast;
        }
    }

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
        public int? type;
        public int? id;
        public string? country { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime sunrise { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime sunset { get; set; }
    }




    [Serializable]
    public class WeatherInfo : DataSource, IReflectable
    {

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

        public override DateTime LastUpdated => Localize(dt);

        public override BitmapImage? Icon => new(new($"https://openweathermap.org/img/wn/{weather[0].icon}@4x.png"),
            new());

        public override Main Main => main;

        public override Wind Wind => wind;

        public override string Description => weather[0].description;

        public override City City => new()
        {
            coord = coord,
            country = sys.country!,
            id = id,
            name = name,
            timezone = timezone,
            sunrise = sys.sunrise,
            sunset = sys.sunset,
        };
    }

    public abstract class DataSource: IReflectable
    {

        public abstract Main Main { get; }
        public abstract DateTime LastUpdated { get; }
        public abstract Wind Wind { get; }
        public abstract BitmapImage? Icon { get; }
        public abstract string Description { get; }
        public abstract City City { get; }


        public TimeOnly StartTime { get => TimeOnly.FromDateTime(LastUpdated); }
        public TimeOnly EndTime { get => TimeOnly.FromDateTime(LastUpdated); }


        public TimeOnly LocalSunrise => TimeOnly.FromDateTime(Localize(City.sunrise));
        public TimeOnly LocalSunset => TimeOnly.FromDateTime(Localize(City.sunset));

        public TimeSpan TimezoneOffset => TimeSpan.FromSeconds(City.timezone);

        public string UTCTimezone => City.timezone
             < 0 ? $"UTC-{Math.Abs(TimezoneOffset.Hours)}"
             : $"UTC+{TimezoneOffset.Hours}";

        public DateTime Localize(DateTime date) => date.Add(TimezoneOffset);


    }

    [Serializable]
    public class GeoLocation : IReflectable
    {
        public string? zip;
        public required string name;
#pragma warning disable IDE0044, CS50649 // Add readonly modifier
        [JsonProperty("lat")] double _latRaw;
        [JsonProperty("lon")] double _lonRaw;
#pragma warning restore IDE0044 // Add readonly modifier
        [Newtonsoft.Json.JsonIgnore]
        public Coordinates coords { get => new() { lat = _latRaw, lon = _lonRaw }; }

        public required string country;
        public string? state;
    }


    public class OpenWeatherMap
    {

        private const string APP_ID = "cb3c0e2b0a686064660539730a67c741";
        private HttpClient Client = new();
      

        public OpenWeatherMap()
        {
            
        }

        internal async Task<T?> GetAsJson<T>(string url)
            => await Client.GetStringAsync($"{url}&appid={APP_ID}")
                .ContinueWith((r) => JsonConvert.DeserializeObject<T>(r.Result));

        internal async Task<string?> GetAsString(string url)
        => await Client.GetStringAsync($"{url}&appid={APP_ID}");
        public async Task<GeoLocation?> GetLocationByZip(string zip, string country)
            => await GetAsJson<GeoLocation>($"http://api.openweathermap.org/geo/1.0/zip?zip={zip},{country}");
        public async Task<ForecastInfo?> GetForecastByCoords(Coordinates coords)
            => await GetAsJson<ForecastInfo>($"http://api.openweathermap.org/data/2.5/forecast?lat={coords.lat}&lon={coords.lon}&units=metric");
        public async Task<GeoLocation[]?> GetLocationByQuery(string query)
            => await GetAsJson<GeoLocation[]>($"http://api.openweathermap.org/geo/1.0/direct?q={query}&limit=5");
        public async Task<WeatherInfo?> GetCurrentWeatherByCoords(Coordinates coords)
          => await GetAsJson<WeatherInfo>($"https://api.openweathermap.org/data/2.5/weather?lat={coords.lat}&lon={coords.lon}&units=metric");
    }
}
