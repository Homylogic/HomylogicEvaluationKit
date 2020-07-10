using System.Globalization;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Variables;

namespace HomylogicAsp.Models.Variables
{
    public class EditDecimalViewModel : EditVariableViewModel 
    {
        /*! Double is not working automatically in WEB UI, because of culture and , . chars.
        public double Value { get; set; }
        public double DefaultValue { get; set; } */
        public string Value { get; set; }
        public string DefaultValue { get; set; }

        public EditDecimalViewModel()
        {
            this.SetViewModel(new DecimalVariableX());
        }
        public EditDecimalViewModel(DecimalVariableX decimalVariable)
        {
            this.SetViewModel(decimalVariable);
        }
        public DecimalVariableX GetDecimalVariable()
        {
            DecimalVariableX decimalVariable;
            if (this.ID > 0)
                decimalVariable = (DecimalVariableX)Body.Runtime.Variables.FindDataRecord(this.ID);
            else
                decimalVariable = (DecimalVariableX)Body.Runtime.Variables.GetInitializedVariable(VariableX.VariableTypes.Decimal);
            decimalVariable.Name = this.Name;
            decimalVariable.Notice = this.Notice;
            decimalVariable.Disabled = this.Disabled;
            double.TryParse(this.Value, NumberStyles.AllowDecimalPoint, X.Basic.XCommon.CSVNumberCulture, out double value);
            decimalVariable.Value = value;
            double.TryParse(this.DefaultValue, NumberStyles.AllowDecimalPoint, X.Basic.XCommon.CSVNumberCulture, out double defaultValue);
            decimalVariable.DefaultValue = defaultValue;
            return decimalVariable;
        }
        private void SetViewModel(DecimalVariableX decimalVariable)
        {
            this.VariableType = VariableX.VariableTypes.Decimal;
            this.ID = decimalVariable.ID;
            this.Name = decimalVariable.Name;
            this.Notice = decimalVariable.Notice;
            this.Disabled = decimalVariable.Disabled;
            this.Value = decimalVariable.Value.ToString(X.Basic.XCommon.CSVNumberCulture);
            this.DefaultValue = decimalVariable.DefaultValue.ToString(X.Basic.XCommon.CSVNumberCulture);
        }



    }
}
