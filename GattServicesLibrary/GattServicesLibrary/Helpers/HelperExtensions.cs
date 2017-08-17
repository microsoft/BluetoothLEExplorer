// <copyright file="HelperExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Text;

namespace GattServicesLibrary.Helpers
{
    /// <summary>
    /// Extension class for byte
    /// </summary>
    public static class HelperExtensions
    {
        /// <summary>
        /// Converts byte array to string
        /// </summary>
        /// <param name="array">Byte array to covert</param>
        /// <returns>string equivalent of the byte array</returns>
        public static string BytesToString(this byte[] array)
        {
            var result = new StringBuilder();

            for (int i = 0; i < array.Length; i++)
            {
                result.Append($"{array[i]:X2}");
                if (i < array.Length - 1)
                {
                    result.Append(" ");
                }
            }

            return result.ToString();
        }
    }
}
