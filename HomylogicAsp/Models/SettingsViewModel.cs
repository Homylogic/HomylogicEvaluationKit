using HomylogicAsp.Models.Users;
using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models;

namespace HomylogicAsp.Models
{
    public class SettingsViewModel
    {
        public Exception SaveException;
        public Settings Settings => Body.Environment.Settings;
        public Settings.HomeSettings.BackgroundImageTypes Home_BackgroundImage { get; set; } = Body.Environment.Settings.Home.BackgroundImage;
        public Int64 Security_DefaultUserID { get; set; } = Body.Environment.Settings.Security.DefaultUserID;
    }
}
