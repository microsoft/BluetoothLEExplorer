// <copyright file="CharacteristicPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
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
                if ((WriteValue.Text != string.Empty) &&
                    (int.TryParse(WriteValue.Text, System.Globalization.NumberStyles.HexNumber, null, out buf) == false))
                {
                    WriteValue.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                else
                {
                    WriteValue.Background = new SolidColorBrush(Windows.UI.Colors.White);
                }
            }
            else
            {
                WriteValue.Background = new SolidColorBrush(Windows.UI.Colors.White);
            }
        }
    }
}
