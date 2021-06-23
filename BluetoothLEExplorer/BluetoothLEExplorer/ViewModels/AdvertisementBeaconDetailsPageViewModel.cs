using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Template10.Mvvm;
using Template10.Services.NavigationService;

using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.Advertisement;

using BluetoothLEExplorer.Models;
using BluetoothLEExplorer.Services.AdvertisementHelpers;


namespace BluetoothLEExplorer.ViewModels
{
    public class AdvertisementBeaconDetailsPageViewModel : ViewModelBase
    {
        public void GotoSettings() => NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        /// <summary>
        /// Go to privacy
        /// </summary>
        public void GotoPrivacy() => NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        /// <summary>
        /// Go to about
        /// </summary>
        public void GotoAbout() => NavigationService.Navigate(typeof(Views.SettingsPage), 2);
    }
}
