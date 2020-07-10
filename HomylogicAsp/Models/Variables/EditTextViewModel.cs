using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Variables;

namespace HomylogicAsp.Models.Variables
{
    public class EditTextViewModel : EditVariableViewModel 
    {
        public string Value { get; set; }
        public string DefaultValue { get; set; }

        public EditTextViewModel()
        {
            this.SetViewModel(new TextVariableX());
        }
        public EditTextViewModel(TextVariableX textVariable)
        {
            this.SetViewModel(textVariable);
        }
        public TextVariableX GetTextVariable()
        {
            TextVariableX textVariable;
            if (this.ID > 0)
                textVariable = (TextVariableX)Body.Runtime.Variables.FindDataRecord(this.ID);
            else
                textVariable = (TextVariableX)Body.Runtime.Variables.GetInitializedVariable(VariableX.VariableTypes.Text);
            textVariable.Name = this.Name;
            textVariable.Notice = this.Notice;
            textVariable.Disabled = this.Disabled;
            textVariable.Value = this.Value;
            textVariable.DefaultValue = this.DefaultValue;
            return textVariable;
        }
        private void SetViewModel(TextVariableX textVariable)
        {
            this.VariableType = VariableX.VariableTypes.Text;
            this.ID = textVariable.ID;
            this.Name = textVariable.Name;
            this.Notice = textVariable.Notice;
            this.Disabled = textVariable.Disabled;
            this.Value = textVariable.Value;
            this.DefaultValue = textVariable.DefaultValue;
        }
    }
}
