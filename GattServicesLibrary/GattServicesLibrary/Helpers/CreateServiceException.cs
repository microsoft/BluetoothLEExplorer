// <copyright file="CreateServiceException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace GattServicesLibrary.Helpers
{
    /// <summary>
    /// Class for Create Service Exception
    /// </summary>
    public class CreateServiceException : Exception
    {
        /// <summary>
        /// Gets the value indicating the create service exception details
        /// </summary>
        public GattServiceProviderResult CreateServiceExceptionResult { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateServiceException"/> class
        /// </summary>
        /// <param name="createServiceExceptionResult">Gatt Service provider result</param>
        public CreateServiceException(GattServiceProviderResult createServiceExceptionResult) : 
                                      base(string.Format($"Error occured while creating the provider, Error Code:{createServiceExceptionResult.Error}"))
        {
            CreateServiceExceptionResult = createServiceExceptionResult;
        }
    }
}
