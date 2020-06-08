using System;
using X.Homylogic;
using X.Homylogic.Models.Objects;

namespace HomylogicAsp.Models
{
    public class HomeViewModel
    {

        public DeviceXList DeviceList => Body.Runtime.Devices;


    }
}
