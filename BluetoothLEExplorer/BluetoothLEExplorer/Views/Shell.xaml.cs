// <copyright file="Shell.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Shell page 
    /// </summary>
    public sealed partial class Shell : Page
    {
        /// <summary>
        /// Gets or sets the shell instance
        /// </summary>
        public static Shell Instance { get; set; }

        /// <summary>
        /// Hamburger menu instance
        /// </summary>
        public static HamburgerMenu HamburgerMenu => Instance.MyHamburgerMenu;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shell" /> class.
        /// </summary>
        public Shell()
        {
            Instance = this;
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shell" /> class.
        /// </summary>
        /// <param name="navigationService"></param>
        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        /// <summary>
        /// Initializes the navigation service
        /// </summary>
        /// <param name="navigationService"></param>
        public void SetNavigationService(INavigationService navigationService)
        {
            MyHamburgerMenu.NavigationService = navigationService;
        }
    }
}
