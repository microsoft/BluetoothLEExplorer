// <copyright file="GattUuidsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothLEExplorer.Services.GattUuidHelpers
{
    public static class GattServiceUuidHelper
    {
        /// <summary>
        /// Helper function to convert a UUID to a name
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>Name of the UUID</returns>
        public static string ConvertUuidToName(Guid uuid)
        {
            var shortId = BluetoothUuidHelper.TryGetShortId(uuid);
            if (shortId.HasValue &&
                Enum.TryParse(shortId.Value.ToString(), out GattServiceUuid name) == true)
            {
                return name.ToString();
            }
            return uuid.ToString();
        }

        public static bool IsReadOnly(Guid uuid)
        {
            if (GattServiceUuids.DeviceInformation == uuid || GattServiceUuids.GenericAttribute == uuid || GattServiceUuids.GenericAccess == uuid || GattServiceUuids.ScanParameters == uuid)
            {
                return true;
            }

            return false;
        }

        public static bool IsReserved(Guid uuid)
        {
            if (GattServiceUuids.HumanInterfaceDevice == uuid)
            {
                return true;
            }

            return false;
        }
    }

    public static class GattCharacteristicUuidHelper
    {
        /// <summary>
        /// Helper function to convert a UUID to a name
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>Name of the UUID</returns>
        public static string ConvertUuidToName(Guid uuid)
        {
            var shortId = BluetoothUuidHelper.TryGetShortId(uuid);
            if (shortId.HasValue &&
                Enum.TryParse(shortId.Value.ToString(), out GattCharacteristicUuid name) == true)
            {
                return name.ToString();
            }
            return uuid.ToString();
        }
    }

    public static class GattDescriptorUuidHelper
    {
        /// <summary>
        /// Helper function to convert a UUID to a name
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>Name of the UUID</returns>
        public static string ConvertUuidToName(Guid uuid)
        {
            var shortId = BluetoothUuidHelper.TryGetShortId(uuid);
            if (shortId.HasValue &&
                Enum.TryParse(shortId.Value.ToString(), out GattDescriptorUuid name) == true)
            {
                return name.ToString();
            }
            return uuid.ToString();
        }
    }

    /// <summary>
    ///     This enum assists in finding a string representation of a BT SIG assigned value for Service UUIDS
    ///     Reference: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    /// </summary>
    public enum GattServiceUuid : ushort
    {
        Unknown = 0,

        GenericAccess = 0x1800,
        GenericAttribute = 0x1801,
        ImmediateAlert = 0x1802,
        LinkLoss = 0x1803,
        TxPower = 0x1804,
        CurrentTime = 0x1805,
        ReferenceTimeUpdate = 0x1806,
        NextDstChange = 0x1807,
        Glucose = 0x1808,
        HealthThermometer = 0x1809,
        DeviceInformation = 0x180A,
        HeartRate = 0x180D,
        PhoneAlertStatus = 0x180E,
        Battery = 0x180F,
        BloodPressure = 0x1810,
        AlertNotification = 0x1811,
        HumanInterfaceDevice = 0x1812,
        ScanParameters = 0x1813,
        RunningSpeedAndCadence = 0x1814,
        AutomationIo = 0x1815,
        CyclingSpeedAndCadence = 0x1816,
        CyclingPower = 0x1818,
        LocationAndNavigation = 0x1819,
        EnvironmentalSensing = 0x181A,
        BodyComposition = 0x181B,
        UserData = 0x181C,
        WeightScale = 0x181D,
        BondManagement = 0x181E,
        ContinuousGlucoseMonitoring = 0x181F,
        InternetProtocolSupport = 0x1820,
        IndoorPositioning = 0x1821,
        PulseOximeter = 0x1822,
        HttpProxy = 0x1823,
        TransportDiscovery = 0x1824,
        ObjectTransfer = 0x1825,
        FitnessMachine = 0x1826,
        MeshProvisioning = 0x1827,
        MeshProxy = 0x1828,
        ReconnectionConfiguration = 0x1829,
        InsulinDelivery = 0x183A,
        BinarySensor = 0x183B,
        EmergencyConfiguration = 0x183C,
        PhysicalActivityMonitor = 0x183E,
        AudioInputControl = 0x1843,
        VolumeControl = 0x1844,
        VolumeOffsetControl = 0x1845,
        CoordinatedSetIdentificationService = 0x1846,
        DeviceTime = 0x1847,
        MediaControlService = 0x1848,
        GenericMediaControlService = 0x1849,
        ConstantToneExtension = 0x184A,
        TelephoneBearerService = 0x184B,
        GenericTelephoneBearerService = 0x184C,
        MicrophoneControl = 0x184D,
    }

    /// <summary>
    ///     This enum is nice for finding a string representation of a BT SIG assigned value for Characteristic UUIDs
    ///     Reference: https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicsHome.aspx
    /// </summary>
    public enum GattCharacteristicUuid : ushort
    {
        Unknown = 0,

        DeviceName = 0x2A00,
        Appearance = 0x2A01,
        PeripheralPrivacyFlag = 0x2A02,
        ReconnectionAddress = 0x2A03,
        PeripheralPreferredConnectionParameters = 0x2A04,
        ServiceChanged = 0x2A05,
        AlertLevel = 0x2A06,
        TxPowerLevel = 0x2A07,
        DateTime = 0x2A08,
        DayOfWeek = 0x2A09,
        DayDateTime = 0x2A0A,
        ExactTime256 = 0x2A0C,
        DstOffset = 0x2A0D,
        TimeZone = 0x2A0E,
        LocalTimeInformation = 0x2A0F,
        TimeWithDst = 0x2A11,
        TimeAccuracy = 0x2A12,
        TimeSource = 0x2A13,
        ReferenceTimeInformation = 0x2A14,
        TimeUpdateControlPoint = 0x2A16,
        TimeUpdateState = 0x2A17,
        GlucoseMeasurement = 0x2A18,
        BatteryLevel = 0x2A19,
        TemperatureMeasurement = 0x2A1C,
        TemperatureType = 0x2A1D,
        IntermediateTemperature = 0x2A1E,
        MeasurementInterval = 0x2A21,
        BootKeyboardInputReport = 0x2A22,
        SystemId = 0x2A23,
        ModelNumberString = 0x2A24,
        SerialNumberString = 0x2A25,
        FirmwareRevisionString = 0x2A26,
        HardwareRevisionString = 0x2A27,
        SoftwareRevisionString = 0x2A28,
        ManufacturerNameString = 0x2A29,
        Ieee11073_20601RegulatoryCertificationDataList = 0x2A2A,
        CurrentTime = 0x2A2B,
        MagneticDeclination = 0x2A2C,
        ScanRefresh = 0x2A31,
        BootKeyboardOutputReport = 0x2A32,
        BootMouseInputReport = 0x2A33,
        GlucoseMeasurementContext = 0x2A34,
        BloodPressureMeasurement = 0x2A35,
        IntermediateCuffPressure = 0x2A36,
        HeartRateMeasurement = 0x2A37,
        BodySensorLocation = 0x2A38,
        HeartRateControlPoint = 0x2A39,
        AlertStatus = 0x2A3F,
        RingerControlPoint = 0x2A40,
        RingerSetting = 0x2A41,
        AlertCategoryIdBitMask = 0x2A42,
        AlertCategoryId = 0x2A43,
        AlertNotificationControlPoint = 0x2A44,
        UnreadAlertStatus = 0x2A45,
        NewAlert = 0x2A46,
        SupportedNewAlertCategory = 0x2A47,
        SupportedUnreadAlertCategory = 0x2A48,
        BloodPressureFeature = 0x2A49,
        HidInformation = 0x2A4A,
        ReportMap = 0x2A4B,
        HidControlPoint = 0x2A4C,
        HidReport = 0x2A4D,
        ProtocolMode = 0x2A4E,
        ScanIntervalWindow = 0x2A4F,
        PnpId = 0x2A50,
        GlucoseFeature = 0x2A51,
        RecordAccessControlPoint = 0x2A52,
        RscMeasurement = 0x2A53,
        RscFeature = 0x2A54,
        ScControlPoint = 0x2A55,
        Digital = 0x2A56,
        Analog = 0x2A58,
        Aggregate = 0x2A5A,
        CscMeasurement = 0x2A5B,
        CscFeature = 0x2A5C,
        SensorLocation = 0x2A5D,
        PlxSpotCheckMeasurement = 0x2A5E,
        PlxContinuousMeasurement = 0x2A5F,
        PlxFeatures = 0x2A60,
        CyclingPowerMeasurement = 0x2A63,
        CyclingPowerVector = 0x2A64,
        CyclingPowerFeature = 0x2A65,
        CyclingPowerControlPoint = 0x2A66,
        LocationAndSpeed = 0x2A67,
        Navigation = 0x2A68,
        PositionQuality = 0x2A69,
        LnFeature = 0x2A6A,
        LnControlPoint = 0x2A6B,
        Elevation = 0x2A6C,
        Pressure = 0x2A6D,
        Temperature = 0x2A6E,
        Humidity = 0x2A6F,
        TrueWindSpeed = 0x2A70,
        TrueWindDirection = 0x2A71,
        ApparentWindSpeed = 0x2A72,
        ApparentWindDirection = 0x2A73,
        GustFactor = 0x2A74,
        PollenConcentration = 0x2A75,
        UvIndex = 0x2A76,
        Irradiance = 0x2A77,
        Rainfall = 0x2A78,
        WindChill = 0x2A79,
        HeatIndex = 0x2A7A,
        DewPoint = 0x2A7B,
        DescriptorValueChanged = 0x2A7D,
        AerobicHeartRateLowerLimit = 0x2A7E,
        AerobicThreshold = 0x2A7F,
        Age = 0x2A80,
        AnaerobicHeartRateLowerLimit = 0x2A81,
        AnaerobicHeartRateUpperLimit = 0x2A82,
        AnaerobicThreshold = 0x2A83,
        AerobicHeartRateUpperLimit = 0x2A84,
        DateOfBirth = 0x2A85,
        DateOfThresholdAssessment = 0x2A86,
        EmailAddress = 0x2A87,
        FatBurnHeartRateLowerLimit = 0x2A88,
        FatBurnHeartRateUpperLimit = 0x2A89,
        FirstName = 0x2A8A,
        FiveZoneHeartRateLimits = 0x2A8B,
        Gender = 0x2A8C,
        HeartRateMax = 0x2A8D,
        Height = 0x2A8E,
        HipCircumference = 0x2A8F,
        LastName = 0x2A90,
        MaximumRecommendedHeartRate = 0x2A91,
        RestingHeartRate = 0x2A92,
        SportTypeForAerobicAndAnaerobicThresholds = 0x2A93,
        ThreeZoneHeartRateLimits = 0x2A94,
        TwoZoneHeartRateLimit = 0x2A95,
        Vo2Max = 0x2A96,
        WaistCircumference = 0x2A97,
        Weight = 0x2A98,
        DatabaseChangeIncrement = 0x2A99,
        UserIndex = 0x2A9A,
        BodyCompositionFeature = 0x2A9B,
        BodyCompositionMeasurement = 0x2A9C,
        WeightMeasurement = 0x2A9D,
        WeightScaleFeature = 0x2A9E,
        UserControlPoint = 0x2A9F,
        MagneticFluxDensity2D = 0x2AA0,
        MagneticFluxDensity3D = 0x2AA1,
        Language = 0x2AA2,
        BarometricPressureTrend = 0x2AA3,
        BondManagementControlPoint = 0x2AA4,
        BondManagementFeature = 0x2AA5,
        CentralAddressResolution = 0x2AA6,
        CgmMeasurement = 0x2AA7,
        CgmFeature = 0x2AA8,
        CgmStatus = 0x2AA9,
        CgmSessionStartTime = 0x2AAA,
        CgmSessionRunTime = 0x2AAB,
        CgmSpecificOpsControlPoint = 0x2AAC,
        IndoorPositioningConfiguration = 0x2AAD,
        Latitude = 0x2AAE,
        Longitude = 0x2AAF,
        LocalNorthCoordinate = 0x2AB0,
        LocalEastCoordinate = 0x2AB1,
        FloorNumber = 0x2AB2,
        Altitude = 0x2AB3,
        Uncertainty = 0x2AB4,
        LocationName = 0x2AB5,
        Uri = 0x2AB6,
        HttpHeaders = 0x2AB7,
        HttpStatusCode = 0x2AB8,
        HttpEntityBody = 0x2AB9,
        HttpControlPoint = 0x2ABA,
        HttpsSecurity = 0x2ABB,
        TdsControlPoint = 0x2ABC,
        OtsFeature = 0x2ABD,
        ObjectName = 0x2ABE,
        ObjectType = 0x2ABF,
        ObjectSize = 0x2AC0,
        ObjectFirstCreated = 0x2AC1,
        ObjectLastModified = 0x2AC2,
        ObjectId = 0x2AC3,
        ObjectProperties = 0x2AC4,
        ObjectActionControlPoint = 0x2AC5,
        ObjectListControlPoint = 0x2AC6,
        ObjectListFilter = 0x2AC7,
        ObjectChanged = 0x2AC8,
        DatabaseHash = 0x2B2A,
        ClientSupportedFeatures = 0x2B29,
        AudioInputState = 0x2B77,
        GainSettingsAttribute = 0x2B78,
        AudioInputType = 0x2B79,
        AudioInputStatus = 0x2B7A,
        AudioInputControlPoint = 0x2B7B,
        AudioInputDescription = 0x2B7C,
        VolumeState = 0x2B7D,
        VolumeControlPoint = 0x2B7E,
        VolumeFlags = 0x2B7F,
        OffsetState = 0x2B80,
        AudioLocation = 0x2B81,
        VolumeOffsetControlPoint = 0x2B82,
        AudioOutputDescription = 0x2B83,
        SetIdentityResolvingKeyCharacteristic = 0x2B84,
        SizeCharacteristic = 0x2B85,
        LockCharacteristic = 0x2B86,
        RankCharacteristic = 0x2B87,
        DeviceTimeFeature = 0x2B8E,
        DeviceTimeParameters = 0x2B8F,
        DeviceTime = 0x2B90,
        DeviceTimeControlPoint = 0x2B91,
        TimeChangeLogData = 0x2B92,
        MediaPlayerName = 0x2B93,
        MediaPlayerIconObjectID = 0x2B94,
        MediaPlayerIconURL = 0x2B95,
        TrackChanged = 0x2B96,
        TrackTitle = 0x2B97,
        TrackDuration = 0x2B98,
        TrackPosition = 0x2B99,
        PlaybackSpeed = 0x2B9A,
        SeekingSpeed = 0x2B9B,
        CurrentTrackSegmentsObjectID = 0x2B9C,
        CurrentTrackObjectID = 0x2B9D,
        NextTrackObjectID = 0x2B9E,
        ParentGroupObjectID = 0x2B9F,
        CurrentGroupObjectID = 0x2BA0,
        PlayingOrder = 0x2BA1,
        PlayingOrdersSupported = 0x2BA2,
        MediaState = 0x2BA3,
        MediaControlPoint = 0x2BA4,
        MediaControlPointOpcodesSupported = 0x2BA5,
        SearchResultsObjectID = 0x2BA6,
        SearchControlPoint = 0x2BA7,
        MediaPlayerIconObjectType = 0x2BA9,
        TrackSegmentsObjectType = 0x2BAA,
        TrackObjectType = 0x2BAB,
        GroupObjectType = 0x2BAC,
        ConstantToneExtensionEnable = 0x2BAD,
        AdvertisingConstantToneExtensionMinimumLength = 0x2BAE,
        AdvertisingConstantToneExtensionMinimumTransmitCount = 0x2BAF,
        AdvertisingConstantToneExtensionTransmitDuration = 0x2BB0,
        AdvertisingConstantToneExtensionInterval = 0x2BB1,
        AdvertisingConstantToneExtensionPHY = 0x2BB2,
        BearerProviderName = 0x2BB3,
        BearerUCI = 0x2BB4,
        BearerTechnology = 0x2BB5,
        BearerURISchemesSupportedList = 0x2BB6,
        BearerSignalStrength = 0x2BB7,
        BearerSignalStrengthReportingInterval = 0x2BB8,
        BearerListCurrentCalls = 0x2BB9,
        ContentControlID = 0x2BBA,
        StatusFlags = 0x2BBB,
        IncomingCallTargetBearerURI = 0x2BBC,
        CallState = 0x2BBD,
        CallControlPoint = 0x2BBE,
        CallControlPointOptionalOpcodes = 0x2BBF,
        TerminationReason = 0x2BC0,
        IncomingCall = 0x2BC1,
        CallFriendlyName = 0x2BC2,
        Mute = 0x2BC3,
        SimpleKeyState = 0xFFE1,
    }

    /// <summary>
    ///     This enum assists in finding a string representation of a BT SIG assigned value for Descriptor UUIDs
    ///     Reference: https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx
    /// </summary>
    enum GattDescriptorUuid : ushort
    {
        Unknown = 0,

        CharacteristicExtendedProperties = 0x2900,
        CharacteristicUserDescription = 0x2901,
        ClientCharacteristicConfiguration = 0x2902,
        ServerCharacteristicConfiguration = 0x2903,
        CharacteristicPresentationFormat = 0x2904,
        CharacteristicAggregateFormat = 0x2905,

        ValidRange = 0x2906,
        ExternalReportReference = 0x2907,
        ReportReference = 0x2908,
        NumberOfDigitals = 0x2909,
        ValueTriggerSetting = 0x290A,
        EnvironmentalSensingConfiguration = 0x290B,
        EnvironmentalSensingMeasurement = 0x290C,
        EnvironmentalSensingTriggerSetting = 0x290D,
        TimeTriggerSetting = 0x290E,
    }
}
