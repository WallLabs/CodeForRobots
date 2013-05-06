﻿using System;
using System.Net.NetworkInformation;

namespace MrdsToolkit.Windows.Services
{
    /// <summary>
    /// Networking extensions.
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Gets the hostname and any domain (the FQDN) of the local computer.
        /// </summary>
        public static string GetFullHostName()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return String.IsNullOrWhiteSpace(ipProperties.DomainName)
                       ? ipProperties.HostName
                       : ipProperties.HostName + "." + ipProperties.DomainName;
        }
    }
}