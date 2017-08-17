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

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Alert Notification Services
    /// </summary>
    public class AlertNotificationService : GenericGattService
    {
        /// <summary>
        /// Characteristic provides the count of new alerts (for a given category) in the server.
        /// </summary>
        private GattLocalCharacteristic newAlert;

        /// <summary>
        /// Characteristic exposes the count of unread alert events existing in the server.
        /// The count of unread alert events is provided with the Category ID.
        /// </summary>
        private GattLocalCharacteristic unreadAlertStatus;

        /// <summary>
        /// Characteristic allows the client device to enable/disable the alert notification of new alert and 
        /// unread alert events more selectively than can be done by setting or clearing the notification bit 
        /// in the Client Characteristic configuration for each alert characteristic.
        /// </summary>
        private GattLocalCharacteristic alertNotificationCtrlPnt;

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

        /// <summary>
        /// Starts the Alert Notification Services
        /// </summary>
        /// <param name="connectable">True, starts the service as Connectable. False, starts the service as only Discoverable</param>
        public override void Start(bool connectable)
        {
            // Please refer Category ID bit mask https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.alert_category_id_bit_mask.xml
            // The value 0x21 is interpreted as "Simple Alert and SMS bits set"
            StartWithValue(connectable, new byte[] { (byte)(AlertCategoryID.SimpleAlert | AlertCategoryID.SMS_MMS) });
        }

        /// <summary>
        /// Starts the Alert Notification Services for Specific categories
        /// </summary>
        /// <param name="connectable">True, starts the service as Connectable. False, starts the service as only Discoverable</param>
        /// <param name="value">Category ID</param>
        public async void StartWithValue(bool connectable, byte[] value)
        {
            await CreateServiceProvider(GattServiceUuids.AlertNotification);

            GattLocalCharacteristicResult result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.SupportedNewAlertCategory,
                                                                                                           GetPlainReadParameterWithValue(value));
            GattServicesHelper.GetCharacteristicsFromResult(result, ref supportNewAlert);

            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.SupportUnreadAlertCategory,
                                                                             GetPlainReadParameterWithValue(value));
            GattServicesHelper.GetCharacteristicsFromResult(result, ref supportUnreadAlert);

            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.NewAlert, PlainNotifyParameters);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref newAlert);

            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.UnreadAlertStatus, PlainNotifyParameters);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref unreadAlertStatus);

            result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.AlertNotificationControlPoint,
                                                                             PlainWriteOrWriteWithoutRespondsParameter);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref alertNotificationCtrlPnt);
            if (alertNotificationCtrlPnt != null)
            {
                alertNotificationCtrlPnt.WriteRequested += AlertNotificationCtrlPntOnWriteRequested;
            }

            // Once all characteristics have been added - publish to the system
            base.Start(connectable);
        }

        /// <summary>
        /// Stops the already running Alert Notification Services
        /// </summary>
        public override void Stop()
        {
            newAlert = null;
            unreadAlertStatus = null;
            if (alertNotificationCtrlPnt != null)
            {
                alertNotificationCtrlPnt.WriteRequested -= AlertNotificationCtrlPntOnWriteRequested;
                alertNotificationCtrlPnt = null;
            }

            supportUnreadAlert = null;
            supportNewAlert = null;
            base.Stop();
        }

        /// <summary>
        /// Notifies the client about the alert
        /// </summary>
        public async void NotifyValue()
        {
            IBuffer buffer = GattServicesHelper.ConvertValueToBuffer(1);
            await newAlert.NotifyValueAsync(buffer);
            await unreadAlertStatus.NotifyValueAsync(buffer);
        }

        /// <summary>
        /// Event handler for Alert Notification Control point
        /// </summary>
        /// <param name="sender">The source of the Write request</param>
        /// <param name="args">Details about the request</param>
        private void AlertNotificationCtrlPntOnWriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronous initialization 
        /// </summary>
        /// <returns>Initialization task</returns>
        public override Task Init()
        {
            throw new NotImplementedException();
        }
    }
}
