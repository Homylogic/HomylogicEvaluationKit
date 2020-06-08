using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace X.Basic.CodeDom
{
    public static class Ennum
    {

        public class EnnumValues
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
        /// <summary>
        /// Vráti zoznam atribútov descriptions definovaných v enum.
        /// </summary>
        /// <param name="enumType">Enum reflection.</param>
        public static List<EnnumValues> GetEnnumValues(Type enumType)
        {
            List<EnnumValues> list = new List<EnnumValues>();
            foreach (FieldInfo field in enumType.GetFields())
            {
                DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null) list.Add(new EnnumValues { 
                    Description = attribute.Description, 
                    Value = field.GetValue(enumType) });
            }
            return list;
        }
        /// <summary>
        /// Vráti zoznam atribútov descriptions definovaných v enum.
        /// </summary>
        /// <param name="enumType">Enum reflection.</param>
        public static List<string> GetDescriptions(Type enumType)
        {
            List<string> list = new List<string>();
            foreach (FieldInfo field in enumType.GetFields())
            {
                DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null) list.Add(attribute.Description);
            }
            return list;
        }
        /// <summary>
        /// Vráti atribút description podľa zadanej hodnoty enumu.
        /// ! Newvhodné pre používanie v slučkách (vhodné len pre získanie jednej hodnoty description).
        /// </summary>
        /// <param name="enumType">Enum reflection.</param>
        public static string GetDescription(Type enumType, object value)
        {
            string strValue = value.ToString();
            foreach (FieldInfo field in enumType.GetFields())
            {
                if (string.Compare(field.GetValue(value).ToString(), strValue) == 0) 
                { 
                    DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (attribute != null) return attribute.Description;
                }
            }
            return string.Empty;
        }


    }
}
