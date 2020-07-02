using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace X.Basic
{
    public static class XCommon
    {
        static CultureInfo _csvNumberCulture;
        /// <summary>
        /// Return culture info for formatting numbers without spaces (decimal separator is dot, negative is -).
        /// </summary>
        public static CultureInfo CSVNumberCulture
        {
            get 
            {
                if (_csvNumberCulture == null) {
                    _csvNumberCulture = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                    _csvNumberCulture.NumberFormat.NumberDecimalSeparator = ".";
                    _csvNumberCulture.NumberFormat.NegativeSign = "-";
                    _csvNumberCulture.NumberFormat.NumberGroupSeparator = "";
                }
                return _csvNumberCulture;
            }
        }
    }
}
