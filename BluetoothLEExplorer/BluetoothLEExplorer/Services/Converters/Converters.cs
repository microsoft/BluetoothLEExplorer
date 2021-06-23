// <copyright file="Converters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using BluetoothLEExplorer.Models;
using BluetoothLEExplorer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using GattHelper.Converters;
using Windows.Networking.BackgroundTransfer;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName",
    Justification = "This is a helper file for Converters used in the xaml")]

[module: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.StyleCop.CSharp.MaintainabilityRules",
    "SA1402:FileMayOnlyContainASingleClass",
    Justification = "This is a helper file for Converters used in the xaml")]

namespace BluetoothLEExplorer.Services.Converters
{
    class ByteArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var byteArray = value as byte[];
            if (byteArray != null)
            {
                return GattConvert.ToHexString(byteArray.AsBuffer());
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            var byteString = value as String;
            if (byteString != null)
            {
                var data = GattConvert.ToIBufferFromHexString(byteString);

                return data.ToArray();
            }
            return null;
        }
    }

    public class ScanningModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Debug.WriteLine($"Convert: value = {value.ToString()}, {parameter.ToString()}");
            return value.ToString().Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            Debug.Write($"ConvertBack: {value.ToString()}, ");
            AdvertisementMonitorPageViewModel.MonitorScanningMode type = AdvertisementMonitorPageViewModel.MonitorScanningMode.NotSet;
            if (value.Equals(true))
            {
                Debug.WriteLine("value true");
                object ret = Enum.Parse(type.GetType(), parameter as string);
                Debug.WriteLine($"Enum.Parse(type.GetType(), parameter as string) - {ret.ToString()}");
                return ret;
            }
            else
            {
                Debug.WriteLine("unsetting value");
                return DependencyProperty.UnsetValue;
            }
        }
    }

    public class DataFormatTypeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Display type radio button converter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>return value of display type radio button converter</returns>
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Debug.WriteLine($"Convert: value = {value.ToString()}, {parameter.ToString()}");
            return value.ToString().Equals(parameter);
        }

        /// <summary>
        /// Display type radio button back converter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Return value of display type radio button back converter</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            Debug.Write($"ConvertBack: {value.ToString()}, ");
            ObservableBluetoothLEAdvertisementFilter.DataFormatType type = ObservableBluetoothLEAdvertisementFilter.DataFormatType.NotSet;
            if (value.Equals(true))
            {
                Debug.WriteLine("value true");
                object ret = Enum.Parse(type.GetType(), parameter as string);
                Debug.WriteLine($"Enum.Parse(type.GetType(), parameter as string) - {ret.ToString()}");
                return ret;
            }
            else
            {
                Debug.WriteLine("unsetting value");
                return DependencyProperty.UnsetValue;
            }
        }
    }

    /// <summary>
    /// Converter to change <see cref="ObservableGattCharacteristics.DisplayTypes"/> to a boolean.
    /// This is used in to determine if a radio button is checked or not
    /// </summary>
    public class DisplayTypeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Display type radio button converter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>return value of display type radio button converter</returns>
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Debug.WriteLine($"Convert: value = {value.ToString()}, {parameter.ToString()}");
            return value.ToString().Equals(parameter);
        }

        /// <summary>
        /// Display type radio button back converter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Return value of display type radio button back converter</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            Debug.Write($"ConvertBack: {value.ToString()}, ");
            ObservableGattCharacteristics.DisplayTypes type = ObservableGattCharacteristics.DisplayTypes.NotSet;
            if (value.Equals(true))
            {
                Debug.WriteLine("value true");
                object ret = Enum.Parse(type.GetType(), parameter as string);
                Debug.WriteLine($"Enum.Parse(type.GetType(), parameter as string) - {ret.ToString()}");
                return ret;
            }
            else
            {
                Debug.WriteLine("unsetting value");
                return DependencyProperty.UnsetValue;
            }
        }
    }

    /// <summary>
    /// Converts <see cref="CharacteristicPageViewModel.WriteTypes"/> to boolean.
    /// Used in radio buttons. 
    /// </summary>
    public class WriteTypeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts WriteType to boolean used in radio buttons
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Boolean if radio button should be checked</returns>
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value.ToString().Equals(parameter);
        }

        /// <summary>
        /// Radio button state of write type radio buttons to boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Write value of radio button</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            CharacteristicPageViewModel.WriteTypes type = CharacteristicPageViewModel.WriteTypes.NotSet;
            if (value.Equals(true))
            {
                return Enum.Parse(type.GetType(), parameter as string);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Uses a boolean to determine if text should be crossed out
    /// </summary>
    public class ShellFontConverter : IValueConverter
    {
        /// <summary>
        /// Converts boolean value to strikethrough font
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>Either a normal font or a strike through font</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value == true)
            {
                return Windows.UI.Text.TextDecorations.None;
            }
            else
            {
                return Windows.UI.Text.TextDecorations.Strikethrough;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>Not implemented return value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
