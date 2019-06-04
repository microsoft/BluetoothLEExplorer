// <copyright file="VirtualKeyboardPageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.Devices.Bluetooth;

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the Virtual Keyboard Page
    /// </summary>
    public class VirtualKeyboardPageViewModel : ViewModelBase
    {
        private bool m_isSettingUp = true;
        private VirtualKeyboard m_virtualKeyboard;
        private bool m_isKeyboardEnabled = false;

        private object m_subscribedHidClientsLock = new object();
        private ObservableCollection<ObservableGattClient> m_subscribedHidClients = new ObservableCollection<ObservableGattClient>();

        public ObservableCollection<ObservableGattClient> SubscribedGattClients
        {
            get
            {
                lock (m_subscribedHidClientsLock)
                {
                    return m_subscribedHidClients;
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

        public bool IsSettingUp
        {
            get
            {
                return m_isSettingUp;
            }
            private set
            {
                m_isSettingUp = value;
                RaisePropertyChanged("IsSettingUp");
            }
        }

        public bool IsKeyboardEnabled
        {
            get
            {
                return m_isKeyboardEnabled;
            }
            set
            {
                if (value)
                {
                    m_virtualKeyboard.Enable();
                }
                else
                {
                    m_virtualKeyboard.Disable();
                }
                m_isKeyboardEnabled = value;
            }
        }

        private string errorText;

        public string ErrorText
        {
            get
            {
                return errorText;
            }
            private set
            {
                errorText = value;
                RaisePropertyChanged("ErrorText");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualKeyboardPageViewModel" /> class.
        /// </summary>
        public VirtualKeyboardPageViewModel()
        {
            Context.PropertyChanged += Context_PropertyChanged;

            var coreWindow = CoreWindow.GetForCurrentThread();

            coreWindow.KeyDown += CoreWindow_KeyDown;
            coreWindow.KeyUp += CoreWindow_KeyUp;

            InitializeVirtualKeyboard();
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
        public async void InitializeVirtualKeyboard()
        {
            try
            {
                m_virtualKeyboard = new VirtualKeyboard();
                m_virtualKeyboard.SubscribedHidClientsChanged += VirtualKeyboard_SubscribedHidClientsChanged;
                await m_virtualKeyboard.InitiliazeAsync();

                // Done setting up.
                IsSettingUp = false;
            }
            catch (Exception e)
            {
                ErrorText = e.ToString();
            }
        }

        private async void VirtualKeyboard_SubscribedHidClientsChanged(IReadOnlyList<Windows.Devices.Bluetooth.GenericAttributeProfile.GattSubscribedClient> subscribedClients)
        {
            ObservableCollection<ObservableGattClient> currentHidClients = new ObservableCollection<ObservableGattClient>();
            if (subscribedClients != null)
            {
                foreach (var client in subscribedClients)
                {
                    currentHidClients.Add(await ObservableGattClient.FromIdAsync(client.Session.DeviceId.Id));
                }
            }

            lock (m_subscribedHidClientsLock)
            {
                m_subscribedHidClients = currentHidClients;
            }
            RaisePropertyChanged("SubscribedGattClients");
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

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!args.KeyStatus.WasKeyDown)
            {
                m_virtualKeyboard.PressKey(GetPs2Set1ScanCodeFromStatus(args.KeyStatus));
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.KeyStatus.IsKeyReleased)
            {
                m_virtualKeyboard.ReleaseKey(GetPs2Set1ScanCodeFromStatus(args.KeyStatus));
            }
        }

        private uint GetPs2Set1ScanCodeFromStatus(CorePhysicalKeyStatus keyStatus)
        {
            if (keyStatus.IsExtendedKey)
            {
                return (0xE000 | keyStatus.ScanCode);
            }
            else
            {
                return keyStatus.ScanCode;
            }
        }
    }
}
