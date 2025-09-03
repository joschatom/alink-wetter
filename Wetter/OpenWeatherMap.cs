using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
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


    public class City : IReflectable
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Coordinates Coord { get; set; }
        public required string Country { get; set; }
        public int? Population { get; set; }
        public int Timezone { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public DateTime Sunrise { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.UnixDateTimeConverter))]
        public DateTime Sunset { get; set; }
    }



    public class ForecastSpanData : IReflectable
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

    public class WeatherForecast : DataSource
    {
        internal DateTime Day { get; set; }
        internal TimeOnly Timestamp { get; set; }


        public override Main Main => this[Day, Timestamp].main;

        public override DateTime LastUpdated => Localize(this[Day, Timestamp].dt);

        public override Wind Wind => this[Day, Timestamp].wind;

        public override BitmapImage? Icon => new(new($"https://openweathermap.org/img/wn/{info.Data[0].weather[0].icon}@4x.png"),
            new());


        public override string Description => info.Data[0].weather[0].description;

        public override City City => info.city;

        public override Dictionary<DateTime, (string? desc, TimeOnly ts)[]> Timestamps
        {
            get
            {
                Dictionary<DateTime, (string? desc, TimeOnly ts)[]> lst = [];

                foreach (var kv in maped) {
                    (string? desc, TimeOnly ts)[] opts =
                        kv.Value.Keys.Select((o, _) => ((string?)null, o)).ToArray();

                    lst.Add(kv.Key, opts);
                
                }

                return lst;
            }
        }

        ForecastInfo info;
        Dictionary<DateTime, Dictionary<TimeOnly, ForecastSpanData>> maped;

        public ForecastSpanData this[DateTime day, TimeOnly timestamp]
            => maped[day][timestamp];

        public WeatherForecast(ForecastInfo info)
        {
            Dictionary<DateTime, Dictionary<TimeOnly, ForecastSpanData>> forecast = [];

            this.info = info;
            this.maped = forecast;

            foreach (var data in info.Data)
            {
                if (forecast.TryGetValue(data.dt.Date, out Dictionary<TimeOnly, ForecastSpanData>? spans))
                    spans.Add(TimeOnly.FromDateTime(Localize(data.dt)), data);
                else forecast.Add(data.dt.Date, new([new(TimeOnly.FromDateTime(Localize(data.dt)), data)]));
            }

            Day = DateTime.Now.Date;
            Timestamp = TimeOnly.MinValue;


        }


    }

    public class Indexed<I, T>
    {

        private Func<I, T> getFn { get; init; } = (_) => throw new NotImplementedException();
        private Action<I, T> setFn { get; init; } = (_, _) => throw new NotImplementedException();

        public Indexed(
            Func<I, T>? get = null,
            Action<I, T>? set = null
        )
        {

            this.getFn = get ?? getFn;
            this.setFn = set ?? setFn;

        }

        public T this[I idx] { get => getFn(idx); set => setFn(idx, value); }
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
            Coord = coord,
            Country = sys.country!,
            Id = id,
            Name = name,
            Timezone = timezone,
            Sunrise = sys.sunrise,
            Sunset = sys.sunset,
        };

        public override Dictionary<DateTime, (string? desc, TimeOnly ts)[]> Timestamps
            => new([new(dt.Date, [("Live", TimeOnly.FromDateTime(Localize(dt)))])]);
    }

    public abstract class DataSource : IReflectable
    {


        public abstract Main Main { get; }
        public abstract DateTime LastUpdated { get; }
        public abstract Wind Wind { get; }
        public abstract BitmapImage? Icon { get; }
        public abstract string Description { get; }
        public abstract City City { get; }

        public abstract Dictionary<DateTime, (string? desc, TimeOnly ts)[]> Timestamps { get; }

        public TimeOnly StartTime { get => TimeOnly.FromDateTime(LastUpdated); }
        public TimeOnly EndTime { get => TimeOnly.FromDateTime(LastUpdated); }


        public TimeOnly LocalSunrise => TimeOnly.FromDateTime(Localize(City.Sunrise));
        public TimeOnly LocalSunset => TimeOnly.FromDateTime(Localize(City.Sunset));

        public TimeSpan TimezoneOffset => TimeSpan.FromSeconds(City.Timezone);

        public string UTCTimezone => City.Timezone
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

        private const string APP_ID = "<insert token here>";
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
