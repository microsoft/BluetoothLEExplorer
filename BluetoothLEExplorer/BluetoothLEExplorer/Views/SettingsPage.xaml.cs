// <copyright file="SettingsPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Settings page
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        /// <summary>
        /// Serialization service
        /// </summary>
        private Template10.Services.SerializationService.ISerializationService serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage" /> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            serializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        /// <summary>
        /// Executes when navigating to settings page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(serializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;
        }
    }
}
