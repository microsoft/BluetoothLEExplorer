// <copyright file="GenericGattCharacteristic.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.Foundation;

namespace GattServicesLibrary
{
    /// <summary>
    /// Base class for any characteristic. This handles basic responds for read/write and supplies method to 
    /// notify or indicate clients. 
    /// </summary>
    public abstract class GenericGattCharacteristic : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged requirements
        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed method
        /// </summary>
        /// <param name="e">Property that changed</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion

        /// <summary>
        /// Source of <see cref="Characteristic"/> 
        /// </summary>
        private GattLocalCharacteristic characteristic;

        /// <summary>
        /// Gets or sets <see cref="characteristic"/>  that is wrapped by this class
        /// </summary>
        public GattLocalCharacteristic Characteristic
        {
            get
            {
                return characteristic;
            }

            set
            {
                if (characteristic != value)
                {
                    characteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Characteristic"));
                }
            }
        }

        /// <summary>
        /// Gets parent service that this characteristic belongs to
        /// </summary>
        public GenericGattService ParentService
        {
            get; private set;
        }

        /// <summary>
        /// Source of <see cref="Value"/> 
        /// </summary>
        private IBuffer value;

        /// <summary>
        /// Gets or sets the Value of the characteristic
        /// </summary>
        public IBuffer Value
        {
            get
            {
                return value;
            }

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Value"));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGattCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public GenericGattCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service)
        {
            Characteristic = characteristic;
            ParentService = service;

            if (Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                Characteristic.ReadRequested += Characteristic_ReadRequested;
            }

            if (Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write) ||
                Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
            {
                Characteristic.WriteRequested += Characteristic_WriteRequested;
            }

            Characteristic.SubscribedClientsChanged += Characteristic_SubscribedClientsChanged;
        }

        /// <summary>
        /// Base implementation when number of subscribers changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Characteristic_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            Debug.WriteLine("Subscribers: {0}", sender.SubscribedClients.Count());
        }

        /// <summary>
        /// Base implementation to Notify or Indicate clients 
        /// </summary>
        public virtual async void NotifyValue()
        {
            // If parent service is not publishing we shouldn't try to notify
            if (ParentService.IsPublishing == false)
            {
                return;
            }

            bool notify = Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
            bool indicate = Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate);

            if (notify || indicate)
            {
                Debug.WriteLine($"NotifyValue executing: Notify = {notify}, Indicate = {indicate}");
                await Characteristic.NotifyValueAsync(Value);
            }
            else
            {
                Debug.WriteLine("NotifyValue was called but CharacteristicProperties don't include Notify or Indicate");
            }
        }

        /// <summary>
        /// Base implementation for the read callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Characteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            // Grab the event deferral before performing any async operations in the handler.
            var deferral = args.GetDeferral();

            Debug.WriteLine($"({this.GetType()})Entering base.Characteristic_ReadRequested");

            // In order to get the remote request, access to the device must be provided by the user.
            // This can be accomplished by calling BluetoothLEDevice.RequestAccessAsync(), or by getting the request on the UX thread.
            //
            // Note that subsequent calls to RequestAccessAsync or GetRequestAsync for the same device do not need to be called on the UX thread.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(
                async () =>
                {
                    var request = await args.GetRequestAsync();

                    Debug.WriteLine($"Characteristic_ReadRequested - Length {request.Length}, State: {request.State}, Offset: {request.Offset}");

                    if (!ReadRequested(args.Session, request))
                    {
                        request.RespondWithValue(Value);
                    }

                    deferral.Complete();
                });
        }

        protected virtual bool ReadRequested(GattSession session, GattReadRequest request)
        {
            Debug.WriteLine("Request not completed by derrived class.");
            return false;
        }

        /// <summary>
        /// Base implementation for the write callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Characteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            Debug.WriteLine("Characteristic_WriteRequested: Write Requested");

            // Grab the event deferral before performing any async operations in the handler.
            var deferral = args.GetDeferral();

            // In order to get the remote request, access to the device must be provided by the user.
            // This can be accomplished by calling BluetoothLEDevice.RequestAccessAsync(), or by getting the request on the UX thread.
            //
            // Note that subsequent calls to RequestAccessAsync or GetRequestAsync for the same device do not need to be called on the UX thread.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(
                async () =>
                {
                    // Grab the request
                    var request = await args.GetRequestAsync();

                    Debug.WriteLine($"Characteristic_WriteRequested - Length {request.Value.Length}, State: {request.State}, Offset: {request.Offset}");

                    if (!WriteRequested(args.Session, request))
                    {
                        // Set the characteristic Value
                        Value = request.Value;

                        // Respond with completed
                        if (request.Option == GattWriteOption.WriteWithResponse)
                        {
                            Debug.WriteLine("Characteristic_WriteRequested: Completing request with responds");
                            request.Respond();
                        }
                        else
                        {
                            Debug.WriteLine("Characteristic_WriteRequested: Completing request without responds");
                        }
                    }

                    // everything below this is debug. Should implement this on non-UI thread based on
                    // https://github.com/Microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md
                    byte[] data;
                    CryptographicBuffer.CopyToByteArray(Value, out data);

                    if (data == null)
                    {
                        Debug.WriteLine("Characteristic_WriteRequested: Value after write complete was NULL");
                    }
                    else
                    {
                        Debug.WriteLine($"Characteristic_WriteRequested: New Value: {data.BytesToString()}");
                    }

                    deferral.Complete();
                });
        }

        protected virtual bool WriteRequested(GattSession session, GattWriteRequest request)
        {
            Debug.WriteLine("Request not completed by derrived class.");
            return false;
        }
    }
}
