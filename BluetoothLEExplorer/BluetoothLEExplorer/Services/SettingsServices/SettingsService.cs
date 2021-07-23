// <copyright file="SettingsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;

namespace BluetoothLEExplorer.Services.SettingsServices
{
    /// <summary>
    /// Settings helper service
    /// </summary>
    public class SettingsService
    {
        /// <summary>
        /// Gets the settings service
        /// </summary>
        public static SettingsService Instance { get; } = new SettingsService();

        /// <summary>
        /// The settings helper
        /// </summary>
        private Template10.Services.SettingsService.ISettingsHelper helper;

        /// <summary>
        /// Prevents a default instance of the<see cref="SettingsService" /> class from being created.
        /// </summary>
        private SettingsService()
        {
            helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the shell screen should have a back button
        /// </summary>
        public bool UseShellBackButton
        {
            get
            {
                return helper.Read<bool>(nameof(UseShellBackButton), true);
            }

            set
            {
                helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.Dispatcher.Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                    BootStrapper.Current.NavigationService.Refresh();
                });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use caching.
        /// </summary>
        public bool UseCaching
        {
            get
            {
                return helper.Read<bool>(nameof(UseCaching), true);
            }

            set
            {
                helper.Write(nameof(UseCaching), value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether connections should be closed upon going back to Discovery page.
        /// </summary>
        public bool CloseConnections
        {
            get
            {
                return helper.Read<bool>(nameof(CloseConnections), true);
            }

            set
            {
                helper.Write(nameof(CloseConnections), value);
            }
        }

        /// <summary>
        /// Gets or sets the application theme
        /// </summary>
        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = helper.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }

            set
            {
                helper.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
                Views.Shell.HamburgerMenu.RefreshStyles(value);
            }
        }

        /// <summary>
        /// Gets or sets a value that I don't know what it does
        /// </summary>
        public TimeSpan CacheMaxDuration
        {
            get
            {
                return helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2));
            }

            set
            {
                helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        public ObservableDictionary<string, object> SettingsDictionary = new ObservableDictionary<string, object>();
    }
}
