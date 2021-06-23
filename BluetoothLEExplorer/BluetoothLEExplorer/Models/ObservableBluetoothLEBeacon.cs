using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation.Metadata;
using GattHelper.Converters;

namespace BluetoothLEExplorer.Models
{

    public class ObservableBluetoothLEBeacon : INotifyPropertyChanged
    {
        private GattSampleContext Context { get; set; }

        private BluetoothLEAdvertisementPublisher publisher;

        public String Name { get; private set; }

        private BluetoothLEAdvertisementPublisherStatus status = BluetoothLEAdvertisementPublisherStatus.Created;

        bool isPublishing = false;

        public bool IsPublishing
        {
            get
            {
                return isPublishing;
            }

            set
            {
                if (value != isPublishing)
                {
                    if (value)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        public bool IsReady { get; private set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableBluetoothLEBeacon(
            byte[] payload,
            bool useExtendedFormat,
            bool isAnonymous,
            bool includeTxPower,
            Int16? txPower)
        {
            Context = GattSampleContext.Context;

            var payloadString = GattConvert.ToHexString(payload.AsBuffer());
            Name = payloadString.Substring(0, Math.Min(8, payloadString.Length));

            var advertisement = new BluetoothLEAdvertisement();
            advertisement.ManufacturerData.Add(new BluetoothLEManufacturerData(0x0006, payload.AsBuffer()));

            publisher = new BluetoothLEAdvertisementPublisher(advertisement);

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
            {
                publisher.UseExtendedAdvertisement = useExtendedFormat;
                publisher.IsAnonymous = isAnonymous;
                publisher.IncludeTransmitPowerLevel = includeTxPower;
                publisher.PreferredTransmitPowerLevelInDBm = txPower;
            }

            publisher.StatusChanged += Publisher_StatusChanged;
        }

        private void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            if (args.Status == BluetoothLEAdvertisementPublisherStatus.Started)
            {
                if (!isPublishing)
                {
                    isPublishing = true;
                    OnPropertyChanged("IsPublishing");
                }
                IsReady = true;
            }
            else if (args.Status == BluetoothLEAdvertisementPublisherStatus.Stopped ||
                args.Status == BluetoothLEAdvertisementPublisherStatus.Aborted)
            {
                if (isPublishing)
                {
                    isPublishing = false;
                    OnPropertyChanged("IsPublishing");
                }
                IsReady = true;
            }
        }

        public void Start()
        {
            IsReady = false;
            publisher.Start();
        }

        public void Stop()
        {
            IsReady = false;
            publisher.Stop();
        }

        public void Destroy()
        {
            Stop();
            Context.Beacons.Remove(this);
        }

        private async void OnPropertyChanged(string propertyName)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
            }
            catch (Exception e)
            {
                Debug.Fail(String.Format("Failed to update property '{0}' due to {1}", propertyName, e.ToString()));
            }
        }
    }
}
