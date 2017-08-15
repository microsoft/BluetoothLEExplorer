// <copyright file="GenericGattCharacteristicViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using GattServicesLibrary;
using Template10.Mvvm;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using GattHelper.Converters;
using System;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View model used to display a <see cref="GenericGattCharacteristic"/>
    /// </summary>
    public class GenericGattCharacteristicViewModel : ViewModelBase
    {
        /// <summary>
        /// Source for <see cref="Characteristic"/>
        /// </summary>
        private GenericGattCharacteristic characteristic;

        /// <summary>
        /// Gets the characteristic that this class wraps
        /// </summary>
        public GenericGattCharacteristic Characteristic
        {
            get
            {
                return characteristic;
            }

            private set
            {
                Set(ref characteristic, value, "Characteristic");
            }
        }

        /// <summary>
        /// Source for <see cref="UserDescription"/>
        /// </summary>
        private string userDescription;

        /// <summary>
        /// Gets or sets the user description
        /// </summary>
        public string UserDescription
        {
            get
            {
                return userDescription;
            }

            set
            {
                Set(ref userDescription, value, "UserDescription");
            }
        }

        /// <summary>
        /// Source for <see cref="Descriptors"/>
        /// </summary>
        private ObservableCollection<GattCharacteristicProperties> descriptors;

        /// <summary>
        /// Gets or sets the list of descriptors for this service
        /// </summary>
        private ObservableCollection<GattCharacteristicProperties> Descriptors
        {
            get
            {
                return descriptors;
            }

            set
            {
                Set(ref descriptors, value, "Descriptors");
            }
        }

        /// <summary>
        /// Source for <see cref="HasReadDescriptor"/>
        /// </summary>
        private bool hasReadDescriptor = true;

        /// <summary>
        /// Gets or sets a value indicating whether this characteristic can be read
        /// </summary>
        public bool HasReadDescriptor
        {
            get
            {
                return hasReadDescriptor;
            }

            set
            {
                Set(ref hasReadDescriptor, value, "HasReadDescriptor");
            }
        }

        /// <summary>
        /// Source for <see cref="HasWriteDescriptor"/>
        /// </summary>
        private bool hasWriteDescriptor = false;

        /// <summary>
        /// Gets or sets a value indicating whether this characteristic can write
        /// </summary>
        public bool HasWriteDescriptor
        {
            get
            {
                return hasWriteDescriptor;
            }

            set
            {
                Set(ref hasWriteDescriptor, value, "HasWriteDescriptor");
            }
        }

        /// <summary>
        /// Source for <see cref="HasWriteWithoutResponseDescriptor"/>
        /// </summary>
        private bool hasWriteWithoutResponseDescriptor = false;

        /// <summary>
        /// Gets or sets a value indicating whether this characteristic can write without responds
        /// </summary>
        public bool HasWriteWithoutResponseDescriptor
        {
            get
            {
                return hasWriteWithoutResponseDescriptor;
            }

            set
            {
                Set(ref hasWriteWithoutResponseDescriptor, value, "JasWriteWithoutRespondsDescriptor");
            }
        }

        /// <summary>
        /// Source for <see cref="HasNotifyDescriptor"/>
        /// </summary>
        private bool hasNotifyDescriptor = false;

        /// <summary>
        /// Gets or sets a value indicating whether this characteristic can notify
        /// </summary>
        public bool HasNotifyDescriptor
        {
            get
            {
                return hasNotifyDescriptor;
            }

            set
            {
                Set(ref hasNotifyDescriptor, value, "HasNotifyDescriptor");
            }
        }

        /// <summary>
        /// Source for <see cref="HasIndicateDescriptor"/> 
        /// </summary>
        private bool hasIndicateDescriptor = false;

        /// <summary>
        /// Gets or sets a value indicating whether this characteristic can indicate
        /// </summary>
        public bool HasIndicateDescriptor
        {
            get
            {
                return hasIndicateDescriptor;
            }

            set
            {
                Set(ref hasIndicateDescriptor, value, "HasIndicateDescriptor");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGattCharacteristicViewModel" /> class.
        /// </summary>
        /// <param name="characteristic"></param>
        public GenericGattCharacteristicViewModel(GenericGattCharacteristic characteristic)
        {
            this.characteristic = characteristic;
            userDescription = Characteristic.Characteristic.UserDescription;

            HasReadDescriptor = Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);
            HasWriteDescriptor = Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);
            HasWriteWithoutResponseDescriptor = Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);
            HasNotifyDescriptor = Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
            HasIndicateDescriptor = Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate);
        }

        /// <summary>
        /// This sets the value based on the input string with regard to the characteristics first presentation format
        /// </summary>
        /// <param name="value">value to set</param>
        /// <param name="isHexString">is the passed in string a hex byte array</param>
        public void SetValueFromString(string value, bool isHexString = false)
        {
            if(isHexString == true)
            {
                Characteristic.Value = GattConvert.ToIBufferFromHexString(value);
                return;
            }

            if(Characteristic.Characteristic.PresentationFormats.Count > 0)
            {
                byte format = Characteristic.Characteristic.PresentationFormats[0].FormatType;

                // Check our supported formats to convert a string to this Characteristics Value
                if(!((format != GattPresentationFormatTypes.SInt32) ||
                     (format != GattPresentationFormatTypes.Utf8)))
                {
                    throw new NotImplementedException("Only SInt32 and UTF8 are supported");
                }

                //TODO: Support more presentation types
                if(format == GattPresentationFormatTypes.SInt32)
                {
                    Characteristic.Value = GattConvert.ToIBuffer(Convert.ToInt32(value));
                }
                else if(format == GattPresentationFormatTypes.Utf8)
                {
                    Characteristic.Value = GattConvert.ToIBuffer(value);
                }
            }
        }
    }
}