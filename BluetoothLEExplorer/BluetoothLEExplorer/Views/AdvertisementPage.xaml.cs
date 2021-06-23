using BluetoothLEExplorer.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BluetoothLEExplorer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdvertisementPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertisementPage" /> class.
        /// </summary>
        public AdvertisementPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }
}
