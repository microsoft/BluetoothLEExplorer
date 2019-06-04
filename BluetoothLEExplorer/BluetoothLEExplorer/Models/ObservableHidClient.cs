using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using System.Diagnostics;

namespace BluetoothLEExplorer.Models
{
    public class ObservableGattClient : INotifyPropertyChanged, IEquatable<ObservableGattClient>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BluetoothLEDevice m_leDevice;

        private bool m_isConnected;
        public bool IsConnected
        {
            get
            {
                return m_isConnected;
            }
            private set
            {
                m_isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }

        public string Name
        {
            get
            {
                return m_leDevice.Name;
            }
        }

        public static async Task<ObservableGattClient> FromIdAsync(string deviceId)
        {
            var leDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            return new ObservableGattClient(leDevice);
        }

        public ObservableGattClient(BluetoothLEDevice device)
        {
            m_leDevice = device;
            m_leDevice.ConnectionStatusChanged += ConnectionStatusChanged;
            IsConnected = (m_leDevice.ConnectionStatus == BluetoothConnectionStatus.Connected);
        }

        ~ObservableGattClient()
        {
            m_leDevice.ConnectionStatusChanged -= ConnectionStatusChanged;
            m_leDevice.Dispose();
        }

        bool IEquatable<ObservableGattClient>.Equals(ObservableGattClient other)
        {
            if (other == null)
            {
                return false;
            }

            return (m_leDevice.DeviceId == other.m_leDevice.DeviceId);
        }

        private void ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            IsConnected = (sender.ConnectionStatus == BluetoothConnectionStatus.Connected);
        }

        private async void OnPropertyChanged(string propertyName)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        });
                }
            }
            catch (Exception e)
            {
                Debug.Fail(String.Format("Failed to update property '{0}' due to {1}", propertyName, e.ToString()));
            }
        }
    }
}
