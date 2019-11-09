using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace Uninstall
{
    internal class Program
    {
        private const string DnsCryptProxyFolder = "dnscrypt-proxy";
        private const string DnsCryptProxyExecutableName = "dnscrypt-proxy.exe";

        static void Main(string[] args)
        {
            try
            {
                ClearLocalNetworkInterfaces();
                StopService();
                Thread.Sleep(500);
                UninstallService();
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
		///		 Clear all network interfaces.
		/// </summary>
		internal static void ClearLocalNetworkInterfaces()
        {
            try
            {
                string[] networkInterfaceBlacklist =
                {
                    "Microsoft Virtual",
                    "Hamachi Network",
                    "VMware Virtual",
                    "VirtualBox",
                    "Software Loopback",
                    "Microsoft ISATAP",
                    "Microsoft-ISATAP",
                    "Teredo Tunneling Pseudo-Interface",
                    "Microsoft Wi-Fi Direct Virtual",
                    "Microsoft Teredo Tunneling Adapter",
                    "Von Microsoft gehosteter",
                    "Microsoft hosted",
                    "Virtueller Microsoft-Adapter",
                    "TAP"
                };

                var networkInterfaces = new List<NetworkInterface>();
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }
                    foreach (var blacklistEntry in networkInterfaceBlacklist)
                    {
                        if (nic.Description.Contains(blacklistEntry) || nic.Name.Contains(blacklistEntry)) continue;
                        if (!networkInterfaces.Contains(nic))
                        {
                            networkInterfaces.Add(nic);
                        }
                    }
                }

                foreach (var networkInterface in networkInterfaces)
                {
                    using var process = new Process();
                    Console.WriteLine("clearing {0}", networkInterface.Name);
                    ExecuteWithArguments("netsh", "interface ipv4 delete dns \"" + networkInterface.Name + "\" all");
                    ExecuteWithArguments("netsh", "interface ipv6 delete dns \"" + networkInterface.Name + "\" all");
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
		///		Stop the dnscrypt-proxy service.
		/// </summary>
		internal static void StopService()
        {
            Console.WriteLine("stopping dnscrypt service");
            var dnsCryptProxy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DnsCryptProxyFolder, DnsCryptProxyExecutableName);
            ExecuteWithArguments(dnsCryptProxy, "-service stop");
        }

        /// <summary>
		///		Uninstall the dnscrypt-proxy service.
		/// </summary>
		internal static void UninstallService()
        {
            Console.WriteLine("removing dnscrypt service");
            var dnsCryptProxy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DnsCryptProxyFolder, DnsCryptProxyExecutableName);
            ExecuteWithArguments(dnsCryptProxy, "-service uninstall");
            Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application\dnscrypt-proxy", false);
        }

        private static void ExecuteWithArguments(string filename, string arguments)
        {
            try
            {
                const int timeout = 9000;
                using var process = new Process();
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                if (process.WaitForExit(timeout))
                {
                    if (process.ExitCode == 0)
                    {
                        //do nothing
                    }
                }
                else
                {
                    // Timed out.
                    throw new Exception("Timed out");
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
