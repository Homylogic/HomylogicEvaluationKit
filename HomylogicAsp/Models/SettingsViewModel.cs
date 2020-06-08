using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models;

namespace HomylogicAsp.Models
{
    public class SettingsViewModel 
    {
        public string PasswordAccess { get; set; }
        public bool HasAccess { get; set; }
        public Exception SaveException;
        public Settings Settings => Body.Environment.Settings;
        public Settings.HomeSettings.BackgroundImageTypes Home_BackgroundImage { get; set; } = Body.Environment.Settings.Home.BackgroundImage;
        public string Security_Password { get; set; } = Body.Environment.Settings.Security.Password;
    }
}
