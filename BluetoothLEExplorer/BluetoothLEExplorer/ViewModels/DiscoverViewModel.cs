// <copyright file="DiscoverViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using SortedObservableCollection;
using Windows.Devices.Bluetooth.Advertisement;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the device discovery page
    /// </summary>
    public class DiscoverViewModel : ViewModelBase
    {
        /// <summary>
        /// App context
        /// </summary>
        //private GattSampleContext context;

        public GattSampleContext Context
        {
            get; private set;
        }

        private object deviceListLock = new object();

        /// <summary>
        /// Source for <see cref="DeviceList"/>
        /// </summary>
        private SortedObservableCollection<ObservableBluetoothLEDevice> deviceList = new SortedObservableCollection<ObservableBluetoothLEDevice>(new ObservableBluetoothLEDevice.RSSIComparer(), "RSSI");

        /// <summary>
        /// Gets or sets the device list
        /// </summary>
        public SortedObservableCollection<ObservableBluetoothLEDevice> DeviceList
        {
            get
            {
                return deviceList;
            }

            set
            {
                Set(ref deviceList, value);
            }
        }

        /// <summary>
        /// Source for <see cref="SelectedDevice"/>
        /// </summary>
        private ObservableBluetoothLEDevice selectedDevice;

        /// <summary>
        /// Gets or sets currently selected service
        /// </summary>
        public ObservableBluetoothLEDevice SelectedDevice
        {
            get
            {
                return selectedDevice;
            }

            set
            {
                bool same = selectedDevice == value;

                Set(ref selectedDevice, value, "SelectedDevice");
                Context.SelectedBluetoothLEDevice = selectedDevice;

                if (same == false && value != null)
                {
                    ConnectToSelectedDevice();
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsEnumerating"/>
        /// </summary>
        private bool isEnumerating = false;

        /// <summary>
        /// Gets a value indicating whether app is currently enumerating BT LE devices
        /// </summary>
        public bool IsEnumerating
        {
            get
            {
                return Context.IsEnumerating;
            }

            private set
            {
                Set(ref isEnumerating, value, "IsEnumerating");
            }
        }

        /// <summary>
        /// Source for <see cref="EnumerationFinished"/>
        /// </summary>
        private bool enumerationFinished = false;

        /// <summary>
        /// Gets a value indicating whether enumeration has finished
        /// </summary>
        public bool EnumerationFinished
        {
            get
            {
                return enumerationFinished;
            }

            private set
            {
                Set(ref enumerationFinished, value, "EnumerationFinished");
            }
        }

        private bool continuousEnumeration = false;
        public bool ContinuousEnumeration
        {
            get
            {
                return continuousEnumeration;
            }

            set
            {
                Set(ref continuousEnumeration, value, "ContinuousEnumeration");
            }
        }

        /// <summary>
        /// Gets a value indicating whether peripheral role is supported
        /// </summary>
        public bool IsPeripheralRoleSupported
        {
            get
            {
                return GattSampleContext.Context.IsPeripheralRoleSupported;
            }
        }

        /// <summary>
        /// Gets a value indicating whether central role is supported by this device
        /// </summary>
        public bool IsCentralRoleSupported
        {
            get
            {
                return Context.IsCentralRoleSupported;
            }
        }

        private string gridFilter = string.Empty;
        public string GridFilter
        {
            get
            {
                return gridFilter;
            }

            set
            {
                if(gridFilter != value)
                {
                    Set(ref gridFilter, value, "GridFilter");
                    UpdateDeviceList();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoverViewModel" /> class.
        /// </summary>
        public DiscoverViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            Context = GattSampleContext.Context;
            Context.PropertyChanged += Context_PropertyChanged;
        }

        /// <summary>
        /// Context property changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEnumerating")
            {
                IsEnumerating = Context.IsEnumerating;
            }

            if (e.PropertyName == "EnumerationFinished")
            {
                EnumerationFinished = Context.EnumerationFinished;
                if(ContinuousEnumeration == true)
                {
                    Debug.WriteLine("Enumeration finished, but continue is set, continuing");
                    StartEnumeration();
                }
            }

            if (e.PropertyName == "IsPeripheralRoleSupported")
            {
                this.RaisePropertyChanged("IsPeripheralRoleSupported");
            }

            if (e.PropertyName == "IsCentralRoleSupported")
            {
                RaisePropertyChanged("IsCentralRoleSupported");
            }
        }

        /// <summary>
        /// The devices list changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BluetoothLEDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = string.Empty;
            lock (deviceListLock)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (ObservableBluetoothLEDevice newDev in e.NewItems)
                    {
                        if (ShouldShow(newDev))
                        {
                            DeviceList.Add(newDev);
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (ObservableBluetoothLEDevice oldDev in e.OldItems)
                    {
                        DeviceList.Remove(oldDev);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    DeviceList.Clear();
                }
            }

            UpdateDeviceList();
        }

        private bool ShouldShow(ObservableBluetoothLEDevice dev)
        {
            string filter = gridFilter.ToUpper();
            return (dev.Name.ToUpper().Contains(filter) || dev.BluetoothAddressAsString.ToUpper().Contains(filter));
        }

        private void UpdateDeviceList()
        {
            List<ObservableBluetoothLEDevice> toRemove = new List<ObservableBluetoothLEDevice>();

            lock (deviceListLock)
            {
                foreach (ObservableBluetoothLEDevice dev in DeviceList)
                {
                    if (ShouldShow(dev) == false)
                    {
                        toRemove.Add(dev);
                    }
                }

                foreach (ObservableBluetoothLEDevice dev in toRemove)
                {
                    DeviceList.Remove(dev);
                }

                foreach (ObservableBluetoothLEDevice dev in Context.BluetoothLEDevices)
                {
                    if (ShouldShow(dev) && DeviceList.Contains(dev) == false)
                    {
                        DeviceList.Add(dev);
                    }
                }
            }
        }

        /// <summary>
        /// Connect to the currently selected service
        /// </summary>
        public async void ConnectToSelectedDevice()
        {
            Debug.WriteLine("ConnectToSelectedDevice: Entering");
            StopEnumeration();
            Views.Busy.SetBusy(true, "Connecting to " + SelectedDevice.Name);

            Debug.WriteLine("ConnectToSelectedDevice: Trying to connect to " + SelectedDevice.Name);

            if (await SelectedDevice.Connect() == false)
            {
                Debug.WriteLine("ConnectToSelectedDevice: Something went wrong getting the BluetoothLEDevice");
                Views.Busy.SetBusy(false);
                SelectedDevice = null;
                NavigationService.Navigate(typeof(Views.Discover));
                return;
            }

            Debug.WriteLine("ConnectToSelectedDevice: Going to Device Service Page");
            Views.Busy.SetBusy(false);
            GotoDeviceServicesPage();
            Debug.WriteLine("ConnectToSelectedDevice: Exiting");
        }

        /// <summary>
        /// Navigate to page
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns>Navigate to task</returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
            }

            Context.BluetoothLEDevices.CollectionChanged += BluetoothLEDevices_CollectionChanged;
            GridFilter = "";
            if (Services.SettingsServices.SettingsService.Instance.CloseConnections)
            {
                Context.ReleaseAllResources();
            }
            UpdateDeviceList();

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
            if (suspending)
            {
            }

            Context.BluetoothLEDevices.CollectionChanged -= BluetoothLEDevices_CollectionChanged;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Navigate from task</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            Context.StopEnumeration();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Toggles the enumeration
        /// </summary>
        public async void ToggleEnumeration()
        {
            await Dispatcher.DispatchAsync(() =>
            {
                if (Context.IsEnumerating == false)
                {
                    Context.StartEnumeration();
                }
                else
                {
                    Context.StopEnumeration();
                }
            });
        }

        /// <summary>
        /// Start enumeration
        /// </summary>
        public async void StartEnumeration()
        {
            if (Context.IsEnumerating == true)
            {
                return;
            }

            await Dispatcher.DispatchAsync(() =>
            {
                Context.StartEnumeration();
            });
        }

        /// <summary>
        /// Stop enumeration
        /// </summary>
        public async void StopEnumeration()
        {
            if (Context.IsEnumerating == false)
            {
                return;
            }

            await Dispatcher.DispatchAsync(() =>
            {
                Context.StopEnumeration();
            });
        }

        /// <summary>
        /// Go to Devices page
        /// </summary>
        public void GotoDeviceServicesPage()
        {
            NavigationService.Navigate(typeof(Views.DeviceServicesPage));
        }

        /// <summary>
        /// Go to settings
        /// </summary>
        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        /// <summary>
        /// Go to privacy
        /// </summary>
        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        /// <summary>
        /// Go to about
        /// </summary>
        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);
    }
}
