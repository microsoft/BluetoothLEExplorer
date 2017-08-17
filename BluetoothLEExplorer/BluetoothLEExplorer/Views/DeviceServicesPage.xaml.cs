// <copyright file="DeviceServicesPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using BluetoothLEExplorer.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Device Service page
    /// </summary>
    public sealed partial class DeviceServicesPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServicesPage" /> class.
        /// </summary>
        public DeviceServicesPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        /// <summary>
        /// Updates the view model with the just selected characteristic 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CharacteristicsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedCharacteristic = (ObservableGattCharacteristics)e.ClickedItem;
        }
    }
}
