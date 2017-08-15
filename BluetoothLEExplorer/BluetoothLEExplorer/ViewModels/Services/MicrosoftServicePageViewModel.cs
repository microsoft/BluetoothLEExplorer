// <copyright file="MicrosoftServicePageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using GattServicesLibrary.Services;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the <see cref="MicrosoftService"/>  view
    /// </summary>
    public class MicrosoftServicePageViewModel : ViewModelBase
    {
        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Gets or sets the selected service view model
        /// </summary>
        public GenericGattServiceViewModel ServiceVM { get; set; } = GattSampleContext.Context.SelectedGattServerService;

        /// <summary>
        /// Gets or sets the selected service
        /// </summary>
        public MicrosoftService Service { get; set; } = GattSampleContext.Context.SelectedGattServerService.Service as MicrosoftService;

        /// <summary>
        /// Gets the read Characteristic
        /// </summary>
        public GenericGattCharacteristicViewModel ReadCharacteristic { get; private set; }

        /// <summary>
        /// Gets write Characteristic
        /// </summary>
        public GenericGattCharacteristicViewModel WriteCharacteristic { get; private set; }

        /// <summary>
        /// Gets notify Characteristic
        /// </summary>
        public GenericGattCharacteristicViewModel NotifyCharacteristic { get; private set; }

        /// <summary>
        /// Gets indicate Characteristic
        /// </summary>
        public GenericGattCharacteristicViewModel IndicateCharacteristic { get; private set; }

        /// <summary>
        /// Gets the read long characteristic
        /// </summary>
        public GenericGattCharacteristicViewModel ReadLongCharacteristic { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftServicePageViewModel" /> class.
        /// </summary>
        public MicrosoftServicePageViewModel()
        {
            ReadCharacteristic = new GenericGattCharacteristicViewModel(Service.ReadCharacteristic);
            WriteCharacteristic = new GenericGattCharacteristicViewModel(Service.WriteCharacteristic);
            NotifyCharacteristic = new GenericGattCharacteristicViewModel(Service.NotifyCharacteristic);
            IndicateCharacteristic = new GenericGattCharacteristicViewModel(Service.IndicateCharacteristic);
            ReadLongCharacteristic = new GenericGattCharacteristicViewModel(Service.ReadLongCharacteristic);
        }

        /// <summary>
        /// Navigate from page
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Navigate from task</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            context.SelectedGattServerService = null;
            args.Cancel = false;
            await Task.CompletedTask;
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
