using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wetter
{
    public class NumberSelection : RibbonControlGroup
    {
        public int Count { get; set; }
        public int Selection {  get; set; }

        public NumberSelection() {
            for (int i = 0; i <= Count; i++)
            {
                var opt = new RibbonRadioButton() { Label = i.ToString() };
                opt.Checked += (_, _) => Selection = i;
                AddChild(opt);
            }
        }
    }

    public class Tracker
    {
        public int Value { get; set; }

        public bool this[int i]
        {
            get
            {
                return Value == i;
            }
            set
            {
                if (value) Value = i;
            }
        }
    } 

    /// <summary>
    /// Interaction logic for Picker.xaml
    /// </summary>
    public partial class Picker : UserControl
    {
        public int Day => DayTracker.Value;
        public int Timespan { get; set; } = 0;
        public Tracker DayTracker { get; set; } = new();

        public event Action<int>? ChangeView;
        
        public Picker()
        {
            InitializeComponent();

            DaySelection.DataContext = DayTracker;
            DataContext = this;

            var time = DateTime.UnixEpoch;

            for (int i = 0; i < 8; i++)
            { 
                if (time.Day > 1) { throw new Exception("out of day"); }

                var prev = time;

                time = time.AddHours(3);

                Options.Items.Add(new RibbonGalleryItem() { 
                    Content = $"{TimeOnly.FromDateTime(prev)} - {TimeOnly.FromDateTime(time)}",
                    Tag = i
                });
            }
        }

        private void RibbonGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Timespan = (int)((RibbonGalleryItem)((RibbonGallery)sender).SelectedItem).Tag;
        }
    }
}
