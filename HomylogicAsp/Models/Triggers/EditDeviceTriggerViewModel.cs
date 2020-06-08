using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;
using X.Homylogic.Models.Objects.Triggers;

namespace HomylogicAsp.Models.Triggers
{
    public class EditDeviceTriggerViewModel : EditTriggerViewModel
    {
        [Required]
        public Int64 DeviceID { get; set; }
        public DeviceTriggerX.ActionTypes ActionType { get; set; }

        public EditDeviceTriggerViewModel()
        {
            this.SetViewModel(new DeviceTriggerX());
        }
        public EditDeviceTriggerViewModel(DeviceTriggerX deviceTrigger)
        {
            this.SetViewModel(deviceTrigger);
        }
        public DeviceTriggerX GetDeviceTrigger()
        {
            DeviceTriggerX deviceTrigger;
            if (this.ID > 0)
                deviceTrigger = (DeviceTriggerX)Body.Runtime.Triggers.FindDataRecord(this.ID);
            else
                deviceTrigger = (DeviceTriggerX)Body.Runtime.Triggers.GetInitializedTrigger(TriggerX.TriggerTypes.Device);
            deviceTrigger.Name = this.Name;
            deviceTrigger.Disabled = this.Disabled;
            deviceTrigger.DeviceID = this.DeviceID;
            deviceTrigger.ActionType = this.ActionType;
            return deviceTrigger;
        }
        private void SetViewModel(DeviceTriggerX deviceTrigger) 
        {
            this.TriggerType = TriggerX.TriggerTypes.Device;
            this.ID = deviceTrigger.ID;
            this.Name = deviceTrigger.Name;
            this.Disabled = deviceTrigger.Disabled;
            this.DeviceID = deviceTrigger.DeviceID;
            this.ActionType = deviceTrigger.ActionType;
        }

    }
}
