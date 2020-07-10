/* HOMYLOGIC TEXT VARIABLE X
 * 
 * Holds text value.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace X.Homylogic.Models.Objects.Variables
{
    public class TextVariableX : VariableX
    {
        public new string Value 
        {
            get { return base.Value; }
            set { base.Value = value; } 
        }
        public new string DefaultValue
        {
            get { return base.DefaultValue; }
            set { base.DefaultValue = value; }
        }
        public TextVariableX() { base.VariableType = VariableTypes.Text; }
    }
}
