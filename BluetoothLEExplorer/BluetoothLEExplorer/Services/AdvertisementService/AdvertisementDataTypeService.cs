// <copyright file="GattUuidsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace BluetoothLEExplorer.Services.AdvertisementHelpers
{
    enum AdvertisementSectionType : byte
    {
        Flags = 0x01,
        IncompleteService16BitUuids = 0x02,
        CompleteService16BitUuids = 0x03,
        IncompleteService32BitUuids = 0x04,
        CompleteService32BitUuids = 0x05,
        IncompleteService128BitUuids = 0x06,
        CompleteService128BitUuids = 0x07,
        ShortenedLocalName = 0x08,
        CompleteLocalName = 0x09,
        TxPowerLevel = 0x0A,
        ClassOfDevice = 0x0D,
        SimplePairingHashC192 = 0x0E,
        SimplePairingRandomizerR192 = 0x0F,
        SecurityManagerTKValues = 0x10,
        SecurityManagerOutOfBandFlags = 0x11,
        SlaveConnectionIntervalRange = 0x12,
        ServiceSolicitation16BitUuids = 0x14,
        ServiceSolicitation32BitUuids = 0x1F,
        ServiceSolicitation128BitUuids = 0x15,
        ServiceData16BitUuids = 0x16,
        ServiceData32BitUuids = 0x20,
        ServiceData128BitUuids = 0x21,
        PublicTargetAddress = 0x17,
        RandomTargetAddress = 0x18,
        Appearance = 0x19,
        AdvertisingInterval = 0x1A,
        LEBluetoothDeviceAddress = 0x1B,
        LERole = 0x1C,
        SimplePairingHashC256 = 0x1D,
        SimplePairingRandomizerR256 = 0x1E,
        ThreeDimensionInformationData = 0x3D,
        ManufacturerSpecificData = 0xFF,
    }

    public static class AdvertisementDataTypeHelper
    {
        public static string ConvertSectionTypeToString(byte sectionType)
        {
            AdvertisementSectionType convertedSectionType;
            if (Enum.TryParse(sectionType.ToString(), out convertedSectionType))
            {
                return convertedSectionType.ToString();
            }
            return sectionType.ToString("X2");
        }
    }
}
