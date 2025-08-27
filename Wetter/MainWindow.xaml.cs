using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wetter.Reflection;
using static Wetter.OpenWeatherMap;

namespace Wetter
{
    public class Once<T>
    {
        private T? val;
        private Func<T> init;

        public Once(Func<T> initfn)
        {
            val = default;
            init = initfn;
        }

        public T Value { 
            get
            {
                val ??= init();

                return val!;
            }
            set
            {
                val = value!;
            }
        }
       }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private OpenWeatherMap map = new();        
        Coordinates LocationCoords = new();
        public DataSource? DataSource
        {
            get
            {
                if (ViewPicker.Day == 0)
                    return localWeather is null ? null : localWeather;
                else if (forecastInfo is null)
                    return null;
                else {
                    forecastInfo.Selector = DataIdx;
                    return forecastInfo!;
                }
            }
        }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        private WeatherInfo? localWeather;
        private WeatherForecast? forecastInfo; 

        public MainWindow()
        {
            InitializeComponent();

            ViewPicker.ChangeView += ViewPicker_ChangeView;

            _ = Update();
        }

        private void ViewPicker_ChangeView(int day)
        {
            MessageBox.Show("CHANGE VIEW");
        }

        private int DataIdx => ((ViewPicker.Day - 1) * 8) + ViewPicker.Timespan;

        private async void Fetch()
        {
            try
            {
                localWeather = await map.GetCurrentWeatherByCoords(LocationCoords);
                forecastInfo = new WeatherForecast(DataIdx, (await map.GetForecastByCoords(LocationCoords)).Value);
            }catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void ReflectToTree(TreeViewItem tree, (Type type, object? value) refl, string name)
        {
            var item = new TreeViewItem() { Foreground = Brushes.White };

            Console.WriteLine($"{refl.type} {refl.value}, {name}");

            item.Header = name;


            if (refl.value is null) return;
            var value = new TreeViewItem() { Header = refl!.value.ToString(), Foreground = Brushes.White };

            if (IReflectable.IsReflectable(refl.type))
                Reflector<TreeViewItem>.Reflect((IReflectable)refl.value!, value, ReflectToTree);
            
            item.Items.Add(value);

            tree.Items.Add(item);
        }

        internal async Task Update()
        {
            try
            {
                LastUpdated = DateTime.Now;
                LastUpdatedDisplay.Text = LastUpdated.ToString();

                Fetch();

                DataContext = DataSource;


                var tree = new TreeViewItem() { Header = "weather", Foreground = Brushes.White, IsSelected = false };

                
                if(DataSource is not null) Reflector<TreeViewItem>.Reflect(DataSource!, tree, ReflectToTree);

                List<TreeViewItem> items = [tree];

                Tree.ItemsSource = items;

                if (Tree.SelectedItem is not null) ((TreeViewItem)Tree.SelectedItem).IsSelected = false;

                LocationName.Text = DataSource?.City.name;
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }




        private async void CityInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GeoLocation[]? locations;
                try
                {
                    locations = await map.GetLocationByQuery(LocationName.Text);
                } catch (Exception ex) { MessageBox.Show(ex.Message); return; }

                var set = false;

                if (locations?.Length == 0)
                {
                    MessageBox.Show($"Location '{LocationName.Text}' not found.");
                    return;
                }

                foreach (GeoLocation geoLocation in locations!) {
                    var result = MessageBox.Show(
                        $"Do you mean {geoLocation.name}, {geoLocation.state}, {geoLocation.country}?",
                        "Location Selection", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        LocationCoords = geoLocation.coords;
                        set = true;
                        break;
                    }
                }

                if (set)
                {

                    await Update();
                }
                else MessageBox.Show("No Location Selected", "Location Selection");
            }
        }


        private async void Update_Click(object sender, RoutedEventArgs e)
        {


             await Update();
        }
    }
}