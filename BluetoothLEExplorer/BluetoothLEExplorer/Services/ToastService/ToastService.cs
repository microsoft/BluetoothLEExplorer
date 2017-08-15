// <copyright file="ToastService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;

namespace BluetoothLEExplorer.Services.ToastService
{
    /// <summary>
    /// Service to help displaying toast notifications
    /// </summary>
    public static class ToastService
    {
        /// <summary>
        /// Pop up a toast
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns>A toast notification</returns>
        public static ToastNotification PopToast(string title, string content)
        {
            return PopToast(title, content, null, null);
        }

        /// <summary>
        /// Pop up a toast
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="tag"></param>
        /// <param name="group"></param>
        /// <returns>A toast notification</returns>
        public static ToastNotification PopToast(string title, string content, string tag, string group)
        {
            string xml = $@"<toast activationType='foreground'>
                                            <visual>
                                                <binding template='ToastGeneric'>
                                                </binding>
                                            </visual>
                                        </toast>";

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            var binding = doc.SelectSingleNode("//binding");

            var el = doc.CreateElement("text");
            el.InnerText = title;

            binding.AppendChild(el);

            el = doc.CreateElement("text");

            try
            { 
                el.InnerText = content;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                el.InnerText = "Undisplayable UTF8 character";
            }

            binding.AppendChild(el);

            return PopCustomToast(doc, tag, group);
        }

        /// <summary>
        /// Pop up a task
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>A toast notification</returns>
        public static ToastNotification PopCustomToast(string xml)
        {
            return PopCustomToast(xml, null, null);
        }

        /// <summary>
        /// Pop up a custom toast
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="tag"></param>
        /// <param name="group"></param>
        /// <returns>A toast notification</returns>
        public static ToastNotification PopCustomToast(string xml, string tag, string group)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return PopCustomToast(doc, tag, group);
        }

        /// <summary>
        /// pop up a custom toast
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tag"></param>
        /// <param name="group"></param>
        /// <returns>A toast notification</returns>
        [DefaultOverloadAttribute]
        public static ToastNotification PopCustomToast(XmlDocument doc, string tag, string group)
        {
            var toast = new ToastNotification(doc);

            if (tag != null)
            {
                toast.Tag = tag;
            }

            if (group != null)
            {
                toast.Group = group;
            }

            ToastNotificationManager.CreateToastNotifier().Show(toast);

            return toast;
        }
    }
}
