using System;
using System.Collections.Generic;
using System.Text;

namespace X.Basic
{
    /// <summary>
    /// Umožňuje používanie vlastnej Exception, ktorá obsahuje aj kód chyby.
    /// </summary>
    public sealed class XException
    {
        /// <summary>
        /// Či je chyba alebo nie je chyba.
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Text chyby.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Číslo chyby.
        /// </summary>
        public long Number { get; set; }


        /// <summary>
        /// Vytvorí objekt s resultom TRUE (bez chyby).
        /// </summary>
        public XException()
        {
            this.Result = true;
        }

        /// <summary>
        /// Vytvorí chybu s resultom FALSE.
        /// </summary>
        public XException(string message, long number = 1) 
        {
            this.Result = false;
            this.Message = message;
            this.Number = number;
        }

    }
}
