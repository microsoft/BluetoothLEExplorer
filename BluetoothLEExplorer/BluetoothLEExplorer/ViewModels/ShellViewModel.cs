// <copyright file="ShellViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using BluetoothLEExplorer.Models;
using Template10.Mvvm;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the shell
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        /// <summary>
        /// App context
        /// </summary>
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Gets a value indicating whether this host supports peripheral role
        /// </summary>
        public bool IsPeripheralRoleSupported
        {
            get
            {
                return context.IsPeripheralRoleSupported;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this host supports central role
        /// </summary>
        public bool IsCentralRoleSupported
        {
            get
            {
                return context.IsCentralRoleSupported;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel" /> class.
        /// </summary>
        public ShellViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }

            context.PropertyChanged += Context_PropertyChanged;
        }

        /// <summary>
        /// Property changed
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
    }
}
