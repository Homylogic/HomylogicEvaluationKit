/* CONFIGURATION MANAGER SETTINGS
 * 
 * Umožňuje prácu s nastaveniami uloženými v súbore pri aplikácii.
 * Nachádajú sa v súbore: [AppName].dll.config
 * 
 */
using System;
using System.Configuration;

namespace X.App.Settings
{
    /// <summary>
    /// Nastavenia uložené v konfiguračnom súbore, vhodné pre UI alebo nejaké jednoduché hodnoty.
    /// </summary>
    public static class ConfigFile
    {
        /// <summary>
        /// Zapísanie hodnoty do nastavení.
        /// </summary>
        public static void Write(string key, string value) 
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = configFile.AppSettings.Settings;
            if (appSettings[key] == null)
            {
                appSettings.Add(key, value);
            }
            else
            {
                appSettings[key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }


        /// <summary>
        /// Načítanie hodnoty z nastavení.
        /// </summary>
        public static string Read(string key, string defaultValue = null) 
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings[key] ?? defaultValue;
        }


            



        // -------------------------- SAMPLE FOR READ ALL SETTINGS ----------------------------------------------------------------------------------------------------------
        /*
        static void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }
        */
    }
}
