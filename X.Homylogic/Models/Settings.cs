/* HOMYLOGIC SETTINGS
 * 
 * Obsahuje všetky natavenia programu.
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using X.App.Settings;
using X.Data;

namespace X.Homylogic.Models
{
    public sealed class Settings : SettingsList
    {
        #region --- HOME --

        public sealed class HomeSettings 
        {
            public enum BackgroundImageTypes : Int32 { None = 0, Custom = 1, Blue = 5, Orange = 6, Green = 7 }
            /// <summary>
            /// Nastavenie obrázka na hlavnej obrazovke.
            /// </summary>
            public BackgroundImageTypes BackgroundImage { get; set; } = BackgroundImageTypes.Blue; 
        }

        #endregion

        #region --- SECURITY --

        public sealed class SecuritySettings
        {
            /// <summary>
            /// Heslo pre prístup k nastaveniam a runtime.
            /// </summary>
            public String Password { get; set; } = null;
        }

        #endregion

        public HomeSettings Home { get; private set; } = new HomeSettings();
        public SecuritySettings Security { get; private set; } = new SecuritySettings();
        public Settings(DBClient dbClient) : base(dbClient) { }

        /// <summary>
        /// Načíta nastavenia.
        /// </summary>
        public void Load() 
        {
            this.LoadData();
            foreach (SettingsRecord settingsRecord in this.List) 
            {
                switch (settingsRecord.Section) 
                {
                    case "home": this.LoadHomeSettings(settingsRecord); break;
                }
            }
            foreach (SettingsRecord settingsRecord in this.List)
            {
                switch (settingsRecord.Section)
                {
                    case "security": this.LoadSecuritySettings(settingsRecord); break;
                }
            }
        }
        /// <summary>
        /// Uloži nastavenia.
        /// </summary>
        public void Save()
        {
            // Načítaj už existujúce (už uložené) nastavenia.
            Hashtable hashtable = new Hashtable();    
            foreach (SettingsRecord settingsRecord in this.List)
            {
                hashtable.Add($"{settingsRecord.Section}:{settingsRecord.Key}", settingsRecord);
            }

            // Uloženie všetkých nastavení.
            this.SaveHomeSettings(hashtable);
            this.SaveSecuritySettings(hashtable);
        }
        private void LoadHomeSettings(SettingsRecord settingsRecord)
        {
            switch (settingsRecord.Key)
            {
                case "background_image": this.Home.BackgroundImage = (HomeSettings.BackgroundImageTypes)settingsRecord.Value.Int32; break;
            }
        }
        private void LoadSecuritySettings(SettingsRecord settingsRecord)
        {
            switch (settingsRecord.Key)
            {
                case "password": this.Security.Password = settingsRecord.Value.Text; break;
            }
        }
        private void SaveHomeSettings(Hashtable hashtable)
        {
            SettingsRecord settingsRecord;
            // BackgroundImage
            settingsRecord = this.GetSettingsRecord(hashtable, "home", "background_image");
            settingsRecord.Value.SetValue((Int32)this.Home.BackgroundImage);
            settingsRecord.Save();
        }
        private void SaveSecuritySettings(Hashtable hashtable)
        {
            SettingsRecord settingsRecord;
            // Password
            settingsRecord = this.GetSettingsRecord(hashtable, "security", "password");
            settingsRecord.Value.SetValue(this.Security.Password);
            settingsRecord.Save();
        }
        private SettingsRecord GetSettingsRecord(Hashtable hashtable, string section, string key) 
        {
            string hashKey = $"{section}:{key}";
            if (hashtable.Contains(hashKey))
            {
                return (SettingsRecord)hashtable[hashKey];
            }
            SettingsRecord newSettings = (SettingsRecord)this.GetInitializedDataRecord();
            newSettings.Section = section;
            newSettings.Key = key;
            return newSettings;
        }


    }
}
