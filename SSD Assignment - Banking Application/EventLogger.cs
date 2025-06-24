using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.Versioning;

namespace Banking_Application
{
    [SupportedOSPlatform("windows")]
    public static class EventLogger
    {
        private const string EVENT_SOURCE = "SSD Banking Application";
        private const string EVENT_LOG = "Application";

        // Initialize the event source if it doesn't exist
        static EventLogger()
        {
            try
            {
#if WINDOWS
                if (!EventLog.SourceExists(EVENT_SOURCE))
                {
                    EventLog.CreateEventSource(EVENT_SOURCE, EVENT_LOG);
                }
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create event source: {ex.Message}");
            }
        }

        public static void LogTransaction(TransactionType transactionType, string tellerName,
            string accountNumber, string accountHolderName, double? amount = null, string reason = null)
        {
            try
            {
                string who1 = tellerName ?? "Unknown Teller";
                string who2 = $"Account: {accountNumber}, Holder: {accountHolderName}";
                string what = GetTransactionDescription(transactionType, amount);
                string where = GetDeviceIdentifier();
                string when = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string why = reason ?? "N/A";
                string how = GetApplicationMetadata();

                string logMessage = $"BANKING TRANSACTION LOG\n" +
                                    $"WHO (Teller): {who1}\n" +
                                    $"WHO (Account): {who2}\n" +
                                    $"WHAT: {what}\n" +
                                    $"WHERE: {where}\n" +
                                    $"WHEN: {when}\n" +
                                    $"WHY: {why}\n" +
                                    $"HOW: {how}";

#if WINDOWS
                EventLog.WriteEntry(EVENT_SOURCE, logMessage, EventLogEntryType.Information);
#else
                Console.WriteLine("EventLog simulation (WINDOWS only):\n" + logMessage);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not write to event log: {ex.Message}");
            }
        }

        public static void LogAuthenticationAttempt(string username, bool successful, string reason = null)
        {
            try
            {
                string eventType = successful ? "SUCCESSFUL LOGIN" : "FAILED LOGIN";
                string logMessage = $"AUTHENTICATION LOG\n" +
                                    $"WHO: {username ?? "Unknown"}\n" +
                                    $"WHAT: {eventType}\n" +
                                    $"WHERE: {GetDeviceIdentifier()}\n" +
                                    $"WHEN: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                    $"WHY: {reason ?? "N/A"}\n" +
                                    $"HOW: {GetApplicationMetadata()}";

#if WINDOWS
                EventLogEntryType entryType = successful ? EventLogEntryType.Information : EventLogEntryType.Warning;
                EventLog.WriteEntry(EVENT_SOURCE, logMessage, entryType);
#else
                Console.WriteLine("Authentication EventLog simulation (WINDOWS only):\n" + logMessage);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not write authentication log: {ex.Message}");
            }
        }

        private static string GetTransactionDescription(TransactionType type, double? amount)
        {
            return type switch
            {
                TransactionType.AccountCreation => "Account Creation",
                TransactionType.AccountClosure => "Account Closure",
                TransactionType.BalanceQuery => "Balance Query",
                TransactionType.Lodgement => $"Lodgement - Amount: €{amount:F2}",
                TransactionType.Withdrawal => $"Withdrawal - Amount: €{amount:F2}",
                _ => "Unknown Transaction"
            };
        }

        private static string GetDeviceIdentifier()
        {
            try
            {
                string macAddress = GetMacAddress();
                if (!string.IsNullOrEmpty(macAddress))
                    return $"MAC: {macAddress}";

                string ipAddress = GetLocalIPAddress();
                if (!string.IsNullOrEmpty(ipAddress))
                    return $"IP: {ipAddress}";

                string sid = GetWindowsSID();
                return $"SID: {sid}";
            }
            catch
            {
                return "Device ID: Unable to determine";
            }
        }

        private static string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        if (ni.OperationalStatus == OperationalStatus.Up)
                        {
                            return ni.GetPhysicalAddress().ToString();
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetWindowsSID()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User?.ToString() ?? "Unknown SID";
            }
            catch
            {
                return "Unknown SID";
            }
        }

        private static string GetApplicationMetadata()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version?.ToString() ?? "Unknown";
                var location = assembly.Location;

                string hash = "Unknown";
                try
                {
                    if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    {
                        using (var sha256 = SHA256.Create())
                        {
                            var hashBytes = sha256.ComputeHash(File.ReadAllBytes(location));
                            hash = Convert.ToHexString(hashBytes)[..16]; // First 16 chars for brevity
                        }
                    }
                }
                catch { }

                return $"App: SSD Banking Application, Version: {version}, Hash: {hash}";
            }
            catch
            {
                return "App: SSD Banking Application, Version: Unknown, Hash: Unknown";
            }
        }
    }

    public enum TransactionType
    {
        AccountCreation,
        AccountClosure,
        BalanceQuery,
        Lodgement,
        Withdrawal,
        LoginSuccess,
        LoginFailure,
    }
}
