// <copyright file="CharacteristicPageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using GattHelper.Converters;
using BluetoothLEExplorer.Services.GattUuidHelpers;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View model for Characteristics View
    /// </summary>
    public class CharacteristicPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Enum to determine how to write the <see cref="ValueToWrite"/> to the server
        /// </summary>
        public enum WriteTypes
        {
            NotSet,
            Decimal,
            Hex,
            UTF8
        }

        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Gets or sets strings to show user
        /// </summary>
        public ObservableCollection<string> NotifyUser { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// True if currently settings the error message
        /// </summary>
        private bool settingErrorMessage = false;

        /// <summary>
        /// Source for <see cref="Characteristic"/>
        /// </summary>
        private ObservableGattCharacteristics characteristic = GattSampleContext.Context.SelectedCharacteristic;

        /// <summary>
        /// Gets or sets the characteristic that this view model wraps
        /// </summary>
        public ObservableGattCharacteristics Characteristic
        {
            get
            {
                return characteristic;
            }

            set
            {
                Set(ref characteristic, value, "Characteristic");
            }
        }

        /// <summary>
        /// Source for <see cref="SelectedDescriptor"/>
        /// </summary>
        private ObservableGattDescriptors selectedDescriptor;

        /// <summary>
        /// Gets or sets the currently selected descriptor
        /// </summary>
        public ObservableGattDescriptors SelectedDescriptor
        {
            get
            {
                return selectedDescriptor;
            }

            set
            {
                Set(ref selectedDescriptor, value, "SelectedDescriptor");
                context.SelectedDescriptor = SelectedDescriptor;
                //NavigationService.Navigate(typeof(DescriptorPage));
            }
        }

        /// <summary>
        /// Source for <see cref="Properties"/>
        /// </summary>
        private string properties = "None";

        /// <summary>
        /// Gets the string showing what properties are supported
        /// </summary>
        public string Properties
        {
            get
            {
                return properties;
            }

            private set
            {
                Set(ref properties, value);
            }
        }
        
        /// <summary>
        /// Source for <see cref="Notify"/>
        /// </summary>
        private bool notify = false;

        /// <summary>
        /// Gets or sets a value indicating whether notify is supported by this characteristic
        /// </summary>
        public bool Notify
        {
            get
            {
                return notify;
            }

            set
            {
                if (notify == value)
                {
                    return;
                }

                // The heavy lifting for writting the CCCD is done in the PropertyChanged method
                // in this class that gets called when this property is actually changed.

                Set(ref notify, value);
            }
        }

        /// <summary>
        /// Source for <see cref="NotifyProgress"/>
        /// </summary>
        private bool notifyProgress = false;

        /// <summary>
        /// Gets or sets a value indicating whether the progress ring should be displayed 
        /// while the notify descriptor is written
        /// </summary>
        public bool NotifyProgress
        {
            get
            {
                return notifyProgress;
            }

            set
            {
                if (notifyProgress == value)
                {
                    return;
                }

                Set(ref notifyProgress, value);
            }
        }

        /// <summary>
        /// Source for <see cref="NotifyError"/>
        /// </summary>
        private bool notifyError = false;

        /// <summary>
        /// Gets a value indicating whether there was an error setting the notify descriptor
        /// </summary>
        public bool NotifyError
        {
            get
            {
                return notifyError;
            }

            private set
            {
                if (notifyError == value)
                {
                    return;
                }

                Set(ref notifyError, value);
            }
        }

        /// <summary>
        /// Source for <see cref="Indicate"/>
        /// </summary>
        private bool indicate = false;

        /// <summary>
        /// Gets or sets a value indicating whether indicate is supported by this characteristic
        /// </summary>
        public bool Indicate
        {
            get
            {
                return indicate;
            }

            set
            {
                if (indicate == value)
                {
                    return;
                }

                // The heavy lifting for writting the CCCD is done in the PropertyChanged method
                // in this class that gets called when this property is actually changed.

                Set(ref indicate, value);
            }
        }

        /// <summary>
        /// Source for <see cref="IndicateProgress"/>
        /// </summary>
        private bool indicateProgress = false;

        /// <summary>
        /// Gets or sets a value indicating whether the progress ring should be displayed 
        /// while the indicate descriptor is written
        /// </summary>
        public bool IndicateProgress
        {
            get
            {
                return indicateProgress;
            }

            set
            {
                if (indicateProgress == value)
                {
                    return;
                }

                Set(ref indicateProgress, value);
            }
        }

        /// <summary>
        /// Source for <see cref="IndicateError"/>
        /// </summary>
        private bool indicateError = false;

        /// <summary>
        /// Gets a value indicating whether there was an error setting the indicate descriptor
        /// </summary>
        public bool IndicateError
        {
            get
            {
                return indicateError;
            }

            private set
            {
                if (indicateError == value)
                {
                    return;
                }

                Set(ref indicateError, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can be read
        /// </summary>
        public bool CharacteristicCanBeRead
        {
            get
            {
                return Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can be written to
        /// </summary>
        public bool CharacteristicCanWrite
        {
            get
            {
                // Windows blocks writing on certain characteristics
                if (GattServiceUuidHelper.IsReadOnly(Characteristic.Characteristic.Service.Uuid))
                {
                    return false;
                }

                return ( Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write) ||
                         Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can notify
        /// </summary>
        public bool CharacteristicCanNotify
        {
            get
            {
                return Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can indicate
        /// </summary>
        public bool CharacteristicCanIndicate
        {
            get
            {
                return Characteristic.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can notify or indicate
        /// </summary>
        public bool CharacteristicCanNotifyOrIndicate
        {
            get
            {
                return CharacteristicCanNotify | CharacteristicCanIndicate;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value text box should be shown
        /// </summary>
        public bool CharacteristicValueVisible
        {
            get
            {
                return CharacteristicCanNotify | CharacteristicCanIndicate | CharacteristicCanBeRead;
            }
        }

        /// <summary>
        /// Source for <see cref="ValueToWrite"/>
        /// </summary>
        private string valueToWrite = String.Empty;
        
        /// <summary>
        /// Gets or sets the value to write to server
        /// </summary>
        public string ValueToWrite
        {
            get
            {
                return valueToWrite;
            }

            set
            {
                Set(ref valueToWrite, value);
            }
        }

        /// <summary>
        /// Source for <see cref="WriteType"/>
        /// </summary>
        private WriteTypes writeType = WriteTypes.Hex;

        /// <summary>
        /// Gets or sets how the value should be written to server
        /// </summary>
        public WriteTypes WriteType
        {
            get
            {
                return writeType;
            }

            set
            {
                Set(ref writeType, value);
            }
        }

        /// <summary>
        /// Source for <see cref="UseWindowsNotifications"/>
        /// </summary>
        private bool useWindowsNotifications = false;

        /// <summary>
        /// Gets or sets a value indicating whether windows notifications should be used
        /// </summary>
        public bool UseWindowsNotifications
        {
            get
            {
                return useWindowsNotifications;
            }

            set
            {
                Set(ref useWindowsNotifications, value);
            }
        }

        /// <summary>
        /// Gets the name of the device
        /// </summary>
        public string DeviceName
        {
            get
            {
                return context.SelectedBluetoothLEDevice.Name;
            }
        }

        /// <summary>
        /// Source for <see cref="DisplayPresentError"/>
        /// </summary>
        private Windows.UI.Xaml.Visibility displayPresentError = Windows.UI.Xaml.Visibility.Visible;

        /// <summary>
        /// Gets value to show or hide error text depending on <see cref="ObservableGattCharacteristics.DisplayTypes"/> 
        /// </summary>
        public Windows.UI.Xaml.Visibility DisplayPresentError
        {
            get
            {
                return displayPresentError;
            }

            private set
            {
                Set(ref displayPresentError, value);
            }
        }

        public bool IsTransactionInProgress
        {
            get
            {
                return context.IsTransactionInProgress;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicPageViewModel" /> class.
        /// </summary>
        public CharacteristicPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            Notify = Characteristic.IsNotifySet;
            Indicate = Characteristic.IsIndicateSet;

            this.PropertyChanged += CharacteristicPageViewModel_PropertyChanged;
            Characteristic.PropertyChanged += Characteristic_PropertyChanged;

            if (Characteristic.DisplayType == ObservableGattCharacteristics.DisplayTypes.Unsupported)
            {
                DisplayPresentError = Windows.UI.Xaml.Visibility.Visible;
            }

            context.PropertyChanged += Context_PropertyChanged;
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsTransactionInProgress")
            {
                this.RaisePropertyChanged("IsTransactionInProgress");
            }
        }

        /// <summary>
        /// Characteristic changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Characteristic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                if (UseWindowsNotifications == true && (Notify == true || Indicate == true))
                {
                    BluetoothLEExplorer.Services.ToastService.ToastService.PopToast(Characteristic.Name, Characteristic.Value, "Notification", "Notification");
                }
            }

            if (e.PropertyName == "DisplayType")
            {
                if (Characteristic.DisplayType == ObservableGattCharacteristics.DisplayTypes.Unsupported)
                {
                    DisplayPresentError = Windows.UI.Xaml.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Characteristic view model changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CharacteristicPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Notify")
            {
                if (Notify == true)
                {
                    NotifyProgress = true;
                    bool success = await Characteristic.SetNotify();
                    NotifyProgress = false;
                    if (success == true)
                    {
                        if (settingErrorMessage == true)
                        {
                            settingErrorMessage = false;
                        }
                        else
                        {
                            NotifyError = false;
                        }
                    }
                    else
                    {
                        settingErrorMessage = true;
                        Notify = false;
                        NotifyError = true;
                    }
                }
                else
                {
                    NotifyProgress = true;
                    bool success = await Characteristic.StopNotify();
                    NotifyProgress = false;
                    if (success == true)
                    {
                        if (settingErrorMessage == true)
                        {
                            settingErrorMessage = false;
                        }
                        else
                        {
                            NotifyError = false;
                        }
                    }
                    else
                    {
                        settingErrorMessage = true;
                        NotifyError = true;
                        Notify = true;
                    }
                }
            }

            if (e.PropertyName == "Indicate")
            {
                if (Indicate == true)
                {
                    IndicateProgress = true;
                    bool success = await Characteristic.SetIndicate();
                    IndicateProgress = false;
                    if (success == true)
                    {
                        IndicateError = false;
                    }
                    else
                    {
                        Indicate = false;
                        IndicateError = true;
                    }
                }
                else
                {
                    IndicateProgress = true;
                    bool success = await Characteristic.StopIndicate();
                    IndicateProgress = false;

                    if (success == true)
                    {
                        IndicateError = false;
                    }
                    else
                    {
                        IndicateError = true;
                        Indicate = true;
                    }
                }
            }
        }

        /// <summary>
        /// Write the value to the server
        /// </summary>
        public async void WriteValue()
        {
            if (!String.IsNullOrEmpty(ValueToWrite))
            {
                IBuffer writeBuffer = null;

                if (WriteType == WriteTypes.Decimal)
                {
                    DataWriter writer = new DataWriter();
                    writer.ByteOrder = ByteOrder.LittleEndian;
                    writer.WriteInt32(Int32.Parse(ValueToWrite));
                    writeBuffer = writer.DetachBuffer();
                }
                else if (WriteType == WriteTypes.Hex)
                {
                    try
                    {
                        // pad the value if we've received odd number of bytes
                        if (ValueToWrite.Length % 2 == 1)
                        {
                            writeBuffer = GattConvert.ToIBufferFromHexString("0" + ValueToWrite);
                        }
                        else
                        {
                            writeBuffer = GattConvert.ToIBufferFromHexString(ValueToWrite);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageDialog dialog = new MessageDialog(ex.Message, "Error");
                        await dialog.ShowAsync();
                        return;
                    }

                }
                else if (WriteType == WriteTypes.UTF8)
                {
                    writeBuffer = CryptographicBuffer.ConvertStringToBinary(ValueToWrite,
                    BinaryStringEncoding.Utf8);
                }

                try
                {
                    // BT_Code: Writes the value from the buffer to the characteristic.
                    GattCommunicationStatus result = await Characteristic.Characteristic.WriteValueAsync(writeBuffer);

                    if (result == GattCommunicationStatus.Unreachable)
                    {
                        NotifyUser.Insert(0, "Unable to write data - Device unreachable");
                    }
                    else if (result == GattCommunicationStatus.ProtocolError)
                    {
                        NotifyUser.Insert(0, "Unable to write data - Protocol error");
                    }
                    ValueToWrite = String.Empty;
                }
                catch (Exception ex) when ((uint)ex.HResult == 0x80650003 || (uint)ex.HResult == 0x80070005)
                {
                    // E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED or E_ACCESSDENIED
                    // This usually happens when a device reports that it support writing, but it actually doesn't.
                    NotifyUser.Insert(0, "Error writing to characteristic. This usually happens when a device reports that it support writing, but it actually doesn't.");
                }
                catch(Exception ex)
                {
                    MessageDialog dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                NotifyUser.Insert(0, "No data to write to device");
            }
        }

        public async void WriteTransaction()
        {
            if (!String.IsNullOrEmpty(ValueToWrite))
            {
                IBuffer writeBuffer = null;

                if (WriteType == WriteTypes.Decimal)
                {
                    DataWriter writer = new DataWriter();
                    writer.ByteOrder = ByteOrder.LittleEndian;
                    writer.WriteInt32(Int32.Parse(ValueToWrite));
                    writeBuffer = writer.DetachBuffer();
                }
                else if (WriteType == WriteTypes.Hex)
                {
                    try
                    {
                        // pad the value if we've received odd number of bytes
                        if (ValueToWrite.Length % 2 == 1)
                        {
                            writeBuffer = GattConvert.ToIBufferFromHexString("0" + ValueToWrite);
                        }
                        else
                        {
                            writeBuffer = GattConvert.ToIBufferFromHexString(ValueToWrite);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageDialog dialog = new MessageDialog(ex.Message, "Error");
                        await dialog.ShowAsync();
                        return;
                    }

                }
                else if (WriteType == WriteTypes.UTF8)
                {
                    writeBuffer = CryptographicBuffer.ConvertStringToBinary(ValueToWrite,
                    BinaryStringEncoding.Utf8);
                }

                context.WriteTransaction(Characteristic.Characteristic, writeBuffer);
            }
            else
            {
                NotifyUser.Insert(0, "No data to write to device");
            }
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
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, 
                () =>
            {
                
                if(Services.SettingsServices.SettingsService.Instance.SettingsDictionary.Keys.Contains(Characteristic.UUID + "-UseWindowsNotification"))
                {
                    UseWindowsNotifications = (bool)Services.SettingsServices.SettingsService.Instance.SettingsDictionary[Characteristic.UUID + "-UseWindowsNotification"];
                }
            });

            if (Characteristic.Characteristic.CharacteristicProperties != GattCharacteristicProperties.None)
            {
                StringBuilder sb = new StringBuilder();
                bool first = true;

                foreach (GattCharacteristicProperties p in Enum.GetValues(typeof(GattCharacteristicProperties)))
                {
                    if (p == GattCharacteristicProperties.None)
                    {
                        continue;
                    }

                    if (GattServiceUuidHelper.IsReadOnly(Characteristic.Characteristic.Service.Uuid) &&
                        (p == GattCharacteristicProperties.Write || p == GattCharacteristicProperties.WriteWithoutResponse))
                    {
                        continue;
                    }

                    if (Characteristic.Characteristic.CharacteristicProperties.HasFlag(p))
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        sb.Append(Enum.GetName(typeof(GattCharacteristicProperties), p));
                    }
                }

                Properties = sb.ToString();
            }
        }
        
        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns>Navigate task</returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
            }

            Services.SettingsServices.SettingsService.Instance.SettingsDictionary[Characteristic.UUID + "-UseWindowsNotification"] = useWindowsNotifications;

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
            await Task.CompletedTask;
        }
    }
}
