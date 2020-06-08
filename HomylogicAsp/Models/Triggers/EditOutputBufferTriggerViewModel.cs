using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;
using X.Homylogic.Models.Objects.Triggers;

namespace HomylogicAsp.Models.Triggers
{
    public class EditOutputBufferTriggerViewModel : EditTriggerViewModel
    {
        public Int64 DeviceID { get; set; }
        public OutputBufferTriggerX.ActionTypes ActionType { get; set; }
        public string Data { get; set; }

        public EditOutputBufferTriggerViewModel()
        {
            this.SetViewModel(new OutputBufferTriggerX());
        }
        public EditOutputBufferTriggerViewModel(OutputBufferTriggerX outputBufferTrigger)
        {
            this.SetViewModel(outputBufferTrigger);
        }
        public OutputBufferTriggerX GetOutputBufferTrigger()
        {
            OutputBufferTriggerX outputBufferTrigger;
            if (this.ID > 0)
                outputBufferTrigger = (OutputBufferTriggerX)Body.Runtime.Triggers.FindDataRecord(this.ID);
            else
                outputBufferTrigger = (OutputBufferTriggerX)Body.Runtime.Triggers.GetInitializedTrigger(TriggerX.TriggerTypes.OutputBuffer);
            outputBufferTrigger.Name = this.Name;
            outputBufferTrigger.Disabled = this.Disabled;
            outputBufferTrigger.DeviceID = this.DeviceID;
            outputBufferTrigger.ActionType = this.ActionType;
            outputBufferTrigger.Data = this.Data;
            return outputBufferTrigger;
        }
        private void SetViewModel(OutputBufferTriggerX outputBufferTrigger) 
        {
            this.TriggerType = TriggerX.TriggerTypes.OutputBuffer;
            this.ID = outputBufferTrigger.ID;
            this.Name = outputBufferTrigger.Name;
            this.Disabled = outputBufferTrigger.Disabled;
            this.DeviceID = outputBufferTrigger.DeviceID;
            this.ActionType = outputBufferTrigger.ActionType;
            this.Data = outputBufferTrigger.Data;
        }

    }
}
