using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models.Devices
{
    public class EditDeviceViewModel : EditObjectViewModel
    {
        public DeviceX.DeviceTypes DeviceType { get; set; }
        public bool ShowOnHome { get; set; }
    }
}
