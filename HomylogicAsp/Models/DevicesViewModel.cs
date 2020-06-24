using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models
{
    public class DevicesViewModel
    {
        public Exception PasswordException;
        public DeviceXList DeviceList => Body.Runtime.Devices;
        public Int64 SelectedID { get; set; }

        public DevicesViewModel(HttpRequest httpRequest)
        {
            try
            {
                if (httpRequest != null)
                {
                    string cookValue = httpRequest.Cookies["SelectedDeviceID"];
                    if (string.IsNullOrEmpty(cookValue))
                    {
                        if (DeviceList.List.Count > 0)
                            SelectedID = DeviceList.List[0].ID;
                    }
                    else
                    {
                        SelectedID = Int64.Parse(cookValue);
                        DataRecord dataRecord = DeviceList.FindDataRecord(SelectedID);
                        if (dataRecord == null)
                        {
                            if (DeviceList.List.Count > 0)
                                SelectedID = DeviceList.List[0].ID;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public DevicesViewModel() {}
    }
}
