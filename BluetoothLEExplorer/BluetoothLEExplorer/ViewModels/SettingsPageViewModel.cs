// <copyright file="SettingsPageViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.StyleCop.CSharp.MaintainabilityRules",
    "SA1402:FileMayOnlyContainASingleClass",
    Justification = "Template 10 comes this way and it improves readability")]

namespace BluetoothLEExplorer.ViewModels
{
    /// <summary>
    /// View Model for the settings page
    /// </summary>
    public class SettingsPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the settings part of the settings page
        /// </summary>
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();

        /// <summary>
        /// Gets the about part of the settings page
        /// </summary>
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
    }

    /// <summary>
    /// View Model for the settings part of the settings page
    /// </summary>
    public class SettingsPartViewModel : ViewModelBase
    {
        /// <summary>
        /// Settings service
        /// </summary>
        private Services.SettingsServices.SettingsService settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPartViewModel" /> class.
        /// </summary>
        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                settings = Services.SettingsServices.SettingsService.Instance;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the shell should have the back button
        /// </summary>
        public bool UseShellBackButton
        {
            get
            {
                return settings.UseShellBackButton;
            }

            set
            {
                settings.UseShellBackButton = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether caching should be used
        /// </summary>
        public bool UseCachingButton
        {
            get
            {
                return settings.UseCaching;
            }

            set
            {
                settings.UseCaching = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether connections should be closed upon going back to Discovery page
        /// </summary>
        public bool CloseConnectionsButton
        {
            get
            {
                return settings.CloseConnections;
            }

            set
            {
                settings.CloseConnections = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether light theme should be used
        /// </summary>
        public bool UseLightThemeButton
        {
            get
            {
                return settings.AppTheme.Equals(ApplicationTheme.Light);
            }

            set
            {
                settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Text to show on busy page
        /// </summary>
        private string busyText = "Please wait...";

        /// <summary>
        /// Gets or sets the busy text
        /// </summary>
        public string BusyText
        {
            get
            {
                return busyText;
            }

            set
            {
                Set(ref busyText, value);
                showBusyCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Delegate command to show busy page
        /// </summary>
        private DelegateCommand showBusyCommand;

        /// <summary>
        /// Show busy command delegate
        /// </summary>
        public DelegateCommand ShowBusyCommand
            => showBusyCommand ?? (showBusyCommand = new DelegateCommand(async () =>
            {
                Views.Busy.SetBusy(true, busyText);
                await Task.Delay(5000);
                Views.Busy.SetBusy(false);
            }, 
                () => !string.IsNullOrEmpty(BusyText)));
    }

    /// <summary>
    /// View Model of the about part of the settings page
    /// </summary>
    public class AboutPartViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the logo of this application
        /// </summary>
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        /// <summary>
        /// Gets the display name of this application
        /// </summary>
        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        /// <summary>
        /// Gets the publisher of this application
        /// </summary>
        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        /// <summary>
        /// Gets the version string
        /// </summary>
        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }
    }
}
