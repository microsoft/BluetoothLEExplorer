// <copyright file="HeartRateServicePageViewModel.cs" company="Microsoft Corporation">
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
    /// View Model for the <see cref="HeartRateServicePageViewModel"/> view
    /// </summary>
    public class HeartRateServicePageViewModel : ViewModelBase
    {
        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Gets or sets the currently selected service view model
        /// </summary>
        public GenericGattServiceViewModel ServiceVM { get; set; } = GattSampleContext.Context.SelectedGattServerService;

        /// <summary>
        /// Gets or sets the currently selected service
        /// </summary>
        public HeartRateService Service { get; set; } = GattSampleContext.Context.SelectedGattServerService.Service as HeartRateService;

        /// <summary>
        /// Gets the heart rate measurement
        /// </summary>
        public GenericGattCharacteristicViewModel HeartRateMeasurement { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateServicePageViewModel" /> class.
        /// </summary>
        public HeartRateServicePageViewModel()
        {
            HeartRateMeasurement = new GenericGattCharacteristicViewModel(Service.HeartRateMeasurement);
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
