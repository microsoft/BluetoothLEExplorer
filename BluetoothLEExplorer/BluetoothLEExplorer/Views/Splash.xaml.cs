// <copyright file="Splash.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// Splash screen
    /// </summary>
    public sealed partial class Splash : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Splash" /> class.
        /// </summary>
        /// <param name="splashScreen"></param>
        public Splash(SplashScreen splashScreen)
        {
            InitializeComponent();
            Window.Current.SizeChanged += (s, e) => Resize(splashScreen);
            Resize(splashScreen);
        }

        /// <summary>
        /// Resizes the splash screen
        /// </summary>
        /// <param name="splashScreen"></param>
        private void Resize(SplashScreen splashScreen)
        {
            if (splashScreen.ImageLocation.Top == 0)
            {
                splashImage.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                rootCanvas.Background = null;
                splashImage.Visibility = Visibility.Visible;
            }

            splashImage.Height = splashScreen.ImageLocation.Height;
            splashImage.Width = splashScreen.ImageLocation.Width;
            splashImage.SetValue(Canvas.TopProperty, splashScreen.ImageLocation.Top);
            splashImage.SetValue(Canvas.LeftProperty, splashScreen.ImageLocation.Left);
            ProgressTransform.TranslateY = splashImage.Height / 2;
        }
    }
}