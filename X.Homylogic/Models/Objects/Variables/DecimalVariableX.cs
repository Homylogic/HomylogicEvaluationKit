/* HOMYLOGIC DECIMAL VARIABLE X
 * 
 * Holds decimal value.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace X.Homylogic.Models.Objects.Variables
{
    public class DecimalVariableX : VariableX
    {
        public new double Value 
        {
            get 
            {
                if (string.IsNullOrEmpty(base.Value)) return 0D;
                double.TryParse(base.Value, NumberStyles.AllowDecimalPoint, X.Basic.XCommon.CSVNumberCulture, out double value);
                return value;
            }
            set 
            {
                base.Value = value.ToString(X.Basic.XCommon.CSVNumberCulture); 
            } 
        }
        public new double DefaultValue
        {
            get
            {
                if (string.IsNullOrEmpty(base.DefaultValue)) return 0D;
                double.TryParse(base.DefaultValue, NumberStyles.AllowDecimalPoint, X.Basic.XCommon.CSVNumberCulture, out double defaultValue);
                return defaultValue;
            }
            set
            {
                base.DefaultValue = value.ToString(X.Basic.XCommon.CSVNumberCulture);
            }
        }
        public DecimalVariableX() { base.VariableType = VariableTypes.Decimal; }

        public override string ToString()
        {
            // Return value in current culture format, because . , chars.
            return this.Value.ToString();
        }
    }
}
