using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;
using GattHelper.Converters;
using BluetoothLEExplorer.Services.AdvertisementHelpers;
using Windows.Security.Cryptography;

namespace BluetoothLEExplorer.Models
{
    public class ObservableBluetoothLEAdvertisementFilter : INotifyPropertyChanged
    {
        public enum DataFormatType
        {
            NotSet,
            Hex,
            String,
        }

        public string Name
        {
            get;
            private set;
        }

        public byte SectionType
        {
            get;
            private set;
        }

        public byte SectionOffset
        {
            get;
            set;
        }

        public string SectionDataString
        {
            get;
            set;
        }

        public bool DataSectionRawFilter
        {
            get
            {
                return true;
            }
        }

        private DataFormatType sectionDataFormat = DataFormatType.Hex;

        public DataFormatType SectionDataFormat
        {
            get
            {
                return sectionDataFormat;
            }

            set
            {
                if (value == DataFormatType.NotSet)
                {
                    return;
                }

                if (sectionDataFormat != value)
                {
                    sectionDataFormat = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SectionDataFormat"));
                }
            }
        }

        public ObservableBluetoothLEAdvertisementFilter(byte sectionType)
        {
            SectionType = sectionType;
            Name = AdvertisementDataTypeHelper.ConvertSectionTypeToString(sectionType);
        }

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Executes when this class changes
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public BluetoothLEAdvertisementBytePattern GetBytePattern()
        {
            var pattern = new BluetoothLEAdvertisementBytePattern();
            pattern.DataType = SectionType;
            pattern.Offset = SectionOffset;
            if (SectionDataString != null && SectionDataString.Length > 0)
            {
                if (sectionDataFormat == DataFormatType.Hex)
                {
                    // pad the value if we've received odd number of bytes
                    if (SectionDataString.Length % 2 == 1)
                    {
                        pattern.Data = GattConvert.ToIBufferFromHexString("0" + SectionDataString);
                    }
                    else
                    {
                        pattern.Data = GattConvert.ToIBufferFromHexString(SectionDataString);
                    }
                }
                else if (sectionDataFormat == DataFormatType.String)
                {
                    pattern.Data = CryptographicBuffer.ConvertStringToBinary(SectionDataString, BinaryStringEncoding.Utf8);
                }
            }
            return pattern;
        }
    }
}
