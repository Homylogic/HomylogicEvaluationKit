/* HOMYLOGIC SYTEM INFO
 * 
 * Vráti informácie o hardweri a aplikácii.
 * 
 * 
 */ 
using System;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;
using System.Reflection;

namespace X.Homylogic
{
    public static class SystemInfo
    {
        const string TITLE = "System information";

        public enum OSVersionTypes { Unknown, Windows, Linux, Mac }
        /// <summary>
        /// Vráti verziu operačného sytému.
        /// </summary>
        public static OSVersionTypes OSVersion 
        {
            get 
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSVersionTypes.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSVersionTypes.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSVersionTypes.Mac;
                return OSVersionTypes.Unknown;                   
            }
        }
        /// <summary>
        /// Vráti jedinečný identifikátor základnej dosky zariadenia na ktorom beží aplikácia.
        /// </summary>
        public static String DeviceSerialNumber 
        {
            get 
            {
                try
                {
                    if (OSVersion == OSVersionTypes.Windows) 
                    {
                        string mbInfo = String.Empty;
                        ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
                        scope.Connect();
                        ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());
                        foreach (PropertyData propData in wmiClass.Properties)
                        {
                            if (propData.Name == "SerialNumber")
                                return Convert.ToString(propData.Value);
                        }
                    }
                    if (OSVersion == OSVersionTypes.Linux) 
                    { 
                        return Unosquare.RaspberryIO.Pi.Info.Hardware;
                    }
                }
                catch (Exception ex)
                {
                    Body.Environment.Logs.Error("Error while gathering device system information.", ex, TITLE);
                }
                return null;
            }
        }
        /// <summary>
        /// Vráti voľné miesto na disku na ktorom beží aplikácia.
        /// </summary>
        public static long FreeSpace()
        {
            string drive = Path.GetPathRoot(Assembly.GetEntryAssembly().Location);
            DriveInfo info = new DriveInfo(drive);
            return info.AvailableFreeSpace;
        }

    }
}
