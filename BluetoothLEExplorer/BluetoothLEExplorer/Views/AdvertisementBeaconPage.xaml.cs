// <copyright file="AdvertisementBeaconPage.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;

namespace BluetoothLEExplorer.Views
{
    //public class ValidationTextBox : TextBox
    //{
    //    public Regex ValidationExpression { get; set; }

    //    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    //    {
            

    //        string currentText = this.Text;
    //        string newText = currentText + e.Text;
    //    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdvertisementBeaconPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertisementBeaconPage" /> class.
        /// </summary>
        public AdvertisementBeaconPage()
        {
            this.InitializeComponent();
        }

        public void PreferredTxPower_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewText) && Regex.IsMatch(e.NewText, @"[^0-9\-]+"))
            {
                e.Cancel = true;
            }
        }

        public void Payload_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewText) &&
                !Regex.IsMatch(e.NewText, @"^([0-9a-f]{1,2}[\-]{0,1})*$", RegexOptions.IgnoreCase))
            {
                e.Cancel = true;
            }
        }
    }
}
