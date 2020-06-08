/* SQL CONVERT FUNCTIONS
 * 
 * Obsahuje funkcie pre vytváranie SQL dotazov, napr. formátovanie údajov.
 * Použitie: using Q = X.Data.Management.SqlConvert; 
 * 
 */
using System;
using System.Globalization;

namespace X.Data.Management
{
    public sealed class SqlConvert
    {
        readonly DBClient.ClientTypes _clientType;

        /// <summary>
        /// Konverzia C# Boolean pre databázovú hodnotu celé číslo.
        /// </summary>
        public Int32 Innt32(bool value) { return value ? 1 : 0; ; }
        /// <summary>
        /// Konverzia databázovej celj číselnej hodnoty na C# Boolean.
        /// </summary>
        public bool Bol(Int32 value) { return value > 0 ? true : false; }
        /// <summary>
        /// Konverzia databázovej celj číselnej hodnoty na DB DateTime.
        /// </summary>
        public string DTime(DateTime value) { return string.Format("'{0}'", value.ToString("yyyy-MM-dd HH:mm:ss")); }
        /// <summary>
        /// Konverzia databázovej celej číselnej hodnoty na DB string.
        /// </summary>
        public string Str(string value) { return (string.IsNullOrEmpty(value)) ? "''" : string.Format("'{0}'", value.Replace(@"'", @"''")); }
        /// <summary>
        /// Konverzia databázovej desatinnej číselnej hodnoty na DB floating.
        /// </summary>
        public string Float(float value) 
        {
            CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NegativeSign = "-";
            ci.NumberFormat.NumberGroupSeparator = "";
            return value.ToString(ci); 
        }


        public SqlConvert(DBClient.ClientTypes clientType) { _clientType = clientType; }

    }
}
