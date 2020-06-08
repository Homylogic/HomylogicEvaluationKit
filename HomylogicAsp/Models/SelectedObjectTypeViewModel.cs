using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomylogicAsp.Models
{
    public class SelectedObjectTypeViewModel
    {
        public string ObjectTypeName { get; private set; }
        public List<X.Basic.CodeDom.Ennum.EnnumValues> SelectableValues { get; private set; }

        public SelectedObjectTypeViewModel(string objectTypeName) 
        {
            this.ObjectTypeName = objectTypeName;

            switch (this.ObjectTypeName) 
            {
                case "Devices":
                    SelectableValues = X.Basic.CodeDom.Ennum.GetEnnumValues(typeof(X.Homylogic.Models.Objects.DeviceX.DeviceTypes));
                    break;

                case "Triggers":
                    SelectableValues = X.Basic.CodeDom.Ennum.GetEnnumValues(typeof(X.Homylogic.Models.Objects.TriggerX.TriggerTypes));
                    break;
            }
        }
    }
}
