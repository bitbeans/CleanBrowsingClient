using CleanBrowsingClient.Config;
using CleanBrowsingClient.Models;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace CleanBrowsingClient.Helper
{
    /// <summary>
	///     Class to manage the local network interfaces.
	/// </summary>
	public static class LocalNetworkInterfaceManager
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(LocalNetworkInterfaceManager));

        /// <summary>
        ///     Get a list of the local network interfaces.
        /// </summary>
        /// <param name="listenAddresses"></param>
        /// <param name="showHiddenCards">Show hidden cards.</param>
        /// <param name="showOnlyOperationalUp">Include only connected network cards.</param>
        /// <returns>A (filtered) list of the local network interfaces.</returns>
        /// <exception cref="NetworkInformationException">A Windows system function call failed. </exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static List<LocalNetworkInterface> GetLocalNetworkInterfaces(List<string> listenAddresses = null, bool showHiddenCards = false,
            bool showOnlyOperationalUp = true)
        {
            var interfaces = new List<LocalNetworkInterface>();
            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (showOnlyOperationalUp)
                    {
                        if (nic.OperationalStatus != OperationalStatus.Up)
                        {
                            continue;
                        }
                    }

                    if (!showHiddenCards)
                    {
                        var add = true;
                        foreach (var blacklistEntry in Global.NetworkInterfaceBlacklist)
                        {
                            if (nic.Description.Contains(blacklistEntry) || nic.Name.Contains(blacklistEntry))
                            {
                                add = false;
                            }
                        }
                        if (!add) continue;
                    }
                    var localNetworkInterface = new LocalNetworkInterface
                    {
                        Name = nic.Name,
                        Description = nic.Description,
                        Type = nic.NetworkInterfaceType,
                        Dns = GetDnsServerList(nic.Id),
                        Ipv4Support = nic.Supports(NetworkInterfaceComponent.IPv4),
                        Ipv6Support = nic.Supports(NetworkInterfaceComponent.IPv6),
                        OperationalStatus = nic.OperationalStatus
                    };
                    //do a strict check if the interface supports IPv6
                    localNetworkInterface.UseDnsCrypt = IsUsingDnsCrypt(listenAddresses, localNetworkInterface, localNetworkInterface.Ipv6Support);
                    interfaces.Add(localNetworkInterface);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetLocalNetworkInterfaces");
            }
            return interfaces;
        }

        /// <summary>
        ///     Simple check if the network interface contains any of resolver addresses.
        /// </summary>
        /// <param name="listenAddresses"></param>
        /// <param name="localNetworkInterface">The interface to check.</param>
        /// <param name="strictCheck">All addresses must be set.</param>
        /// <returns><c>true</c> if a address was found, otherwise <c>false</c></returns>
        public static bool IsUsingDnsCrypt(List<string> listenAddresses, LocalNetworkInterface localNetworkInterface, bool strictCheck = false)
        {
            if (listenAddresses == null) return false;
            var addressOnly = (from listenAddress in listenAddresses
                               let lastIndex = listenAddress.LastIndexOf(":", StringComparison.Ordinal)
                               select listenAddress.Substring(0, lastIndex).Replace("[", "").Replace("]", "")).ToList();

            if (strictCheck)
            {
                return addressOnly.Intersect(localNetworkInterface.Dns.Select(x => x.Address).ToList()).Count() == addressOnly.Count();
            }
            return localNetworkInterface.Dns.Any(d => addressOnly.Contains(d.Address));
        }

        /// <summary>
        ///     Get the nameservers of an interface.
        /// </summary>
        /// <param name="localNetworkInterface">The interface to extract from.</param>
        /// <returns>A list of nameservers.</returns>
        internal static List<DnsServer> GetDnsServerList(string localNetworkInterface)
        {
            var serverAddresses = new List<DnsServer>();
            try
            {
                var registryKeyIpv6 = Registry.GetValue(
                        "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\TCPIP6\\Parameters\\Interfaces\\" +
                        localNetworkInterface,
                        "NameServer", "");
                var registryKeyIpv4 = Registry.GetValue(
                        "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" +
                        localNetworkInterface,
                        "NameServer", "");

                if (registryKeyIpv6 != null && registryKeyIpv6.ToString().Length > 0)
                {
                    var entries = ((string)registryKeyIpv6).Split(new[] { "," }, StringSplitOptions.None);
                    serverAddresses.AddRange(entries.Select(address => new DnsServer
                    {
                        Address = address,
                        Type = NetworkInterfaceComponent.IPv6
                    }));
                }

                if (registryKeyIpv4 != null && registryKeyIpv4.ToString().Length > 0)
                {
                    var entries = ((string)registryKeyIpv4).Split(new[] { "," }, StringSplitOptions.None);
                    serverAddresses.AddRange(entries.Select(address => new DnsServer
                    {
                        Address = address,
                        Type = NetworkInterfaceComponent.IPv4
                    }));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetDnsServerList");
            }
            return serverAddresses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unconvertedServers"></param>
        /// <returns></returns>
        public static List<DnsServer> ConvertToDnsList(List<string> unconvertedServers)
        {
            return (from unconvertedServer in unconvertedServers
                    let lastIndex = unconvertedServer.LastIndexOf(":", StringComparison.Ordinal)
                    select unconvertedServer.Substring(0, lastIndex)
                into addressOnly
                    select new DnsServer
                    {
                        Address = addressOnly.Replace("[", "").Replace("]", ""),
                        Type = addressOnly.Contains(":") ? NetworkInterfaceComponent.IPv6 : NetworkInterfaceComponent.IPv4
                    }).ToList();
        }

        public static bool UnsetNameservers(LocalNetworkInterface localNetworkInterface)
        {
            return SetNameservers(localNetworkInterface, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localNetworkInterface"></param>
        /// <param name="dnsServers"></param>
        /// <returns></returns>
        public static bool SetNameservers(LocalNetworkInterface localNetworkInterface, List<DnsServer> dnsServers)
        {
            var succedded = true;
            try
            {
                if (dnsServers == null)
                {
                    dnsServers = new List<DnsServer>();
                }

                var delete4 = ExecuteWithArguments("interface ipv4 delete dns \"" + localNetworkInterface.Name + "\" all");
                if (delete4 != null)
                {
                    if (!delete4.Success)
                    {
                        Logger.Warning("failed to delete DNS (IPv4)");
                        succedded = false;
                    }
                    else
                    {
                        Logger.Information("delete DNS (IPv4) succeeded");
                    }
                }
                else
                {
                    Logger.Warning("failed to delete DNS (IPv4)");
                    succedded = false;
                }

                var delete6 = ExecuteWithArguments("interface ipv6 delete dns \"" + localNetworkInterface.Name + "\" all");
                if (delete6 != null)
                {
                    if (!delete6.Success)
                    {
                        Logger.Warning("failed to delete DNS (IPv6)");
                        succedded = false;
                    }
                    else
                    {
                        Logger.Information("delete DNS (IPv6) succeeded");
                    }
                }
                else
                {
                    Logger.Warning("failed to delete DNS (IPv6)");
                    succedded = false;
                }

                foreach (var dnsServer in dnsServers)
                {
                    if (dnsServer.Type == NetworkInterfaceComponent.IPv4)
                    {
                        var add4 = ExecuteWithArguments("interface ipv4 add dns \"" + localNetworkInterface.Name + "\" " + dnsServer.Address + " validate=no");
                        if (add4 != null)
                        {
                            if (!add4.Success)
                            {
                                Logger.Warning("failed to add DNS (IPv4)");
                                succedded = false;
                            }
                            else
                            {
                                Logger.Information("add DNS (IPv4) succeeded");
                            }
                        }
                        else
                        {
                            Logger.Warning("failed to add DNS (IPv4)");
                            succedded = false;
                        }
                    }
                    else
                    {
                        var add6 = ExecuteWithArguments("interface ipv6 add dns \"" + localNetworkInterface.Name + "\" " + dnsServer.Address + " validate=no");
                        if (add6 != null)
                        {
                            if (!add6.Success)
                            {
                                Logger.Warning("failed to add DNS (IPv6)");
                                succedded = false;
                            }
                            else
                            {
                                Logger.Information("add DNS (IPv6) succeeded");
                            }
                        }
                        else
                        {
                            Logger.Warning("failed to add DNS (IPv6)");
                            succedded = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "SetNameservers");
            }
            return succedded;
        }

        private static ProcessResult ExecuteWithArguments(string arguments)
        {
            var processResult = new ProcessResult();
            try
            {
                const int timeout = 9000;
                using var process = new Process();
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                var output = new StringBuilder();
                var error = new StringBuilder();

                using var outputWaitHandle = new AutoResetEvent(false);
                using var errorWaitHandle = new AutoResetEvent(false);
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        error.AppendLine(e.Data);
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    if (process.ExitCode == 0)
                    {
                        processResult.StandardOutput = output.ToString();
                        processResult.StandardError = error.ToString();
                        processResult.Success = true;
                    }
                    else
                    {
                        processResult.StandardOutput = output.ToString();
                        processResult.StandardError = error.ToString();
                        processResult.Success = false;
                    }
                }
                else
                {
                    // Timed out.
                    throw new Exception("Timed out");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "ExecuteWithArguments");
                processResult.StandardError = exception.Message;
                processResult.Success = false;
            }
            return processResult;
        }
    }
}
