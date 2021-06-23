// <copyright file="ServicesPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using BluetoothLEExplorer.Models;
using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Device Service page
    /// </summary>
    public sealed partial class ServicePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePage" /> class.
        /// </summary>
        public ServicePage()
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