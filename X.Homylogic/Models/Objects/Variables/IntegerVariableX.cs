/* HOMYLOGIC INTEGER VARIABLE X
 * 
 * Holds long integer value.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace X.Homylogic.Models.Objects.Variables
{
    public class IntegerVariableX : VariableX
    {
        public new long Value 
        {
            get 
            {
                if (string.IsNullOrEmpty(base.Value)) return 0L;
                long.TryParse(base.Value, out long value);
                return value;
            }
            set 
            {
                base.Value = value.ToString(); 
            } 
        }
        public new long DefaultValue
        {
            get
            {
                if (string.IsNullOrEmpty(base.DefaultValue)) return 0L;
                long.TryParse(base.DefaultValue, out long defaultValue);
                return defaultValue;
            }
            set
            {
                base.DefaultValue = value.ToString();
            }
        }
        public IntegerVariableX() { base.VariableType = VariableTypes.Integer; }

        public override string ToString()
        {
            // Return value with group separator (format is for Int64.Max value).
            return this.Value.ToString("# ### ### ### ### ### ###"); 
        }
    }
}
