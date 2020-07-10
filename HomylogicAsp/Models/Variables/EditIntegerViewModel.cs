using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Variables;

namespace HomylogicAsp.Models.Variables
{
    public class EditIntegerViewModel : EditVariableViewModel 
    {
        public long Value { get; set; }
        public long DefaultValue { get; set; }

        public EditIntegerViewModel()
        {
            this.SetViewModel(new IntegerVariableX());
        }
        public EditIntegerViewModel(IntegerVariableX integerVariable)
        {
            this.SetViewModel(integerVariable);
        }
        public IntegerVariableX GetIntegerVariable()
        {
            IntegerVariableX integerVariable;
            if (this.ID > 0)
                integerVariable = (IntegerVariableX)Body.Runtime.Variables.FindDataRecord(this.ID);
            else
                integerVariable = (IntegerVariableX)Body.Runtime.Variables.GetInitializedVariable(VariableX.VariableTypes.Integer);
            integerVariable.Name = this.Name;
            integerVariable.Notice = this.Notice;
            integerVariable.Disabled = this.Disabled;
            integerVariable.Value = this.Value;
            integerVariable.DefaultValue = this.DefaultValue;
            return integerVariable;
        }
        private void SetViewModel(IntegerVariableX integerVariable)
        {
            this.VariableType = VariableX.VariableTypes.Integer;
            this.ID = integerVariable.ID;
            this.Name = integerVariable.Name;
            this.Notice = integerVariable.Notice;
            this.Disabled = integerVariable.Disabled;
            this.Value = integerVariable.Value;
            this.DefaultValue = integerVariable.DefaultValue;
        }
    }
}
