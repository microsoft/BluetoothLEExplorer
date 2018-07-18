// <copyright file="UnreadAlertStatus.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Power;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using GattServicesLibrary.CharacteristicParameterValues;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using GattHelper.Converters;
using GattServicesLibrary.Services;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Implementation of the battery profile
    /// </summary>
    public class UnreadAlertStatusCharacteristic : GenericGattCharacteristic
    {
        private Windows.UI.Notifications.Management.UserNotificationListener notificationListener;
        private bool enabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnreadAlertStatusCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">The characteristic that this wraps</param>
        public UnreadAlertStatusCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            notificationListener = UserNotificationListener.Current;
        }

        public void DisableForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

            //notificationListener.NotificationChanged -= NotificationListener_NotificationChanged;

            enabled = false;
        }

        public /*async*/ void EnableForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

            //if (notificationListener.GetAccessStatus() != UserNotificationListenerAccessStatus.Allowed)
            //{
            //    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            //        CoreDispatcherPriority.Normal,
            //        async () =>
            //        {
            //            var status = await notificationListener.RequestAccessAsync();
            //        });
            //}

            //notificationListener.NotificationChanged += NotificationListener_NotificationChanged;

            enabled = true;
        }

        //private async void NotificationListener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        //{
        //    var notifications = await notificationListener.GetNotificationsAsync(NotificationKinds.Toast);
        //    var value = new byte[] { (byte)AlertCategoryId.SimpleAlert, Convert.ToByte(notifications.Count) };
        //    Value = GattConvert.ToIBuffer(value);
        //    NotifyValue();
        //}

        public /*async*/ void NotifyImmediatelyForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

            //if (notificationListener.GetAccessStatus() != UserNotificationListenerAccessStatus.Allowed)
            //{
            //    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            //        CoreDispatcherPriority.Normal,
            //        async () =>
            //        {
            //            var status = await notificationListener.RequestAccessAsync();
            //        });
            //}

            //var notifications = await notificationListener.GetNotificationsAsync(NotificationKinds.Toast);

            var service = base.ParentService as AlertNotificationService;

            var value = new byte[] { (byte)AlertCategoryId.SimpleAlert, Convert.ToByte(service.UnreadCount) };
            Value = GattConvert.ToIBuffer(value);
            base.NotifyValue();
        }

        public override void NotifyValue()
        {
            if (!enabled)
            {
                return;
            }

            NotifyImmediatelyForCategory(AlertCategoryId.SimpleAlert);
        }
    }
}
