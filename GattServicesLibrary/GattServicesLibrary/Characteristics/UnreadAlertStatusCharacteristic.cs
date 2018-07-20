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

            enabled = false;
        }

        public /*async*/ void EnableForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

            enabled = true;
        }

        public /*async*/ void NotifyImmediatelyForCategory(AlertCategoryId categoryId)
        {
            if ((categoryId != AlertCategoryId.SimpleAlert) && (categoryId != AlertCategoryId.All))
            {
                return;
            }

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
