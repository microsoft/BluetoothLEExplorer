// <copyright file="VirtualPeripheralPageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the Virtual Peripheral Page
    /// </summary>
    public class VirtualPeripheralPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the name of the available services
        /// </summary>
        public List<string> AvailableServices { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the name of the newly selected service
        /// </summary>
        public string NewSelectedService { get; set; } = String.Empty;

        /// <summary>
        /// Source for <see cref="SelectedService"/>
        /// </summary>
        private GenericGattServiceViewModel selectedService;

        /// <summary>
        /// Gets or sets the currently selected service
        /// </summary>
        public GenericGattServiceViewModel SelectedService
        {
            get
            {
                return selectedService;
            }

            set
            {
                if (selectedService != value && value != null)
                {
                    Set(ref selectedService, value, "SelectedService");
                    Context.SelectedGattServerService = selectedService;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this host supports peripheral role
        /// </summary>
        public bool IsPeripheralRoleSupported
        {
            get
            {
                return GattSampleContext.Context.IsPeripheralRoleSupported;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this host supports central role
        /// </summary>
        public bool IsCentralRoleSupported
        {
            get
            {
                return Context.IsCentralRoleSupported;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPeripheralPageViewModel" /> class.
        /// </summary>
        public VirtualPeripheralPageViewModel()
        {
            Context.PropertyChanged += Context_PropertyChanged;
            AvailableServices.Add("Alert Notification Service");
            AvailableServices.Add("Current Time Service");
            AvailableServices.Add("Battery Service");
            AvailableServices.Add("Microsoft Service");
            AvailableServices.Add("Heart Rate Service");
            AvailableServices.Add("Blood Pressure Service");
        }

        /// <summary>
        /// Callback when context changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsPeripheralRoleSupported")
            {
                this.RaisePropertyChanged("IsPeripheralRoleSupported");
            }

            if (e.PropertyName == "IsCentralRoleSupported")
            {
                RaisePropertyChanged("IsCentralRoleSupported");
            }
        }

        /// <summary>
        /// Gets the context
        /// </summary>
        public GattSampleContext Context { get; } = GattSampleContext.Context;

        /// <summary>
        /// Creates the service that was selected
        /// </summary>
        public async void CreateService()
        {
            GattServicesLibrary.GenericGattService service = null;

            if (NewSelectedService == null)
            {
                return;
            }

            Views.Busy.SetBusy(true, "Creating Service");

            switch(NewSelectedService)
            {
                case "Alert Notification Service":
                    service = new GattServicesLibrary.Services.AlertNotificationService();
                    break;

                case "Current Time Service":
                    service = new GattServicesLibrary.Services.CurrentTimeService();
                    break;

                case "Battery Service":
                    service = new GattServicesLibrary.Services.BatteryService();
                    break;

                case "Microsoft Service":
                    service = new GattServicesLibrary.Services.MicrosoftService();
                    break;

                case "Heart Rate Service":
                    service = new GattServicesLibrary.Services.HeartRateService();
                    break;

                case "Blood Pressure Service":
                    service = new GattServicesLibrary.Services.BloodPressureService();
                    break;

                default:
                    return;
            }
            
            await service.Init();
            GenericGattServiceViewModel serviceVM = new GenericGattServiceViewModel(service);
            Context.CreatedServices.Add(serviceVM);
            Context.SelectedGattServerService = serviceVM;
            NavigateToService();
            return;
        }

        /// <summary>
        /// Initializes <see cref="NavigateToService"/> 
        /// </summary>
        public void NavigateToService()
        {
            if (Context.SelectedGattServerService == null)
            {
                Views.Busy.SetBusy(false);
                return;
            }

            switch(Context.SelectedGattServerService.Service.Name)
            {
                case "Alert Notification Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.AlertNotificationServicePage));
                    break;

                case "Current Time Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.CurrentTimeServicePage));
                    break;

                case "Battery Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.BatteryServicePage));
                    break;

                case "Microsoft Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.MicrosoftServicePage));
                    break;

                case "Heart Rate Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.HeartRateServicePage));
                    break;

                case "Blood Pressure Service":
                    Views.Busy.SetBusy(false);
                    NavigationService.Navigate(typeof(Views.BloodPressureServicePage));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Navigating to page
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns>Navigate to task</returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
            }

            Context.SelectedGattServerService = null;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Navigating from page
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Navigate from task</returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Go to settings
        /// </summary>
        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        /// <summary>
        /// Go to private
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
