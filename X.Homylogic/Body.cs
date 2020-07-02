/* HOMYLOGIC BODY
 * 
 * Tu sa nachádzajú všetky aktívne inštancie objektov programu.
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace X.Homylogic
{
    public static class Body
    {
        public const string VERSION_NAME = "July/2020";

        public static Models.Database Database { get; private set; }
        public static Models.Environment Environment { get; private set; }
        public static Models.Runtime Runtime { get; private set; }

        [STAThread]
        static public void Main(String[] args, X.Data.DBClient.ClientTypes databaseType)
        {
            Database = new Models.Database(databaseType);
            Environment = new Models.Environment(Database);
            Runtime = new Models.Runtime();
        }


    }
}
