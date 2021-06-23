// <copyright file="ObservableBluetoothLEDevice.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.DispatcherService;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using System.Collections;
using System.Collections.Generic;
using GattHelper.Converters;
using BluetoothLEExplorer.Services.AdvertisementHelpers;
using Windows.UI.Xaml.Media.Animation;

namespace BluetoothLEExplorer.Models
{
    public class ObservableBluetoothLEAdvertisementSection : INotifyPropertyChanged
    {
        public string TypeAsString
        {
            get;
            private set;
        }

        public string TypeAsDisplayString
        {
            get;
            private set;
        }

        public string DataAsString
        {
            get;
            private set;
        }

        public string DataAsDisplayString
        {
            get;
            private set;
        }

        public ObservableBluetoothLEAdvertisementSection(BluetoothLEAdvertisementDataSection section)
        {
            TypeAsString = section.DataType.ToString("X2");
            TypeAsDisplayString = AdvertisementDataTypeHelper.ConvertSectionTypeToString(section.DataType);

            DataAsString = GattConvert.ToHexString(section.Data);
            if (section.DataType == BluetoothLEAdvertisementDataTypes.Flags)
            {
                var flagsInt = GattConvert.ToInt32(section.Data);
                DataAsDisplayString = ((BluetoothLEAdvertisementFlags)Enum.ToObject(typeof(BluetoothLEAdvertisementFlags), flagsInt)).ToString();
            }
            else if (section.DataType == BluetoothLEAdvertisementDataTypes.CompleteLocalName ||
                section.DataType == BluetoothLEAdvertisementDataTypes.ShortenedLocalName)
            {
                DataAsDisplayString = GattConvert.ToUTF8String(section.Data);
            }
            else if (section.DataType == BluetoothLEAdvertisementDataTypes.TxPowerLevel)
            {
                var txPowerLevel = GattConvert.ToInt16(section.Data);
                DataAsDisplayString = txPowerLevel.ToString();
            }
            else
            {
                DataAsDisplayString = "<Unknown>";
            }
        }

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Executes when this class changes
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }

    public class ObservableBluetoothLEAdvertisement : INotifyPropertyChanged
    {
        public class RssiComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var a = x as ObservableBluetoothLEAdvertisement;
                var b = y as ObservableBluetoothLEAdvertisement;

                if (a == null || b == null)
                {
                    throw new InvalidOperationException("Compared objects are not ObservableBluetoothLEAdvertisement");
                }

                // If they're equal
                if (SmoothValue(a.Rssi) == SmoothValue(b.Rssi))
                {
                    return 0;
                }

                if (a.Rssi < b.Rssi)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            private short SmoothValue(short value)
            {
                var remainder = (value % 10);
                if (remainder >= 5)
                {
                    return (short)(value - remainder + 10);
                }
                return (short)(value - remainder);
            }
        }

        /// <summary>
        /// Gets the bluetooth address of this device as a string
        /// </summary>
        public string AddressAsString
        {
            get;
            private set;
        }

        public BluetoothAddressType AddressType { get; private set; } = BluetoothAddressType.Unspecified;

        public BluetoothLEAdvertisementType Type
        {
            get;
            private set;
        }

        public Int16 Rssi
        {
            get;
            private set;
        }

        public bool Anonymous { get; private set; } = false;

        public bool Connectable { get; private set; } = false;
        public bool Scannable { get; private set; } = false;
        public bool Directed { get; private set; } = false;
        public bool ScanResponse { get; private set; } = false;

        public Nullable<Int16> TxPower { get; private set; } = null;

        public String PayloadAsString
        {
            get
            {
                String payload = "";
                foreach (var section in DataSections)
                {
                    payload += String.Format("{0}-{1}-", section.TypeAsString, section.DataAsString);
                }
                return payload.TrimEnd('-');
            }
        }

        public String InternalHashString
        {
            get;
            private set;
        }

        public DateTime Timestamp
        {
            get;
            private set;
        }

        public ObservableCollection<ObservableBluetoothLEAdvertisementSection> DataSections { get; } = new ObservableCollection<ObservableBluetoothLEAdvertisementSection>();

        public ObservableBluetoothLEAdvertisement(BluetoothLEAdvertisementReceivedEventArgs advertisementEvent)
        {
            AddressAsString = advertisementEvent.BluetoothAddress.ToString("X12");
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
            {
                AddressType = advertisementEvent.BluetoothAddressType;
                Anonymous = advertisementEvent.IsAnonymous;
                Connectable = advertisementEvent.IsConnectable;
                Scannable = advertisementEvent.IsScannable;
                Directed = advertisementEvent.IsDirected;
                ScanResponse = advertisementEvent.IsScanResponse;
                TxPower = advertisementEvent.TransmitPowerLevelInDBm;
            }
            else
            {
                if (advertisementEvent.AdvertisementType == BluetoothLEAdvertisementType.ConnectableDirected)
                {
                    Connectable = true;
                    Directed = true;
                }
                else if (advertisementEvent.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected)
                {
                    Connectable = true;
                }
                else if (advertisementEvent.AdvertisementType == BluetoothLEAdvertisementType.ScannableUndirected)
                {
                    Scannable = true;
                }
                else if (advertisementEvent.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse)
                {
                    ScanResponse = true;
                }
            }

            Type = advertisementEvent.AdvertisementType;
            Rssi = advertisementEvent.RawSignalStrengthInDBm;
            Timestamp = advertisementEvent.Timestamp.LocalDateTime;

            foreach (var section in advertisementEvent.Advertisement.DataSections)
            {
                DataSections.Add(new ObservableBluetoothLEAdvertisementSection(section));
            }

            InternalHashString = AddressAsString + PayloadAsString;
        }

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(ObservableBluetoothLEAdvertisement other)
        {
            if (AddressType != other.AddressType)
            {
                AddressType = other.AddressType;
                OnPropertyChanged(new PropertyChangedEventArgs("AddressType"));
            }

            if (TxPower != other.TxPower)
            {
                TxPower = other.TxPower;
                OnPropertyChanged(new PropertyChangedEventArgs("TxPower"));
            }

            if (Type != other.Type)
            {
                Type = other.Type;
                OnPropertyChanged(new PropertyChangedEventArgs("Type"));
            }

            if (Rssi != other.Rssi)
            {
                Rssi = other.Rssi;
                OnPropertyChanged(new PropertyChangedEventArgs("Rssi"));
            }

            if (Timestamp != other.Timestamp)
            {
                Timestamp = other.Timestamp;
                OnPropertyChanged(new PropertyChangedEventArgs("Timestamp"));
            }

            Anonymous = other.Anonymous;
            Connectable = other.Connectable;
            Scannable = other.Scannable;
            Directed = other.Directed;
            ScanResponse = other.ScanResponse;
        }

        public override int GetHashCode()
        {
            return InternalHashString.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return InternalHashString.Equals(((ObservableBluetoothLEAdvertisement)obj).InternalHashString);
        }

        /// <summary>
        /// Executes when this class changes
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
