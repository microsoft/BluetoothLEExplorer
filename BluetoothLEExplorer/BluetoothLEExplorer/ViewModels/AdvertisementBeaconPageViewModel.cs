// <copyright file="BeaconViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Template10.Mvvm;
using Template10.Services.NavigationService;

using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.Advertisement;

using BluetoothLEExplorer.Models;
using BluetoothLEExplorer.Services.AdvertisementHelpers;
using System.Windows.Input;
using GattHelper.Converters;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using Windows.Foundation.Metadata;

namespace BluetoothLEExplorer.ViewModels
{
    public class AdvertisementBeaconPageNewBeaconViewModel : ViewModelBase
    {
        private bool useExtendedFormat = false;

        public bool UseExtendedFormat
        {
            get
            {
                return useExtendedFormat;
            }

            set
            {
                if (value != useExtendedFormat)
                {
                    Set(ref useExtendedFormat, value, "UseExtendedFormat");
                    IsValid = AreParametersValid();
                }
            }
        }

        private bool isAnonymous = false;

        public bool IsAnonymous
        {
            get
            {
                return isAnonymous;
            }

            set
            {
                if (value != isAnonymous)
                {
                    Set(ref isAnonymous, value, "IsAnonymous");
                    IsValid = AreParametersValid();
                }
            }
        }

        private bool includeTxPower = false;

        public bool IncludeTxPower
        {
            get
            {
                return includeTxPower;
            }

            set
            {
                if (value != includeTxPower)
                {
                    Set(ref includeTxPower, value, "IncludeTxPower");
                    IsValid = AreParametersValid();
                }
            }
        }

        private String preferredTxPower = "";

        public String PreferredTxPower
        {
            get
            {
                return preferredTxPower;
            }

            set
            {
                if (value != preferredTxPower)
                {
                    Set(ref preferredTxPower, value, "PreferredTxPower");
                    IsValid = AreParametersValid();
                }
            }
        }

        private String payload = "";

        public String Payload
        {
            get
            {
                return payload;
            }

            set
            {
                if (value != payload)
                {
                    Set(ref payload, value, "Payload");
                    IsValid = AreParametersValid();
                }
            }
        }

        private bool isValid = false;

        public bool IsValid
        {
            get
            {
                return isValid;
            }

            private set
            {
                if (value != isValid)
                {
                    Set(ref isValid, value, "IsValid");
                }
            }
        }

        public void Reset()
        {
            UseExtendedFormat = false;
            IsAnonymous = false;
            IncludeTxPower = false;
            PreferredTxPower = "";
            Payload = "";
        }

        public bool AreParametersValid()
        {
            if (payload == "")
            {
                return false;
            }
            else if (isAnonymous || includeTxPower)
            {
                if (!useExtendedFormat)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertisementBeaconPageViewModel" /> class. Currently unused.
    /// </summary>
    public class AdvertisementBeaconPageViewModel : ViewModelBase
    {
        private GattSampleContext Context;

        public ICommand CreateBeaconCommand
        {
            get
            {
                return new DelegateCommand<AdvertisementBeaconPageNewBeaconViewModel>((x) => { if (x != null) { CreateBeacon(x); } });
            }
        }

        public ObservableCollection<ObservableBluetoothLEBeacon> Beacons
        {
            get { return Context.Beacons; }
            private set { Context.Beacons = value; }
        }

        public ObservableBluetoothLEBeacon SelectedBeacon
        {
            get { return Context.SelectedBeacon; }

            set
            {
                if (Context.SelectedBeacon != value)
                {
                    Context.SelectedBeacon = value;
                    if (Context.SelectedBeacon != null)
                    {
                        NavigateToBeacon();
                    }
                }
            }
        }

        public AdvertisementBeaconPageViewModel()
        {
            Context = GattSampleContext.Context;
            Context.PropertyChanged += Context_PropertyChanged;
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        public void CreateBeacon(AdvertisementBeaconPageNewBeaconViewModel parameters)
        {
            short? preferredTxPower = null;
            if (parameters.PreferredTxPower != "")
            {
                preferredTxPower = Convert.ToInt16(parameters.PreferredTxPower);
            }

            var beacon = new ObservableBluetoothLEBeacon(
                GattConvert.ToIBufferFromHexString(parameters.Payload).ToArray(),
                parameters.UseExtendedFormat,
                parameters.IsAnonymous,
                parameters.IncludeTxPower,
                preferredTxPower);
            Beacons.Add(beacon);

            // Reset the parameters after creation.
            parameters.Reset();
        }

        public void NavigateToBeacon() => NavigationService.Navigate(typeof(Views.AdvertisementBeaconDetailsPage));

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
