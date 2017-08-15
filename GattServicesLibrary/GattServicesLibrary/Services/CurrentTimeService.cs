// <copyright file="CurrentTimeService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Current time service
    /// </summary>
    public class CurrentTimeService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Current Time Service";
            }
        }

        /// <summary>
        /// Current time characteristics
        /// </summary>
        private GattLocalCharacteristic currentTime;

        /// <summary>
        /// Starts the Current time service
        /// </summary>
        /// <param name="connectable">True, starts the service as Connectable. False, starts the service as only Discoverable</param>
        public override async void Start(bool connectable)
        {
            await CreateServiceProvider(GattServiceUuids.CurrentTime);
            GattLocalCharacteristicResult result = await ServiceProvider.Service.CreateCharacteristicAsync(GattCharacteristicUuids.CurrentTime, 
                                                                                                           PlainReadNotifyParameters);
            GattServicesHelper.GetCharacteristicsFromResult(result, ref currentTime);
            if (currentTime != null)
            {
                currentTime.ReadRequested += ReadCharacteristicReadRequested;
            }

            base.Start(connectable);
        }

        /// <summary>
        /// Event handler for reading Current time
        /// </summary>
        /// <param name="sender">The source of the Write request</param>
        /// <param name="args">Details about the request</param>
        private async void ReadCharacteristicReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            var request = await args.GetRequestAsync();
            request.RespondWithValue(GattServicesHelper.ConvertValueToBuffer(DateTime.Now));
        }

        /// <summary>
        /// Event handler for notifying the current time characteristics value
        /// </summary>
        public async void NotifyValue()
        {
            await currentTime.NotifyValueAsync(GattServicesHelper.ConvertValueToBuffer(DateTime.Now));
        }

        /// <summary>
        /// Stops the already running Current time services
        /// </summary>
        public override void Stop()
        {
            if (currentTime != null)
            {
                currentTime.ReadRequested -= ReadCharacteristicReadRequested;
                currentTime = null;
            }

            base.Stop();
        }

        /// <summary>
        /// Asynchronous initialization
        /// </summary>
        /// <returns>Initialization Task</returns>
        public override Task Init()
        {
            throw new NotImplementedException();
        }
    }
}