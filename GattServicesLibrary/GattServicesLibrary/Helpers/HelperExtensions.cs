// <copyright file="HelperExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

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

    public static class DispatcherTaskExtensions
    {
        public static async Task<T> RunTaskAsync<T>(
            this CoreDispatcher dispatcher,
            Func<Task<T>> func,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static async Task RunTaskAsync(this CoreDispatcher dispatcher,
            Func<Task> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
            await RunTaskAsync(dispatcher, async () => { await func(); return false; }, priority);
    }
}
