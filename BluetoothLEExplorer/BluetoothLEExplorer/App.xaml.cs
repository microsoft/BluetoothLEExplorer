// <copyright file="App.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.SettingsServices;
using Template10.Controls;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Popups;
using BluetoothLEExplorer.Models;
using BluetoothLEExplorer.ViewModels;
using System.Diagnostics;

namespace BluetoothLEExplorer
{
    //// Documentation on APIs used in this page: 
    //// https://github.com/Windows-XAML/Template10/wiki

    /// <summary>
    /// The application
    /// </summary>
    [Bindable]
    public sealed partial class App : Template10.Common.BootStrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
            this.UnhandledException += App_UnhandledException;

            this.Suspending += App_Suspending;
            this.Resuming += App_Resuming;

            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;

            #endregion
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            try
            {
                var deferral = e.SuspendingOperation.GetDeferral();
 
                foreach(GenericGattServiceViewModel service in GattSampleContext.Context.CreatedServices)
                {
                    string key = "Service_"+ service.Service.ServiceProvider.Service.Uuid.ToString() + "_IsPublishing";
                    bool value = service.IsPublishing;
                    SettingsService.Instance.SettingsDictionary[key] = value;

                    if (service.IsPublishing)
                    {
                        service.Service.ServiceProvider.StopAdvertising();
                    }
                }

                GattSampleContext.Context.ReleaseAllResources();

                deferral.Complete();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Suspending: " + ex.Message);
            }
        }

        private void App_Resuming(object sender, object e)
        {
            string[] keys = new string[SettingsService.Instance.SettingsDictionary.Keys.Count];

            SettingsService.Instance.SettingsDictionary.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].Contains("Service_"))
                {
                    string serviceUUID = keys[i].Split('_')[1];
                    bool IsPublishing = (bool)SettingsService.Instance.SettingsDictionary[keys[i]];

                    if (IsPublishing)
                    {
                        foreach (GenericGattServiceViewModel service in GattSampleContext.Context.CreatedServices)
                        {
                            if (serviceUUID == service.Service.ServiceProvider.Service.Uuid.ToString())
                            {
                                service.Start();
                            }
                        }
                    }
                }
            }
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            showDialog(e.Exception.Message + "\n\n" + e.Exception.StackTrace);
        }

        private async void showDialog(string content)
        {
            MessageDialog dialog = new MessageDialog(content, "Fatal Error");
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Application initialization
        /// </summary>
        /// <param name="args"></param>
        /// <returns>On initialization task</returns>
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.Busy(),
                };
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// App initialization for long running tasks
        /// </summary>
        /// <param name="startKind"></param>
        /// <param name="args"></param>
        /// <returns>On start task</returns>
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            //// long-running startup tasks go here

            NavigationService.Navigate(typeof(Views.Discover));
            await Task.CompletedTask;
        }
    }
}
