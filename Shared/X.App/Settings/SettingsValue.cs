/* SETTINGS VALUE
 * 
 * Používa sa pre nastavenie a získanie hodnoty nastavenia v objekte SettingsRecord.
 * 
 */
using System;
using X.Basic;

namespace X.App.Settings
{
    public sealed class SettingsValue
    {
        string _value;
        /// <summary>
        /// Vráti textovú hodnotu.
        /// </summary>
        public string Text => _value;
        /// <summary>
        /// Vráti celočíslenú hodnotu.
        /// </summary>
        public Int32 Int32 
        {
            get 
            {
                try
                {
                    if (string.IsNullOrEmpty(_value)) return 0;
                    return Int32.Parse(_value, XCommon.CSVNumberCulture);
                }
                catch (Exception)
                {
                    return 0;
                }
            }    
        }
        /// <summary>
        /// Vráti celočíslenú hodnotu.
        /// </summary>
        public Int64 Int64
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(_value)) return 0;
                    return Int64.Parse(_value, XCommon.CSVNumberCulture);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Nastavenie textovej hodnoty.
        /// </summary>
        public void SetValue(string text) { _value = text; }
        /// <summary>
        /// Nastavenie celočíslenú hodnoty.
        /// </summary>
        public void SetValue(Int32 number) 
        {
            _value = number.ToString(XCommon.CSVNumberCulture); 
        }
        /// <summary>
        /// Nastavenie celočíslenú hodnoty.
        /// </summary>
        public void SetValue(Int64 number)
        {
            _value = number.ToString(XCommon.CSVNumberCulture);
        }
    }
}
