// <copyright file="MicrosoftService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using GattServicesLibrary;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Microsoft services
    /// </summary>
    public class MicrosoftService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Microsoft Service";
            }
        }

        #region GUIDS
        /// <summary>
        /// Microsoft Service UUID
        /// </summary>
        public static readonly Guid MSFTServiceUuid = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D0");

        /// <summary>
        /// UUID for Microsoft service read characteristics
        /// </summary>
        public static readonly Guid MSFTReadChar = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D1");

        /// <summary>
        /// UUID for Microsoft service write characteristics
        /// </summary>
        public static readonly Guid MSFTWriteChar = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D2");

        /// <summary>
        /// UUID for Microsoft service notify characteristics
        /// </summary>
        public static readonly Guid MSFTNotifyChar = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D3");
        
        /// <summary>
        /// UUID for Microsoft service indicate characteristics
        /// </summary>
        public static readonly Guid MSFTIndicateChar = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D4");

        /// <summary>
        /// UUID for Microsoft service reading long characteristics
        /// </summary>
        public static readonly Guid MSFTLongChar = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D5");

        #endregion

        #region Characteristics
        /// <summary>
        /// Microsoft service read characteristics value
        /// </summary>
        private GenericGattCharacteristic readCharacteristic;

        /// <summary>
        /// Gets or sets the readCharacteristic
        /// </summary>
        public GenericGattCharacteristic ReadCharacteristic
        {
            get
            {
                return readCharacteristic;
            }

            set
            {
                if (readCharacteristic != value)
                {
                    readCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ReadCharacteristic"));
                }
            }
        }
        
        /// <summary>
        /// Microsoft service write characteristics value
        /// </summary>
        private GenericGattCharacteristic writeCharacteristic;

        /// <summary>
        /// Gets or sets the write characteristic
        /// </summary>
        public GenericGattCharacteristic WriteCharacteristic
        {
            get
            {
                return writeCharacteristic;
            }

            set
            {
                if (writeCharacteristic != value)
                {
                    writeCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("WriteCharacteristic"));
                }
            }
        }

        /// <summary>
        /// Microsoft service notify characteristics value
        /// </summary>
        private GenericGattCharacteristic notifyCharacteristic;

        /// <summary>
        /// Gets or sets the notify characteristic
        /// </summary>
        public GenericGattCharacteristic NotifyCharacteristic
        {
            get
            {
                return notifyCharacteristic;
            }

            set
            {
                if (notifyCharacteristic != value)
                {
                    notifyCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("NotifyCharacteristic"));
                }
            }
        }

        /// <summary>
        /// Microsoft service notify characteristics value
        /// </summary>
        private GenericGattCharacteristic indicateCharacteristic;
        
        /// <summary>
        /// Gets or sets the indicate characteristic
        /// </summary>
        public GenericGattCharacteristic IndicateCharacteristic
        {
            get
            {
                return indicateCharacteristic;
            }

            set
            {
                if (indicateCharacteristic != value)
                {
                    indicateCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IndicateCharacteristic"));
                }
            }
        }

        /// <summary>
        /// Microsoft service long characteristics value
        /// </summary>
        private GenericGattCharacteristic readLongCharacteristic;

        /// <summary>
        /// Gets or sets the read long characteristic
        /// </summary>
        public GenericGattCharacteristic ReadLongCharacteristic
        {
            get
            {
                return readLongCharacteristic;
            }

            set
            {
                if (readLongCharacteristic != value)
                {
                    readLongCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ReadLongCharacteristic"));
                }
            }
        }

        #endregion

        /// <summary>
        /// Asynchronous initialization
        /// </summary>
        /// <returns>Initialization Task</returns>
        public override async Task Init()
        {
            var serviceData = new byte[] {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            };
            ServiceData = WindowsRuntimeBuffer.Create(serviceData, 0, serviceData.Length, serviceData.Length);

            await CreateServiceProvider(MSFTServiceUuid);

            GattLocalCharacteristicResult result = null;

            // Prepare the Read Characteristic
            GattLocalCharacteristicParameters readParam = PlainReadParameter;
            readParam.UserDescription = "Microsoft Read characteristic";

            // Add presentation format - 16-bit integer, with exponent 0, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            readParam.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.Signed32BitInteger),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            GattLocalCharacteristic baseReadChar = null;
            result = await ServiceProvider.Service.CreateCharacteristicAsync(MSFTReadChar, readParam);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseReadChar);
            if (baseReadChar != null)
            {
                ReadCharacteristic = new Characteristics.MicrosoftReadCharacteristic(baseReadChar, this);
            }

            result = null;

            // Prepare the Write Characteristic
            GattLocalCharacteristicParameters writeParam = PlainWriteOrWriteWithoutRespondsParameter;
            writeParam.UserDescription = "Microsoft Write characteristic";

            // Add presentation format - 16-bit integer, with exponent 0, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            writeParam.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.UTF8String),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            GattLocalCharacteristic baseWriteChar = null;
            result = await ServiceProvider.Service.CreateCharacteristicAsync(MSFTWriteChar, writeParam);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseWriteChar);
            if (baseWriteChar != null)
            {
                WriteCharacteristic = new Characteristics.MicrosoftWriteCharacteristic(baseWriteChar, this);
            }

            result = null;

            // Prepare the Notify Characteristic
            GattLocalCharacteristicParameters notifyParam = PlainReadNotifyParameters;
            notifyParam.UserDescription = "Microsoft Notify characteristic";

            // Add presentation format - string, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            notifyParam.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.UTF8String),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            GattLocalCharacteristic baseNotifyChar = null;
            result = await ServiceProvider.Service.CreateCharacteristicAsync(MSFTNotifyChar, notifyParam);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseNotifyChar);
            if (baseNotifyChar != null)
            {
                NotifyCharacteristic = new Characteristics.MicrosoftNotifyCharacteristic(baseNotifyChar, this);
            }

            result = null;

            // Prepare the Indicate Characteristic
            GattLocalCharacteristicParameters indicateParam = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Indicate,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            indicateParam.UserDescription = "Microsoft Indicate characteristic";

            // Add presentation format - 16-bit integer, with exponent 0, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            indicateParam.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.UTF8String),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            GattLocalCharacteristic baseIndicateChar = null;
            result = await ServiceProvider.Service.CreateCharacteristicAsync(MSFTIndicateChar, indicateParam);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseIndicateChar);
            if (baseIndicateChar != null)
            {
                IndicateCharacteristic = new Characteristics.MicrosoftNotifyCharacteristic(baseIndicateChar, this);
            }

            result = null;

            // Prepare the Read Long Characteristic
            GattLocalCharacteristicParameters longParam = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Read,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            longParam.UserDescription = "Microsoft Read Long characteristic";

            // Add presentation format - 16-bit integer, with exponent 0, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            longParam.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.OpaqueStructure),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            GattLocalCharacteristic baseLongReadChar = null;
            result = await ServiceProvider.Service.CreateCharacteristicAsync(MSFTLongChar, longParam);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseLongReadChar);
            if (baseLongReadChar != null)
            {
                ReadLongCharacteristic = new Characteristics.MicrosoftReadLongCharacteristic(baseLongReadChar, this);
            }

            result = null;
        }
    }
}