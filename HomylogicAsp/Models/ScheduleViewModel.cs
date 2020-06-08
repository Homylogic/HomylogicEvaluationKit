using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models
{
    public class ScheduleViewModel
    {
        public Int64 OwnerID { get; set; }
        public Int64 SelectedID { get; set; }

        public ScheduleViewModel(HttpRequest httpRequest = null)
        {
            try
            {
                if (httpRequest != null)
                {
                    string cookValue = httpRequest.Cookies["SelectedScheduleID"];
                    if (!string.IsNullOrEmpty(cookValue))
                        SelectedID = Int64.Parse(cookValue);
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
