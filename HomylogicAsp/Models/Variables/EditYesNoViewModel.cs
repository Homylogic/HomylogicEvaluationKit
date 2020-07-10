using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Variables;

namespace HomylogicAsp.Models.Variables
{
    public class EditYesNoViewModel : EditVariableViewModel 
    {
        public bool Value { get; set; }
        public bool DefaultValue { get; set; }

        public EditYesNoViewModel()
        {
            this.SetViewModel(new YesNoVariableX());
        }
        public EditYesNoViewModel(YesNoVariableX yesNoVariable)
        {
            this.SetViewModel(yesNoVariable);
        }
        public YesNoVariableX GetDecimalVariable()
        {
            YesNoVariableX yesNoVariable;
            if (this.ID > 0)
                yesNoVariable = (YesNoVariableX)Body.Runtime.Variables.FindDataRecord(this.ID);
            else
                yesNoVariable = (YesNoVariableX)Body.Runtime.Variables.GetInitializedVariable(VariableX.VariableTypes.YesNo);
            yesNoVariable.Name = this.Name;
            yesNoVariable.Notice = this.Notice;
            yesNoVariable.Disabled = this.Disabled;
            yesNoVariable.Value = this.Value;
            yesNoVariable.DefaultValue = this.DefaultValue;
            return yesNoVariable;
        }
        private void SetViewModel(YesNoVariableX yesNoVariable)
        {
            this.VariableType = VariableX.VariableTypes.YesNo;
            this.ID = yesNoVariable.ID;
            this.Name = yesNoVariable.Name;
            this.Notice = yesNoVariable.Notice;
            this.Disabled = yesNoVariable.Disabled;
            this.Value = yesNoVariable.Value;
            this.DefaultValue = yesNoVariable.DefaultValue;
        }

    }
}