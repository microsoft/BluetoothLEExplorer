using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using BluetoothLEExplorer.Models;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using GattHelper.Converters;

namespace BluetoothLEExplorer.ViewModels
{
    public class AdvertisementPageViewModel : ViewModelBase
    {
        private GattSampleContext context = GattSampleContext.Context;

        /// <summary>
        /// Source for <see cref="Advertisement"/>
        /// </summary>
        private ObservableBluetoothLEAdvertisement advertisement = GattSampleContext.Context.SelectedAdvertisement;

        /// <summary>
        /// Gets or sets the advertisement that this view model wraps
        /// </summary>
        public ObservableBluetoothLEAdvertisement Advertisement
        {
            get
            {
                return advertisement;
            }

            set
            {
                Set(ref advertisement, value, "Advertisement");
            }
        }


    }
}
