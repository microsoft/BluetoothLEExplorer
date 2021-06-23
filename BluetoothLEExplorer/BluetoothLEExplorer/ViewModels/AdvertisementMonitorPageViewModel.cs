using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using SortedObservableCollection;
using GattHelper.Converters;
using Windows.Foundation.Metadata;
using BluetoothLEExplorer.Services.AdvertisementHelpers;
using Windows.UI.Xaml.Data;
using Template10.Common;

namespace BluetoothLEExplorer.ViewModels
{
    public class AdvertisementMonitorPageViewModel : ViewModelBase
    {
        public enum MonitorScanningMode
        {
            NotSet,
            Passive,
            Active,
            None
        }

        private MonitorScanningMode scanningMode = MonitorScanningMode.Passive;

        public MonitorScanningMode ScanningMode
        {
            get
            {
                return scanningMode;
            }

            set
            {
                Set(ref scanningMode, value);
            }
        }

        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext Context;

        public bool IsWatcherStarted
        {
            get
            {
                return Context.AdvertisementWatcherStarted;
            }
        }

        public List<ObservableBluetoothLEAdvertisementFilter> KnownDataSectionFilters { get; private set; } = new List<ObservableBluetoothLEAdvertisementFilter>();

        private ObservableBluetoothLEAdvertisementFilter selectedDataSectionFilter = null;

        public ObservableBluetoothLEAdvertisementFilter SelectedDataSectionFilter
        {
            get
            {
                return selectedDataSectionFilter;
            }

            set
            {
                ShowDataSectionRawPane = value.DataSectionRawFilter;

                Set(ref selectedDataSectionFilter, value);
            }
        }

        private bool showDataSectionRawPane = false;

        public bool ShowDataSectionRawPane
        {
            get
            {
                return showDataSectionRawPane;
            }

            private set
            {
                Set(ref showDataSectionRawPane, value);
            }
        }

        private object advertisementsLock = new object();

        public SortedObservableCollection<ObservableBluetoothLEAdvertisement> AdvertisementsView { get; private set; } = new SortedObservableCollection<ObservableBluetoothLEAdvertisement>(new ObservableBluetoothLEAdvertisement.RssiComparer(), "Rssi");

        private ObservableBluetoothLEAdvertisement selectedAdvertisement;

        public ObservableBluetoothLEAdvertisement SelectedAdvertisement
        {
            get
            {
                return selectedAdvertisement;
            }

            set
            {
                bool same = (selectedAdvertisement == value);

                Set(ref selectedAdvertisement, value, "SelectedAdvertisement");
                Context.SelectedAdvertisement = selectedAdvertisement;

                if ((same == false) && (value != null))
                {
                    ViewAdvertisement();
                }
            }
        }

        public String ContentFilter
        {
            get
            {
                return Context.AdvertisementContentFilter;
            }

            set
            {
                if (Context.AdvertisementContentFilter != value)
                {
                    Context.AdvertisementContentFilter = value;
                    UpdateAdvertisementView();
                    RaisePropertyChanged("ContentFilter");
                }
            }
        }

        public AdvertisementMonitorPageViewModel()
        {
            Context = GattSampleContext.Context;
            Context.PropertyChanged += Context_PropertyChanged;

            foreach (var value in Enum.GetValues(typeof(AdvertisementSectionType)))
            {
                KnownDataSectionFilters.Add(new ObservableBluetoothLEAdvertisementFilter((byte)value));
            }
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AdvertisementWatcherStarted")
            {
                RaisePropertyChanged("IsWatcherStarted");
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Context.Advertisements.MapChanged += Advertisements_MapChanged; 
            await Task.CompletedTask;
        }

        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns>Navigate from task</returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            Context.Advertisements.MapChanged -= Advertisements_MapChanged;
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            Context.StopAdvertisementWatcher();
            await Task.CompletedTask;
        }

        public async void ToggleWatcher()
        {
            await Dispatcher.DispatchAsync(() =>
            {
                if (IsWatcherStarted == false)
                {
                    BluetoothLEScanningMode convertedScanningMode = BluetoothLEScanningMode.Passive;
                    if (scanningMode == MonitorScanningMode.Active)
                    {
                        convertedScanningMode = BluetoothLEScanningMode.Active;
                    }
                    else if ((scanningMode == MonitorScanningMode.None) &&
                        ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
                    {
                        convertedScanningMode = BluetoothLEScanningMode.None;
                    }

                    Context.StartAdvertisementWatcher(convertedScanningMode);
                }
                else
                {
                    Context.StopAdvertisementWatcher();
                }
            });
        }

        public void ApplyFilter()
        {
            var filter = new BluetoothLEAdvertisementFilter();
            if (SelectedDataSectionFilter != null)
            {
                filter.BytePatterns.Add(SelectedDataSectionFilter.GetBytePattern());
            }
            Context.UpdateAdvertisementFilter(filter);
        }

        public void ClearFilter()
        {
            Context.UpdateAdvertisementFilter(new BluetoothLEAdvertisementFilter());
        }

        private void Advertisements_MapChanged(Windows.Foundation.Collections.IObservableMap<string, ObservableBluetoothLEAdvertisement> sender, Windows.Foundation.Collections.IMapChangedEventArgs<string> @event)
        {
            lock (advertisementsLock)
            {
                if ((@event.CollectionChange == Windows.Foundation.Collections.CollectionChange.ItemInserted) ||
                    (@event.CollectionChange == Windows.Foundation.Collections.CollectionChange.ItemChanged))
                {
                    var advertisement = sender[@event.Key];

                    if (IsContentFilterMatch(Context.AdvertisementContentFilter, advertisement))
                    {
                        if (!AdvertisementsView.Contains(advertisement))
                        {
                            AdvertisementsView.Add(advertisement);
                        }
                        else
                        {
                            AdvertisementsView[AdvertisementsView.IndexOf(advertisement)].Update(advertisement);
                        }
                    }
                }
                else if (@event.CollectionChange == Windows.Foundation.Collections.CollectionChange.Reset)
                {
                    AdvertisementsView.Clear();
                }
                else if (@event.CollectionChange == Windows.Foundation.Collections.CollectionChange.ItemRemoved)
                {
                    foreach (var advertisement in AdvertisementsView)
                    {
                        if (advertisement.InternalHashString == @event.Key)
                        {
                            AdvertisementsView.Remove(advertisement);
                        }
                    }
                }
            }
        }

        private bool IsContentFilterMatch(String filter, ObservableBluetoothLEAdvertisement advertisement)
        {
            if (filter == "")
            {
                return true;
            }
            else if ((advertisement.AddressAsString.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (advertisement.PayloadAsString.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }
            return false;
        }

        private void UpdateAdvertisementView()
        {
            AdvertisementsView = new SortedObservableCollection<ObservableBluetoothLEAdvertisement>(new ObservableBluetoothLEAdvertisement.RssiComparer(), "Rssi");

            foreach (var advertisement in Context.Advertisements)
            {
                if (IsContentFilterMatch(ContentFilter, advertisement.Value))
                {
                    AdvertisementsView.Add(advertisement.Value);
                }
            }

            RaisePropertyChanged("AdvertisementsView");
        }

        public void ViewAdvertisement() => NavigationService.Navigate(typeof(Views.AdvertisementPage));

        /// <summary>
        /// Go to settings
        /// </summary>
        public void GotoSettings() => NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        /// <summary>
        /// Go to privacy
        /// </summary>
        public void GotoPrivacy() => NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        /// <summary>
        /// Go to about
        /// </summary>
        public void GotoAbout() => NavigationService.Navigate(typeof(Views.SettingsPage), 2);
    }
}
