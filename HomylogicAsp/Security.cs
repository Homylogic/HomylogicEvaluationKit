using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.Homylogic;

namespace HomylogicAsp
{
    public class Security
    {
        static List<String> AccessKeys;
        /// <summary>
        /// Overí či tento prehliadač má prístup (teda či bolo zadané heslo), funguje pomocou cookies.
        /// </summary>
        public static bool HasUserAccess(HttpRequest httpRequest) 
        {
            if (string.IsNullOrEmpty(Body.Environment.Settings.Security.Password)) return true;    
            if (AccessKeys == null) return false;
            string cookValue = httpRequest.Cookies["AccessKey"];
            if (string.IsNullOrEmpty(cookValue)) return false;
            return AccessKeys.Contains(cookValue);
        }
        /// <summary>
        /// Umožní tomuto prehliadaču získať prístup, napr. ku nastaveniam ak sú chránené heslom.
        /// </summary>
        public static void AllowUserAccess(HttpResponse httpResponse) 
        {
            string accessKey = Guid.NewGuid().ToString();
            httpResponse.Cookies.Append("AccessKey", accessKey);
            if (AccessKeys == null) AccessKeys = new List<String>();
            if (!AccessKeys.Contains(accessKey))
                AccessKeys.Add(accessKey);
        }
        public static void ResetAllAccess() 
        {
            if (AccessKeys != null)
                AccessKeys.Clear();
        }
    }
}
