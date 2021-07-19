// <copyright file="ObservableGattCharacteristics.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
    /// Wrapper around <see cref="GattCharacteristic"/>  to make it easier to use
    /// </summary>
    public class ObservableGattCharacteristics : INotifyPropertyChanged
    {
        /// <summary>
        /// Enum used to determine how the <see cref="Value"/> should be displayed
        /// </summary>
        public enum DisplayTypes
        {
            NotSet,
            Bool,
            Decimal,
            Hex,
            UTF8,
            UTF16,
            Unsupported
        }

        /// <summary>
        /// Raw buffer of this value of this characteristic
        /// </summary>
        private IBuffer rawData;

        /// <summary>
        /// byte array representation of the characteristic value
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Source for <see cref="Characteristic"/>
        /// </summary>
        private GattCharacteristic characteristic;

        /// <summary>
        /// Gets or sets the characteristic this class wraps
        /// </summary>
        public GattCharacteristic Characteristic
        {
            get
            {
                return characteristic;
            }

            set
            {
                if (characteristic != value)
                {
                    characteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Characteristic"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsIndicateSet"/>
        /// </summary>
        private bool isIndicateSet = false;

        /// <summary>
        /// Gets or sets a value indicating whether indicate is set
        /// </summary>
        public bool IsIndicateSet
        {
            get
            {
                return isIndicateSet;
            }

            set
            {
                if (isIndicateSet != value)
                {
                    isIndicateSet = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsIndicateSet"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsNotifySet"/>
        /// </summary>
        private bool isNotifySet = false;

        /// <summary>
        /// Gets or sets a value indicating whether notify is set
        /// </summary>
        public bool IsNotifySet
        {
            get
            {
                return isNotifySet;
            }

            set
            {
                if (isNotifySet != value)
                {
                    isNotifySet = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsNotifySet"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Parent"/>
        /// </summary>
        private ObservableGattDeviceService parent;

        /// <summary>
        /// Gets or sets the parent service of this characteristic
        /// </summary>
        public ObservableGattDeviceService Parent
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
        /// Source for <see cref="Characteristics"/>
        /// </summary>
        private ObservableCollection<ObservableGattDescriptors> descriptors = new ObservableCollection<ObservableGattDescriptors>();

        /// <summary>
        /// Gets or sets all the descriptors of this characterstic
        /// </summary>
        public ObservableCollection<ObservableGattDescriptors> Descriptors
        {
            get
            {
                return descriptors;
            }

            set
            {
                if (descriptors != value)
                {
                    descriptors = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Descriptors"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="SelectedDescriptor"/>
        /// </summary>
        private ObservableGattDescriptors selectedDescriptor;

        /// <summary>
        /// Gets or sets the currently selected characteristic
        /// </summary>
        public ObservableGattDescriptors SelectedDescriptor
        {
            get
            {
                return selectedDescriptor;
            }

            set
            {
                if (selectedDescriptor != value)
                {
                    selectedDescriptor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectedDescriptor"));

                    // The SelectedProperty doesn't exist when this object is first created. This takes
                    // care of adding the correct event handler after the first time it's changed.
                    SelectedDescriptor_PropertyChanged();
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
        /// Source for <see cref="ShortUUID"/>
        /// </summary>
        private string shortUuid;

        /// <summary>
        /// Gets or sets the ShortUUID of this characteristic
        /// </summary>
        public string ShortUUID
        {
            get
            {
                return shortUuid;
            }

            set
            {
                if (shortUuid != value)
                {
                    shortUuid = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ShortUUID"));
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
        /// Source for <see cref="DisplayType"/>
        /// </summary>
        private DisplayTypes displayType = DisplayTypes.NotSet;

        /// <summary>
        /// Gets or sets how this characteristic's value should be displayed
        /// </summary>
        public DisplayTypes DisplayType
        {
            get
            {
                return displayType;
            }

            set
            {
                if (value == DisplayTypes.NotSet)
                {
                    return;
                }

                if (displayType != value)
                {
                    displayType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DisplayType"));
                }
            }
        }

        /// <summary>
        /// Determines if the SelectedDescriptor_PropertyChanged has been added
        /// </summary>
        private bool hasSelectedDescriptorPropertyChangedHandler = false;

        /// <summary>
        /// Initializes a new instance of the<see cref="ObservableGattCharacteristics" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this class wraps</param>
        /// <param name="parent">The parent service that wraps this characteristic</param>
        public ObservableGattCharacteristics(GattCharacteristic characteristic, ObservableGattDeviceService parent)
        {
            Characteristic = characteristic;
            Parent = parent;
            Name = GattCharacteristicUuidHelper.ConvertUuidToName(characteristic.Uuid);
            var shortId = BluetoothUuidHelper.TryGetShortId(characteristic.Uuid);
            if (shortId.HasValue)
            {
                ShortUUID = "0x" + shortId.Value.ToString("X");
            }
            else
            {
                ShortUUID = "";
            }
            UUID = characteristic.Uuid.ToString();
        }

        public async Task Initialize()
        {
            await ReadValueAsync();
            await GetAllDescriptors();

            characteristic.ValueChanged += Characteristic_ValueChanged;
            PropertyChanged += ObservableGattCharacteristics_PropertyChanged;
        }

        /// <summary>
        /// Destruct this object by unsetting notification/indication and unregistering from property changed callbacks
        /// </summary>
        ~ObservableGattCharacteristics()
        {
            characteristic.ValueChanged -= Characteristic_ValueChanged;
            PropertyChanged -= ObservableGattCharacteristics_PropertyChanged;
            descriptors.Clear();

            Cleanup();
        }

        /// <summary>
        /// Cleanup this object by unsetting notification/indication
        /// </summary>
        private async void Cleanup()
        {
            await StopIndicate();
            await StopNotify();
        }

        /// <summary>
        /// Executes when this characteristic changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObservableGattCharacteristics_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                GattReadResult result = await characteristic.ReadValueAsync(
                    BluetoothLEExplorer.Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

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
        /// Adds the SelectedDescriptor_PropertyChanged event handler
        /// </summary>
        private void SelectedDescriptor_PropertyChanged()
        {
            if (hasSelectedDescriptorPropertyChangedHandler == false)
            {
                SelectedDescriptor.PropertyChanged += SelectedDescriptor_PropertyChanged;
                hasSelectedDescriptorPropertyChangedHandler = true;
            }
        }

        /// <summary>
        /// Updates the selected characteristic in the app context
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedDescriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GattSampleContext.Context.SelectedDescriptor = SelectedDescriptor;
        }

        private async Task GetAllDescriptors()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ObservableGattCharacteristics::getAllDescriptors: ");
            sb.Append(Name);

            try
            {
                GattDescriptorsResult result = await characteristic.GetDescriptorsAsync(Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    sb.Append(" - found ");
                    sb.Append(result.Descriptors.Count);
                    sb.Append(" descriptors");
                    Debug.WriteLine(sb);
                    foreach (GattDescriptor descriptor in result.Descriptors)
                    {
                        ObservableGattDescriptors temp = new ObservableGattDescriptors(descriptor, this);
                        await temp.Initialize();
                        Descriptors.Add(temp);
                    }
                }
                else if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    sb.Append(" - failed with Unreachable");
                    Debug.WriteLine(sb.ToString());
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    sb.Append(" - failed with ProtocolError");
                    Debug.WriteLine(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(" - Exception: {0}" + ex.Message);
                Value = "Unknown (exception: " + ex.Message + ")";
            }
        }

        /// <summary>
        /// Set's the indicate descriptor
        /// </summary>
        /// <returns>Set indicate task</returns>
        public async Task<bool> SetIndicate()
        {
            if (IsIndicateSet == true)
            {
                // already set
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = await
                        characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully registered for indications");
                    IsIndicateSet = true;
                    return true;
                }
                else if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error registering for indications: Protocol Error");
                    IsIndicateSet = false;
                    return false;
                }
                else if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error registering for indications: Unreachable");
                    IsIndicateSet = false;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Unauthorized Exception: " + ex.Message);
                IsIndicateSet = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Generic Exception: " + ex.Message);
                IsIndicateSet = false;
                return false;
            }

            IsIndicateSet = false;
            return false;
        }

        /// <summary>
        /// Unsets the indicate descriptor
        /// </summary>
        /// <returns>Unset indicate task</returns>
        public async Task<bool> StopIndicate()
        {
            if (IsIndicateSet == false)
            {
                // indicate is not set, can skip this
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = await
                        characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully un-registered for indications");
                    IsIndicateSet = false;
                    return true;
                }
                else if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error un-registering for indications: Protocol Error");
                    IsIndicateSet = true;
                    return false;
                }
                else if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error un-registering for indications: Unreachable");
                    IsIndicateSet = true;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Exception: " + ex.Message);
                IsIndicateSet = true;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Sets the notify characteristic
        /// </summary>
        /// <returns>Set notify task</returns>
        public async Task<bool> SetNotify()
        {
            if (IsNotifySet == true)
            {
                // already set
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = await
                        characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully registered for notifications");
                    IsNotifySet = true;
                    return true;
                }
                else if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error registering for notifications: Protocol Error");
                    IsNotifySet = false;
                    return false;
                }
                else if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error registering for notifications: Unreachable");
                    IsNotifySet = false;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Unauthorized Exception: " + ex.Message);
                IsNotifySet = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Generic Exception: " + ex.Message);
                IsNotifySet = false;
                return false;
            }

            IsNotifySet = false;
            return false;
        }

        /// <summary>
        /// Unsets the notify descriptor
        /// </summary>
        /// <returns>Unset notify task</returns>
        public async Task<bool> StopNotify()
        {
            if (IsNotifySet == false)
            {
                // indicate is not set, can skip this
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = await
                        characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully un-registered for notifications");
                    IsNotifySet = false;
                    return true;
                }
                else if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error un-registering for notifications: Protocol Error");
                    IsNotifySet = true;
                    return false;
                }
                else if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error un-registering for notifications: Unreachable");
                    IsNotifySet = true;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Exception: " + ex.Message);
                IsNotifySet = true;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Executes when value changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                SetValue(args.CharacteristicValue);
            });
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

            GattPresentationFormat format = null;

            if (characteristic.PresentationFormats.Count > 0)
            {
                format = characteristic.PresentationFormats[0];
            }

            // Determine what to set our DisplayType to
            if (format == null && DisplayType == DisplayTypes.NotSet)
            {
                if (name == "DeviceName")
                {
                    // All devices have DeviceName so this is a special case.
                    DisplayType = DisplayTypes.UTF8;
                }
                else
                {
                    string buffer = string.Empty;
                    bool isString = true;

                    try
                    {
                       buffer = GattConvert.ToUTF8String(rawData);
                    }
                    catch(Exception)
                    {
                        isString = false;
                    }

                    if (isString == true)
                    {

                        // if buffer is only 1 char or 2 char with 0 at end then let's assume it's hex
                        if (buffer.Length == 1)
                        {
                            isString = false;
                        }
                        else if (buffer.Length == 2 && buffer[1] == 0)
                        {
                            isString = false;
                        }
                        else
                        {
                            foreach (char b in buffer)
                            {
                                // if within the reasonable range of used characters and not null, let's assume it's a UTF8 string by default, else hex
                                if ((b < ' ' || b > '~') && b != 0)
                                {
                                    isString = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (isString)
                    {
                        DisplayType = DisplayTypes.UTF8;
                    }
                    else
                    {
                        // By default, display as Hex
                        DisplayType = DisplayTypes.Hex;
                    }
                }
            }
            else if (format != null && DisplayType == DisplayTypes.NotSet)
            {
                if (format.FormatType == GattPresentationFormatTypes.Boolean ||
                    format.FormatType == GattPresentationFormatTypes.Bit2 ||
                    format.FormatType == GattPresentationFormatTypes.Nibble ||
                    format.FormatType == GattPresentationFormatTypes.UInt8 ||
                    format.FormatType == GattPresentationFormatTypes.UInt12 ||
                    format.FormatType == GattPresentationFormatTypes.UInt16 ||
                    format.FormatType == GattPresentationFormatTypes.UInt24 ||
                    format.FormatType == GattPresentationFormatTypes.UInt32 ||
                    format.FormatType == GattPresentationFormatTypes.UInt48 ||
                    format.FormatType == GattPresentationFormatTypes.UInt64 ||
                    format.FormatType == GattPresentationFormatTypes.SInt8 ||
                    format.FormatType == GattPresentationFormatTypes.SInt12 ||
                    format.FormatType == GattPresentationFormatTypes.SInt16 ||
                    format.FormatType == GattPresentationFormatTypes.SInt24 ||
                    format.FormatType == GattPresentationFormatTypes.SInt32)
                {
                    DisplayType = DisplayTypes.Decimal;
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    DisplayType = DisplayTypes.UTF8;
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf16)
                {
                    DisplayType = DisplayTypes.UTF16;
                }
                else if (format.FormatType == GattPresentationFormatTypes.UInt128 ||
                    format.FormatType == GattPresentationFormatTypes.SInt128 ||
                    format.FormatType == GattPresentationFormatTypes.DUInt16 ||
                    format.FormatType == GattPresentationFormatTypes.SInt64 ||
                    format.FormatType == GattPresentationFormatTypes.Struct ||
                    format.FormatType == GattPresentationFormatTypes.Float ||
                    format.FormatType == GattPresentationFormatTypes.Float32 ||
                    format.FormatType == GattPresentationFormatTypes.Float64)
                {
                    DisplayType = DisplayTypes.Unsupported;
                }
                else
                {
                    DisplayType = DisplayTypes.Unsupported;
                }
            }

            // Decode the value into the right display type
            if (DisplayType == DisplayTypes.Hex || DisplayType == DisplayTypes.Unsupported)
            {
                try
                {
                    Value = GattConvert.ToHexString(rawData);
                }
                catch(Exception)
                {
                    Value = "Error: Invalid hex value";
                }
            }
            else if (DisplayType == DisplayTypes.Decimal)
            {
                try
                {
                    Value = GattConvert.ToInt64(rawData).ToString();
                }
                catch(Exception)
                {
                    Value = "Error: Invalid Int64 Value";
                }
            }
            else if (DisplayType == DisplayTypes.UTF8)
            {
                try
                {
                    Value = GattConvert.ToUTF8String(rawData);
                }
                catch(Exception)
                {
                    Value = "Error: Invalid UTF8 String";
                }
            }
            else if (DisplayType == DisplayTypes.UTF16)
            {
                try
                {
                    Value = GattConvert.ToUTF16String(rawData);
                }
                catch(Exception)
                {
                    Value = "Error: Invalid UTF16 String";
                }
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
            if (e.PropertyName == "DisplayType")
            {
                Debug.WriteLine($"{this.Name} - DisplayType set: {this.DisplayType.ToString()}");
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}