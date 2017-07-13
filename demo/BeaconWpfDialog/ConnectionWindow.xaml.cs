using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace BeaconWpfDialog
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private readonly Beacon.Client probe;
        private readonly ObservableCollection<Beacon.Core.BeaconLocation> beaconsList = new ObservableCollection<Beacon.Core.BeaconLocation>();

        public ConnectionWindow(string probeName)
        {
            try
            {
                InitializeComponent();

                probe = new Beacon.Client(probeName, 1);
                probe.BeaconsUpdated += locations => Dispatcher.BeginInvoke((Action)(() => ReplaceBeaconsList(locations)));
                probe.Start();
            }
            catch (Exception e)
            {
                var x = e.Message;
            }
        }

        public string ConnectMessage
        {
            get { return Title; }
            set
            {
                Title = value;
                messageLabel.Content = value;
            }
        }

        private void ReplaceBeaconsList(IEnumerable<Beacon.Core.BeaconLocation> beacons)
        {
            beaconsList.Synchronise(beacons);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            probe.Dispose();
            Close();
        }

        public ObservableCollection<Beacon.Core.BeaconLocation> BeaconsList
        {
            get { return beaconsList; }
        }

        public Beacon.Core.BeaconLocation SelectedBeacon
        {
            get { return (Beacon.Core.BeaconLocation)GetValue(SelectedBeaconProperty); }
            set { SetValue(SelectedBeaconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBeacon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBeaconProperty =
            DependencyProperty.Register("SelectedBeacon", typeof(Beacon.Core.BeaconLocation), typeof(ConnectionWindow), new UIPropertyMetadata(null,
                (obj, e) => ((ConnectionWindow)obj).SelectedBeaconChanged()));

        private void SelectedBeaconChanged()
        {
            LoadBeaconDetails(SelectedBeacon);
        }

        public void LoadBeaconDetails(Beacon.Core.BeaconLocation beacon)
        {
            if (beacon == null)
            {
                HostName = "";
                PortNumber = 8080;
            }
            else 
            {
                HostName = beacon.Address.Address.ToString();
                PortNumber = beacon.Address.Port;
            }
        }

        private void PickList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InputComplete)
            {
                Button_Click(sender, null);
            }
        }

        private void CheckInput()
        {
            InputComplete = !string.IsNullOrEmpty(HostName.Trim()) && PortNumber != 0;
        }

        public string HostName
        {
            get { return (string)GetValue(HostNameProperty); }
            set { SetValue(HostNameProperty, value); }
        }

        public IPEndPoint Address
        {
            get
            {
                var addresses = Dns.GetHostAddresses(HostName);
                if (!addresses.Any()) throw new ArgumentException("No addresses found for the given host");
                return new IPEndPoint(addresses[0], PortNumber);
            }
        }

        public static readonly DependencyProperty HostNameProperty =
            DependencyProperty.Register("HostName", typeof(string), typeof(ConnectionWindow), new UIPropertyMetadata("",
                (obj, args) => ((ConnectionWindow)obj).CheckInput()
                ));

        public int PortNumber
        {
            get { return (int)GetValue(PortNumberProperty); }
            set { SetValue(PortNumberProperty, value); }
        }

        public static readonly DependencyProperty PortNumberProperty =
            DependencyProperty.Register("PortNumber", typeof(int), typeof(ConnectionWindow), new UIPropertyMetadata(8080,
                (obj, args) => ((ConnectionWindow)obj).CheckInput()
                ));

        public bool InputComplete
        {
            get { return (bool)GetValue(InputCompleteProperty); }
            set { SetValue(InputCompleteProperty, value); }
        }

        public static readonly DependencyProperty InputCompleteProperty =
            DependencyProperty.Register("InputComplete", typeof(bool), typeof(ConnectionWindow), new UIPropertyMetadata(false));

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConnectionWindow_OnClosed(object sender, EventArgs e)
        {
            probe.Stop();
        }

    }


    static class Helpers 
    {
        public static void Synchronise<T>(this ObservableCollection<T> currentItems, IEnumerable<T> newItems)
        {
            var newIndex = 0;
            foreach (var item in newItems)
            {
                var currentIndex = currentItems.IndexOf(item);
                if (currentIndex == -1)
                {
                    currentItems.Insert(newIndex, item);
                }
                else
                {
                    while (currentIndex > newIndex)
                    {
                        currentItems.RemoveAt(newIndex);
                        currentIndex--;
                    }
                }
                newIndex++;
            }
            while (currentItems.Count > newIndex)
                currentItems.RemoveAt(newIndex);
        }
    }
}
