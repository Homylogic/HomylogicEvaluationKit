/* HOMYLOGIC INTEGER VARIABLE X
 * 
 * Holds Yes or No (boolean) value.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace X.Homylogic.Models.Objects.Variables
{
    public class YesNoVariableX : VariableX
    {
        public new bool Value 
        {
            get 
            {
                if (string.IsNullOrEmpty(base.Value)) return false;
                if (base.Value == "Y") return true;
                return false;
            }
            set 
            {
                base.Value = value ? "Y" : "N";
            } 
        }
        public new bool DefaultValue
        {
            get
            {
                if (string.IsNullOrEmpty(base.DefaultValue)) return false;
                if (base.DefaultValue == "Y") return true;
                return false;
            }
            set
            {
                base.DefaultValue = value ? "Y" : "N";
            }
        }
        public YesNoVariableX() { base.VariableType = VariableTypes.YesNo; }

        public override string ToString()
        {
            return this.Value ? "Yes" : "No";
        }
    }

}