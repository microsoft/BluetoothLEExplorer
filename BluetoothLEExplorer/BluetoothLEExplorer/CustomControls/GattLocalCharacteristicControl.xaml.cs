// <copyright file="GattLocalCharacteristicControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Text;
using BluetoothLEExplorer.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using GattHelper.Converters;
using Windows.UI.Popups;

//// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BluetoothLEExplorer.CustomControls
{
    /// <summary>
    /// UserControl to display a Characteristic
    /// </summary>
    public sealed partial class GattLocalCharacteristicControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Callback when characteristic changes
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnCharacteristicPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GattLocalCharacteristicControl control = d as GattLocalCharacteristicControl;
            control.Characteristic = e.NewValue as GenericGattCharacteristicViewModel;
            control.Value = GattServicesLibrary.Helpers.ValueConverter.ConvertGattCharacteristicValueToString(control.Characteristic.Characteristic);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GattLocalCharacteristicControl" /> class.
        /// </summary>
        public GattLocalCharacteristicControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Executes when the characteristic changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Characteristic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, 
                () =>
            {
                if (e.PropertyName == "Value")
                {
                    Value = GattServicesLibrary.Helpers.ValueConverter.ConvertGattCharacteristicValueToString(Characteristic.Characteristic);
                }
            });
        }

        /// <summary>
        /// Source for the <see cref="Value"/> property
        /// </summary>
        private string value;

        /// <summary>
        /// Gets or sets the value of this characteristic
        /// </summary>
        public string Value
        {
            get
            {
                return value;
            }

            set
            {
                if (value != this.value)
                {
                    // If this is a read only control then no need to send it back to the base characteristic
                    if (this.IsReadOnly)
                    {
                        this.value = value;
                    }
                    else
                    {
                        try
                        {
                            Characteristic.SetValueFromString(value);
                            this.value = value;
                        }
                        catch (Exception ex)
                        {
                            ErrorDialog(ex.Message);
                        }
                    }
                    OnPropertyChanged(new PropertyChangedEventArgs("Value"));
                }
            }
        }

        async void ErrorDialog(string message)
        {
            MessageDialog err = new MessageDialog(message, "Error");
            await err.ShowAsync();
            return;
        }

        /// <summary>
        /// Gets the visibility of the the notify button
        /// </summary>
        private Visibility NotifyButtonVisibility
        {
            get
            {
                if (Characteristic.HasNotifyDescriptor == true || Characteristic.HasIndicateDescriptor == true)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Gets the text for the notify button
        /// </summary>
        private string NotifyButtonText
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                if (Characteristic.HasNotifyDescriptor)
                {
                    ret.Append("Notify");
                    if (Characteristic.HasIndicateDescriptor)
                    {
                        ret.Append("/Indicate");
                    }
                }
                else if (Characteristic.HasIndicateDescriptor)
                {
                    ret.Append("Indicate");
                }

                return ret.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the characteristic this control wraps
        /// </summary>
        public GenericGattCharacteristicViewModel Characteristic
        {
            get
            {
                return (GenericGattCharacteristicViewModel)GetValue(CharacteristicProperty);
            }

            set
            {
                SetValue(CharacteristicProperty, value);
                
                Characteristic.Characteristic.PropertyChanged += Characteristic_PropertyChanged;
            }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Characteristic.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CharacteristicProperty =
            DependencyProperty.Register(
                "Characteristic", 
                typeof(GenericGattCharacteristicViewModel), 
                typeof(GattLocalCharacteristicControl), 
                new PropertyMetadata(
                    null,
                    new PropertyChangedCallback(OnCharacteristicPropertyChanged)));

        /// <summary>
        /// Gets or sets the visibility 
        /// </summary>
        public Visibility ShowValue
        {
            get { return (Visibility)GetValue(ShowValueProperty); }
            set { SetValue(ShowValueProperty, Visibility); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowValue.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ShowValueProperty =
            DependencyProperty.Register("ShowValue", typeof(Visibility), typeof(GattLocalCharacteristicControl), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets the readonly field of the value text box 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }

            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsReadOnly
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(GattLocalCharacteristicControl), new PropertyMetadata(true));


        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed method
        /// </summary>
        /// <param name="e">Property that changed</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
