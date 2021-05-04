// <copyright file="CharacteristicPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using BluetoothLEExplorer.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Characteristic Page
    /// </summary>
    public sealed partial class CharacteristicPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicPage" /> class.
        /// </summary>
        public CharacteristicPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private void WriteValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            WriteBoxBackgroundCheck();
        }

        private void radioButton5_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WriteBoxBackgroundCheck();
        }

        private void radioButton5_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WriteBoxBackgroundCheck();
        }

        private void WriteBoxBackgroundCheck()
        {
            if (ViewModel.WriteType == ViewModels.CharacteristicPageViewModel.WriteTypes.Hex)
            {
                int buf;
                if (string.IsNullOrWhiteSpace(WriteValue.Text) == false)
                {
                    for(int i = 0; i < WriteValue.Text.Length; i++)
                    {
                        if(int.TryParse(WriteValue.Text[i].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier, null, out buf) == false)
                        {
                            WriteValue.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                            return;
                        }
                    }
                }
            }

            WriteValue.Background = new SolidColorBrush(Windows.UI.Colors.White);
        }

        private void DescriptorsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedDescriptor = (ObservableGattDescriptors)e.ClickedItem;
        }

        /// <summary>
        /// Detect when a user presses enter and writes the value to the selected GattCharacterisitc.
        /// </summary>
        public void DetectEnter_Keydown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.WriteValue();
            }
        }
    }
}
