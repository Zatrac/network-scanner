using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Pinger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            listHosts.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            listHosts.Items.IsLiveSorting = true;        
        }

        private async void BtnScan_OnClick(object sender, RoutedEventArgs e)
        {
            var range = txtIp.Text + cbCidr.Text;
        
            var scanner = new Scanner(range);
            var scans = scanner.Scan();

            ObservableCollection<Host> hosts = new ObservableCollection<Host>();
            listHosts.ItemsSource = hosts;
            hosts.Clear();

            while (scans.Count > 0)
            {
                var scan = await Task.WhenAny(scans);
                var result = await scan;
                scans.Remove(scan);

                if (result.Ping.Status == IPStatus.Success)
                {
                    hosts.Add(new Host() { IP = result.IpAddress.ToString(), DNS = result.Dns ?? "Unknown", PING = result.Ping.RoundtripTime.ToString() + "ms"});
                }
            }
        }

        public class Host : IComparable
        {
            public string IP { get; set; }
            public string DNS { get; set; }
            public string PING { get; set; }

            public int CompareTo(object obj)
            {
                return 0;
            }
        }
    }
}