// <copyright file="ObservableGattDescriptors.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.GattUuidHelpers;
using BluetoothLEExplorer.Services.Other;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using GattHelper.Converters;

namespace BluetoothLEExplorer.Models
{
    /// <summary>
    /// Wrapper around <see cref="GattDescriptor"/>  to make it easier to use
    /// </summary>
    public class ObservableGattDescriptors : INotifyPropertyChanged
    {
        /// <summary>
        /// Raw buffer of this value of this descriptor
        /// </summary>
        private IBuffer rawData;

        /// <summary>
        /// byte array representation of the descriptor value
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Source for <see cref="Descriptor"/>
        /// </summary>
        private GattDescriptor descriptor;

        /// <summary>
        /// Gets or sets the characteristic this class wraps
        /// </summary>
        public GattDescriptor Descriptor
        {
            get
            {
                return descriptor;
            }

            set
            {
                if (descriptor != value)
                {
                    descriptor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Descriptor"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Parent"/>
        /// </summary>
        private ObservableGattCharacteristics parent;

        /// <summary>
        /// Gets or sets the parent service of this characteristic
        /// </summary>
        public ObservableGattCharacteristics Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (parent != value)
                {
                    parent = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Parent"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the name of this characteristic
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="UUID"/>
        /// </summary>
        private string uuid;

        /// <summary>
        /// Gets or sets the UUID of this characteristic
        /// </summary>
        public string UUID
        {
            get
            {
                return uuid;
            }

            set
            {
                if (uuid != value)
                {
                    uuid = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UUID"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Gets the value of this characteristic
        /// </summary>
        public string Value
        {
            get
            {
                return value;
            }

            private set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Value"));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="ObservableGattDescriptors" /> class.
        /// </summary>
        /// <param name="descriptor">Descriptor this class wraps</param>
        /// <param name="parent">The parent service that wraps this characteristic</param>
        public ObservableGattDescriptors(GattDescriptor descriptor, ObservableGattCharacteristics parent)
        {
            Descriptor = descriptor;
            Parent = parent;
            Name = GattDescriptorUuidHelper.ConvertUuidToName(descriptor.Uuid);
            UUID = descriptor.Uuid.ToString();
        }

        public async Task Initialize()
        {
            await ReadValueAsync();
            PropertyChanged += ObservableGattDescriptors_PropertyChanged;
        }

        ~ObservableGattDescriptors()
        {
            PropertyChanged -= ObservableGattDescriptors_PropertyChanged;
        }

        /// <summary>
        /// Executes when this characteristic changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObservableGattDescriptors_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayType")
            {
                SetValue();
            }
        }

        /// <summary>
        /// Reads the value of the Characteristic
        /// </summary>
        public async Task ReadValueAsync()
        {
            try
            {
                GattReadResult result = await descriptor.ReadValueAsync(
                    Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    SetValue(result.Value);
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    Value = Services.Other.GattProtocolErrorParser.GetErrorString(result.ProtocolError);
                }
                else
                {
                    Value = "Unreachable";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
                Value = "Unknown (exception: " + ex.Message + ")";
            }
        }

        /// <summary>
        /// helper function that copies the raw data into byte array
        /// </summary>
        /// <param name="buffer">The raw input buffer</param>
        private void SetValue(IBuffer buffer)
        {
            rawData = buffer;
            CryptographicBuffer.CopyToByteArray(rawData, out data);

            SetValue();
        }

        /// <summary>
        /// Sets the value of this characteristic based on the display type
        /// </summary>
        private void SetValue()
        {
            if (data == null)
            {
                Value = "NULL";
                return;
            }

            Value = GattConvert.ToHexString(rawData);
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
}
