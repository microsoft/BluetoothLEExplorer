// <copyright file="Busy.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using Template10.Common;
using Template10.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Busy overlay
    /// </summary>
    public sealed partial class Busy : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Busy" /> class.
        /// </summary>
        public Busy()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value of the busy text
        /// </summary>
        public string BusyText
        {
            get { return (string)GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        /// <summary>
        /// Busy text property
        /// </summary>
        public static readonly DependencyProperty BusyTextProperty = DependencyProperty.Register(nameof(BusyText), typeof(string), typeof(Busy), new PropertyMetadata("Please wait..."));

        /// <summary>
        /// Gets or sets a value indicating whether the busy screen should be displayed
        /// </summary>
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        /// <summary>
        /// Dependency object for is busy property
        /// </summary>
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(Busy), new PropertyMetadata(false));

        /// <summary>
        /// hide and show busy dialog
        /// </summary>
        /// <param name="busy"></param>
        /// <param name="text"></param>
        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                var view = modal.ModalContent as Busy;

                if (view == null)
                {
                    modal.ModalContent = view = new Busy();
                }

                modal.IsModal = view.IsBusy = busy;
                view.BusyText = text;
            });
        }
    }
}
