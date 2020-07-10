using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using X.Data.Factory;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models
{
    public class VariablesViewModel
    {
        public Exception PasswordException;
        public VariableXList VariableList => Body.Runtime.Variables;
        public Int64 SelectedID { get; set; }

        public VariablesViewModel(HttpRequest httpRequest)
        {
            try
            {
                if (httpRequest != null)
                {
                    string cookValue = httpRequest.Cookies["SelectedVariableID"];
                    if (string.IsNullOrEmpty(cookValue))
                    {
                        if (VariableList.List.Count > 0)
                            SelectedID = VariableList.List[0].ID;
                    }
                    else
                    {
                        SelectedID = Int64.Parse(cookValue);
                        DataRecord dataRecord = VariableList.FindDataRecord(SelectedID);
                        if (dataRecord == null)
                        {
                            if (VariableList.List.Count > 0)
                                SelectedID = VariableList.List[0].ID;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public VariablesViewModel() {}
    }
}
