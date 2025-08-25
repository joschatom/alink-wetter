using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Wetter.OpenWeatherMap;
using Wetter.Reflection;
using Microsoft.Win32;

namespace Wetter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private OpenWeatherMap map= new();
        Coordinates localtion = new();
        public WeatherInfo LocalWeather { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();

             Init();
        }

        private void ReflectToTree(TreeViewItem tree, (Type type, object? value) refl, string name)
        {
            var item = new TreeViewItem() { Foreground = Brushes.White };

            Console.WriteLine($"{refl.type} {refl.value}, {name}");

            item.Header = name;

            if (refl.value is null) return;


            if (IReflectable.IsReflectable(refl.type))
                Reflector<TreeViewItem>.Reflect((IReflectable)refl.value!, item, ReflectToTree);
            else item.Items.Add(new TreeViewItem() { Header = refl.value.ToString(), Foreground = Brushes.White});
            
            tree.Items.Add(item);
        }

        internal async Task Update()
        {
            try
            {
                LastUpdated = DateTime.Now;
                LastUpdatedDisplay.Text = LastUpdated.ToString();



                var weather = await map.GetCurrentWeatherByCoords(localtion);

                LocalWeather = weather!;
                DataContext = LocalWeather;


                var tree = new TreeViewItem() { Header = "weather", Foreground = Brushes.White, IsSelected = false };

                Reflector<TreeViewItem>.Reflect(weather!, tree, ReflectToTree);

                List<TreeViewItem> items = [tree];

                Tree.ItemsSource = items;

                if (Tree.SelectedItem is not null) ((TreeViewItem)Tree.SelectedItem).IsSelected = false; 

                LocationName.Text = weather?.name;
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        protected async void Init()
        {

            LocationName.Text = "London";

            await Update();

        }


        private async void CityInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
  

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GeoLocation[]? locations;
                try
                {
                    locations = await map.GetLocationByQuery(LocationName.Text);
                } catch ( Exception ex ) { MessageBox.Show(ex.Message); return; }

                 var set = false;

                if (locations?.Count() == 0)
                {
                    MessageBox.Show($"Location '{LocationName.Text}' not found.");
                    return;
                }

                foreach (GeoLocation geoLocation in locations!) {
                    var result =  MessageBox.Show(
                        $"Do you mean {geoLocation.name}, {geoLocation.state}, {geoLocation.country}?",
                        "Location Selection", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        
                        localtion = geoLocation.coords;
                        set = true;
                        break;
                    }
                }


                if (set) await Update();
                else MessageBox.Show("No Location Selected", "Location Selection");
            }
        }


        private async void Update_Click(object sender, RoutedEventArgs e)
            => await Update();
    }
}