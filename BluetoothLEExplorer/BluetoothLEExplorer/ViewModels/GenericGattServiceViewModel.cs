// <copyright file="GenericGattServiceViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using BluetoothLEExplorer.Models;
using GattServicesLibrary;
using Template10.Mvvm;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model used to display a <see cref="GenericGattService"/> 
    /// </summary>
    public class GenericGattServiceViewModel : ViewModelBase
    {
        /// <summary>
        /// Source for <see cref="Service"/> property
        /// </summary>
        private GenericGattService service = null;

        /// <summary>
        /// Gets the service for this view model
        /// </summary>
        public GenericGattService Service
        {
            get
            {
                return service;
            }

            private set
            {
                Set(ref service, value, "Service");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Advertisement status is disconverable.
        /// </summary>
        public bool IsConnectable
        {
            get
            {
                return service.IsConnectable;
            }

            set
            {
                if (value != service.IsConnectable)
                {
                    service.IsConnectable = value;
                    RaisePropertyChanged("IsConnectable");
                }

            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Advertisement status is disconverable.
        /// </summary>
        public bool IsDiscoverable
        {
            get
            {
                return service.IsDiscoverable;
            }

            set
            {
                if (value != service.IsDiscoverable)
                {
                    service.IsDiscoverable = value;
                    RaisePropertyChanged("IsDiscoverable");
                }

            }
        }

        public bool IncludeServiceData
        {
            get
            {
                return service.IncludeServiceData;
            }

            set
            {
                if (value != service.IncludeServiceData)
                {
                    service.IncludeServiceData = value;
                    RaisePropertyChanged("IncludeServiceData");
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsPublishing"/> property
        /// </summary>
        private bool isPublishing = false;

        /// <summary>
        /// Gets or sets a value indicating whether the Advertisement status is published or not
        /// </summary>
        public bool IsPublishing
        {
            get
            {
                return isPublishing;
            }

            set
            {
                if (value != isPublishing)
                {
                    if (value == true)
                    {
                        Start();
                    }
                    if(value == false)
                    {
                        Stop();
                    }

                    isPublishing = value;
                    RaisePropertyChanged("IsPublishing");
                }
                
            }
        }

        /// <summary>
        /// Starts the service
        /// </summary>
        public async void Start()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, 
                () =>
            {
                Service.Start();
            });
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        public async void Stop()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                Service.Stop();
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGattServiceViewModel" /> class.
        /// </summary>
        /// <param name="service"></param>
        public GenericGattServiceViewModel(GenericGattService service)
        {
            Service = service;
            Service.PropertyChanged += Service_PropertyChanged;
        }

        private void Service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Track the IsPublishing property
            IsPublishing = Service.IsPublishing;
        }

        /// <summary>
        /// Removes this service from context
        /// </summary>
        public void RemoveThisFromContext()
        {
            GattSampleContext.Context.CreatedServices.Remove(this);
        }
    }
}
