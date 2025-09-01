using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

    /// <summary>
    /// Interaction logic for Picker.xaml
    /// </summary>
    public partial class Picker : UserControl, INotifyPropertyChanged
    {
        public int Timespan { get; set; } = 0;

        public event Action<DateTime, int>? ChangeView = null;
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty SelectedDateProperty
            = DependencyProperty.Register("SelectedDate", typeof(DateTime), typeof(Picker));
        public DateTime SelectedDate => DateSel.SelectedDate ?? DateTime.Now.Date;
        public int HourDetail => 3;

        public Dictionary<DateTime, (string? desc, TimeOnly ts)[]> Timestamps
        {
            get; set;
        }
            = [];

    
        public Picker()
        {
            InitializeComponent();

            DataContext = this;
        }

        public void Update(DateTime dt)
        {
            Options.Items.Clear();
            if (Timestamps.TryGetValue(dt, out var kvs))
                foreach (var dts in kvs)
                {

                    Options.Items.Add(new ListViewItem()
                    {
                        Content = dts.desc is null
                        ? dts.ts.ToString()
                        : dts.desc!,
                        ToolTip = dts.ts.ToString(),
                        Tag = dts.ts,
                    });
                }
        }

        private void DateSel_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            Update((DateTime)e.AddedItems![0]!);

            ChangeView?.Invoke((DateTime)e.AddedItems?[0]!, Options.SelectedIndex);
        }


        private void Options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeView?.Invoke(SelectedDate, Options.SelectedIndex); 
        }
    }
}
