using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Triggers;

namespace HomylogicAsp.Models
{
    public class TriggersViewModel
    {
        public TriggerXList TriggerList => Body.Runtime.Triggers;
        public Int64 SelectedID { get; private set; }

        public TriggersViewModel(HttpRequest httpRequest = null)
        {
            try
            {
                if (httpRequest != null) 
                {
                    string cookValue = httpRequest.Cookies["SelectedTriggerID"];
                    if (string.IsNullOrEmpty(cookValue))
                    {
                        if (Body.Runtime.Triggers.List.Count > 0)
                            SelectedID = Body.Runtime.Triggers.List[0].ID;
                    }
                    else { 
                        SelectedID = Int64.Parse(cookValue);
                        DataRecord dataRecord = Body.Runtime.Triggers.FindDataRecord(SelectedID);
                        if (dataRecord == null)
                        {
                            if (Body.Runtime.Triggers.List.Count > 0)
                                SelectedID = Body.Runtime.Triggers.List[0].ID;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
