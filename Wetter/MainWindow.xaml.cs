using Microsoft.Win32;
using Newtonsoft.Json;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
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
                if (forecastInfo is null)
                    return null;
                else if (ViewPick.Options.SelectedValue is null)
                    return null;
                else if (
                    ViewPick.SelectedDate.DayOfYear == DateTime.Now.DayOfYear
                    && ((ListViewItem)ViewPick.Options.SelectedValue).Content.ToString() == "Live"
                    )
                    return localWeather is null ? null : localWeather;
           
                else
                {
                    forecastInfo.Day = ViewPick.SelectedDate;
                    forecastInfo.Timestamp = (TimeOnly)((ListViewItem)ViewPick.Options.SelectedValue).Tag;

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

            TaskbarItemInfo = new();

            _ = Init();
        }

        public async Task Init()
        {
            await Fetch();
            await Update();
            InvalidateData();
        }

        private void ViewPick_ChangeView(DateTime arg1, int arg2)
        {
            InvalidateData();
        }

        public void InvalidateData()
        {

            InvalidateProperty(DataContextProperty);
            DataContext = DataSource;
            LocationName.Text = DataSource?.City.Name;
        }


        private async Task Fetch()
        {
            try
            {
                localWeather = await map.GetCurrentWeatherByCoords(LocationCoords);
                forecastInfo = new WeatherForecast((await map.GetForecastByCoords(LocationCoords)).Value);

                ViewPick.Timestamps = localWeather!.Timestamps;

                foreach (var ts in forecastInfo.Timestamps)
                {
                    if (!ViewPick.Timestamps.ContainsKey(ts.Key))
                        ViewPick.Timestamps.Add(ts.Key, ts.Value);
                    else ViewPick.Timestamps[ts.Key] = 
                            ViewPick.Timestamps[ts.Key].Concat(ts.Value).ToArray();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void ReflectToTree(TreeViewItem tree, (Type type, object? value) refl, string name)
        {
            var item = new TreeViewItem() { Foreground = Brushes.White };

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
                if (DataSource is not null) ViewPick.Timestamps = DataSource!.Timestamps;

                await Fetch();

                DataContext = DataSource;

                LocationName.Text = DataSource?.City.Name;
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
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            await Fetch();
            await Update();

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {


            if (e.Property == DataContextProperty)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            }
                base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }

                if (e.Property.Name != "SelectedDate") return;
            MessageBox.Show(e.ToString());
        }
    }

}