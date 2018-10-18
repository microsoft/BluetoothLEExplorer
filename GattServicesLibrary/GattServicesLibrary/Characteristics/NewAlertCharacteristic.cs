// <copyright file="NewAlertCharacteristic.cs" company="Microsoft Corporation">
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
    public class NewAlertCharacteristic : GenericGattCharacteristic
    {
        private UserNotificationListener notificationListener;

        private bool enabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewAlertCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">The characteristic that this wraps</param>
        public NewAlertCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            notificationListener = UserNotificationListener.Current;
        }

        public void DisableForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

            enabled = false;

            //notificationListener.NotificationChanged -= NotificationListener_NotificationChanged;
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

        //    var writer = new DataWriter();
        //    writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
        //    writer.WriteByte((byte)AlertCategoryId.SimpleAlert);
        //    writer.WriteByte(Convert.ToByte(notifications.Count));
        //    if (notifications.Count > 0)
        //    {
        //        writer.WriteString(notifications.Last().Id.ToString());
        //    }
        //    else
        //    {
        //        writer.WriteString("");
        //    }

        //    Value = writer.DetachBuffer();
        //    NotifyValue();
        //}

        public async void NotifyImmediatelyForCategory(AlertCategoryId categoryId)
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

            var writer = new DataWriter();
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.WriteByte((byte)AlertCategoryId.SimpleAlert);
            writer.WriteByte(Convert.ToByte(service.UnreadCount));
            if (service.UnreadCount > 0)
            {
                writer.WriteString(service.LastNotification.Name.ToString());
            }
            else
            {
                writer.WriteString("");
            }

            Value = writer.DetachBuffer();

            base.NotifyValue();
        }

        public override void NotifyValue()
        {
            if (!enabled)
            {
                return;
            }

            var service = base.ParentService as AlertNotificationService;
            service.NewAlertArrived("Generated Alert");

            NotifyImmediatelyForCategory(AlertCategoryId.SimpleAlert);
        }
    }
}
