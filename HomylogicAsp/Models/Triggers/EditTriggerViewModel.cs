using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Triggers;

namespace HomylogicAsp.Models.Triggers
{
    public class EditTriggerViewModel : EditObjectViewModel
    {
        public TriggerX.TriggerTypes TriggerType { get; set; }
        public List<X.Basic.CodeDom.Ennum.EnnumValues> SelectableDevices => Body.Runtime.Devices.GetEnnumValues();
    }
}
