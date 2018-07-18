// <copyright file="AlertNotificationControlPointCharacteristic.cs" company="Microsoft Corporation">
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
using System.Runtime.InteropServices.WindowsRuntime;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Implementation of the battery profile
    /// </summary>
    public class AlertNotificationControlPointCharacteristic : GenericGattCharacteristic
    {
        public struct AlertNotificationControlPointCommand
        {
            public GattServicesLibrary.CharacteristicParameterValues.AlertNotificationControlPointCommandId CommandId;
            public GattServicesLibrary.CharacteristicParameterValues.AlertCategoryId CategotyId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertNotificationControlPointCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">The characteristic that this wraps</param>
        public AlertNotificationControlPointCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
        }

        protected override bool WriteRequested(GattSession session, GattWriteRequest request)
        {
            var service = base.ParentService as GattServicesLibrary.Services.AlertNotificationService;
            if (service != null)
            {
                AlertNotificationControlPointCommand command = new AlertNotificationControlPointCommand();
                var byteAccess = request.Value.ToArray();
                command.CommandId = (CharacteristicParameterValues.AlertNotificationControlPointCommandId)byteAccess[0];
                command.CategotyId = (CharacteristicParameterValues.AlertCategoryId)byteAccess[1];

                service.ProcessCommand(command);

                Value = request.Value;
                request.Respond();
                return true;
            }
            return false;
        }
    }
}
