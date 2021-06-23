// <copyright file="ServicePageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using BluetoothLEExplorer.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for Device Services View
    /// </summary>
    public class ServicePageViewModel : ViewModelBase
    {
        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Gets the currently selected bluetooth service
        /// </summary>
        public ObservableGattDeviceService SelectedService { get; set; } = GattSampleContext.Context.SelectedService;

        /// <summary>
        /// Gets the currently selected bluetooth device
        /// </summary>
        public ObservableBluetoothLEDevice SelectedDevice { get; private set; } = GattSampleContext.Context.SelectedBluetoothLEDevice;

        /// <summary>
        /// Source for <see cref="SelectedCharacteristic"/>
        /// </summary>
        private ObservableGattCharacteristics selectedCharacteristic;

        /// <summary>
        /// Gets or sets the currently selected characteristic
        /// </summary>
        public ObservableGattCharacteristics SelectedCharacteristic
        {
            get
            {
                return selectedCharacteristic;
            }

            set
            {
                Set(ref selectedCharacteristic, value, "SelectedCharacteristic");
                context.SelectedCharacteristic = SelectedCharacteristic;
                NavigationService.Navigate(typeof(CharacteristicPage));
            }
        }

        /// <summary>
        /// Source for <see cref="ErrorText"/>
        /// </summary>
        private string errorText = String.Empty;

        /// <summary>
        /// Gets the error text to display
        /// </summary>
        public string ErrorText
        {
            get
            {
                return errorText;
            }

            private set
            {
                Set(ref errorText, value, "ErrorText");
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
        /// Gets a value indicating whether this service has a characteristic that can notify
        /// </summary>
        public bool ServiceCanNotify
        {
            get
            {
                return context.SelectedService.CanNotify();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this characteristic can indicate
        /// </summary>
        public bool ServiceCanIndicate
        {
            get
            {
                return context.SelectedService.CanIndicate();
            }
        }

        /// <summary>
        /// Service view model changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ServicePageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Notify")
            {
                if (Notify == true)
                {
                    NotifyProgress = true;
                    bool success = await SelectedService.TurnOnAllNotifications();
                    NotifyProgress = false;
                    if (success == true)
                    {
                        NotifyError = false;
                    }
                    else
                    {
                        Notify = false;
                        NotifyError = true;
                    }
                }
                else
                {
                    NotifyProgress = true;
                    bool success = await SelectedService.TurnOffAllNotifications();
                    NotifyProgress = false;
                    if (success == true)
                    {
                        NotifyError = false;
                    }
                    else
                    {
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
                    bool success = await SelectedService.TurnOnAllIndications();
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
                    bool success = await SelectedService.TurnOffAllNotifications();
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
        /// Initializes a new instance of the <see cref="DeviceServicesPageViewModel" /> class.
        /// </summary>
        public ServicePageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            this.PropertyChanged += ServicePageViewModel_PropertyChanged;

            Notify = SelectedService.IsNotifySet();
            Indicate = SelectedService.IsIndicateSet();
        }

        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns>Navigate from Task</returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                });

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

            await Task.CompletedTask;
        }

        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Navigate from Task</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
    }
}