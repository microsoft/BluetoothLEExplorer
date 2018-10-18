// <copyright file="AlertNotificationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using GattServicesLibrary.CharacteristicParameterValues;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.ComponentModel;

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Alert Notification Services
    /// </summary>
    public class AlertNotificationService : GenericGattService
    {
        public UInt64 UnreadCount = 0;

        public class AlertNotification
        {
            public AlertNotification(String name)
            {
                Name = name;
            }

            public String Name{ get; private set; }
        };

        public AlertNotification LastNotification = null;

        /// <summary>
        /// Characteristic provides the count of new alerts (for a given category) in the server.
        /// </summary>
        private GattServicesLibrary.Characteristics.NewAlertCharacteristic newAlert;

        /// <summary>
        /// Gets or sets the newAlert
        /// </summary>
        public GenericGattCharacteristic NewAlert
        {
            get
            {
                return newAlert;
            }

            set
            {
                if (newAlert != value)
                {
                    newAlert = value as GattServicesLibrary.Characteristics.NewAlertCharacteristic;
                    OnPropertyChanged(new PropertyChangedEventArgs("NewAlert"));
                }
            }
        }

        /// <summary>
        /// Characteristic exposes the count of unread alert events existing in the server.
        /// The count of unread alert events is provided with the Category ID.
        /// </summary>
        private GattServicesLibrary.Characteristics.UnreadAlertStatusCharacteristic unreadAlertStatus;

        /// <summary>
        /// Gets or sets the unreadAlertStatus
        /// </summary>
        public GenericGattCharacteristic UnreadAlertStatus
        {
            get
            {
                return unreadAlertStatus;
            }

            set
            {
                if (unreadAlertStatus != value)
                {
                    unreadAlertStatus = value as GattServicesLibrary.Characteristics.UnreadAlertStatusCharacteristic;
                    OnPropertyChanged(new PropertyChangedEventArgs("UnreadAlertStatus"));
                }
            }
        }

        /// <summary>
        /// Characteristic allows the client device to enable/disable the alert notification of new alert and 
        /// unread alert events more selectively than can be done by setting or clearing the notification bit 
        /// in the Client Characteristic configuration for each alert characteristic.
        /// </summary>
        private GattServicesLibrary.Characteristics.AlertNotificationControlPointCharacteristic alertNotificationControlPoint;

        public GenericGattCharacteristic AlertNotificationControlPoint
        {
            get
            {
                return alertNotificationControlPoint;
            }

            set
            {
                if (alertNotificationControlPoint != value)
                {
                    alertNotificationControlPoint = value as GattServicesLibrary.Characteristics.AlertNotificationControlPointCharacteristic;
                    OnPropertyChanged(new PropertyChangedEventArgs("AlertNotificationControlPoint"));
                }
            }
        }

        /// <summary>
        /// Characteristic is a bit map showing which categories of unread alert events are supported and which are not.
        /// </summary>
        private GattLocalCharacteristic supportUnreadAlert;

        /// <summary>
        /// Characteristic is a bit map showing which categories of new alert are supported and which are not.
        /// </summary>
        private GattLocalCharacteristic supportNewAlert;

        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Alert Notification Service";
            }
        }

        public void ProcessCommand(Characteristics.AlertNotificationControlPointCharacteristic.AlertNotificationControlPointCommand command)
        {
            switch (command.CommandId)
            {
            case AlertNotificationControlPointCommandId.DisableNewIncomingAlertNotification:
                
                break;

            case AlertNotificationControlPointCommandId.DisableUnreadCategoryStatusNotification:
                break;

            case AlertNotificationControlPointCommandId.EnableNewIncomgAlertNotification:
                // Enable New Alert messages for the category specified in the Category ID field of the command. If the
                // category ID is specified as 0xff, all supported categories shall be enabled. 
                //
                // If a client has configured notifications on the New Alert characteristic, notifications shall be 
                // sent when the count of new alerts changes in the server for an enabled category or the server
                // receives the command "Notify New Alert immediately" for an enabled category via the Alert Notification Control Point.
                newAlert.EnableForCategory(command.CategotyId);
                break;

            case AlertNotificationControlPointCommandId.EnableUnreadCategoryStatusNotification:
                // Enable Unread Alert Status messages for the category specified in the Category ID field of the 
                // command.  If the category ID is specified as 0xff, all supported categories shall be enabled.
                unreadAlertStatus.EnableForCategory(command.CategotyId);
                break;

                case AlertNotificationControlPointCommandId.NotifyNewIncomgAlertImmediately:
                // Notify the New Alert characteristic to the client immediately for the category specified in the 
                // Category ID field if that category is enabled. If there are no new alerts for specified category ID
                // on the server, the value for the “Number of New Alert” field shall be set to 0. If the category ID 
                // is specified as 0xff, the New Alert characteristics for all currently enabled categories shall be
                // notified.
                newAlert.NotifyImmediatelyForCategory(command.CategotyId);
                break;

            case AlertNotificationControlPointCommandId.NotifyUnreadCategoryStatusImmedately:
                // Notify the Unread Alert Status characteristic to the client immediately for the category specified 
                // in the Category ID field if that category is enabled. If there are no unread alerts for specified 
                // category ID on the server, the value for the “Unread count” field shall be set to 0. If the category
                // ID is specified as 0xff, the Unread Alert Status characteristic(s) that covers all currently enabled
                // categories shall be notified.
                unreadAlertStatus.NotifyImmediatelyForCategory(command.CategotyId);
                break;
            }
        }

        /// <summary>
        /// Asynchronous initialization 
        /// </summary>
        /// <returns>Initialization task</returns>
        public override async Task Init()
        {
            await CreateServiceProvider(GattServiceUuids.AlertNotification);

            // Please refer Category ID bit mask https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.alert_category_id_bit_mask.xml
            // The value 0x21 is interpreted as "Simple Alert and SMS bits set"
            var categoryBitMask = new AlertCategoryIdBitMask(AlertCategoryIdBitMask_0.SimpleAlert);

            // SupportedNewAlertCategory
            GattLocalCharacteristicResult result = await ServiceProvider.Service.CreateCharacteristicAsync(
                GattCharacteristicUuids.SupportedNewAlertCategory,
                GetPlainReadParameterWithValue(categoryBitMask.Value));
            GattServicesHelper.GetCharacteristicsFromResult(result, ref supportNewAlert);

            // SupportedUnreadAlertCatagory
            result = await ServiceProvider.Service.CreateCharacteristicAsync(
                GattCharacteristicUuids.SupportUnreadAlertCategory,
               GetPlainReadParameterWithValue(categoryBitMask.Value));
            GattServicesHelper.GetCharacteristicsFromResult(result, ref supportUnreadAlert);

            // NewAlert
            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.NewAlert, PlainNotifyParameters);
            GattLocalCharacteristic newAlertCharacteristic = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref newAlertCharacteristic);
            if (newAlertCharacteristic != null)
            {
                NewAlert = new Characteristics.NewAlertCharacteristic(newAlertCharacteristic, this);
            }

            // UnreadAlertStatus
            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.UnreadAlertStatus, PlainNotifyParameters);
            GattLocalCharacteristic unreadAlertCharacteristic = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref unreadAlertCharacteristic);
            if (unreadAlertCharacteristic != null)
            {
                UnreadAlertStatus = new Characteristics.UnreadAlertStatusCharacteristic(unreadAlertCharacteristic, this);
            }

            // Control Point
            result = await ServiceProvider.Service.CreateCharacteristicAsync(
                GattCharacteristicUuids.AlertNotificationControlPoint,
                PlainWriteOrWriteWithoutRespondsParameter);
            GattLocalCharacteristic alertNotificationControlPointCharacteristic = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref alertNotificationControlPointCharacteristic);
            if (alertNotificationControlPointCharacteristic != null)
            {
                AlertNotificationControlPoint = new Characteristics.AlertNotificationControlPointCharacteristic(alertNotificationControlPointCharacteristic, this);
            }
        }

        public void NewAlertArrived(String name)
        {
            this.UnreadCount++;
            this.LastNotification = new AlertNotification(name);
        }
    }
}
