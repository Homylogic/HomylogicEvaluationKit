/* SHOW COMMAND
 * 
 * Vypisuje objekty typu DataList do konzoly.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X.App.Logs;
using X.Data.Factory;
using X.Homylogic;

namespace HomylogicAsp.Commands
{
    public static class Show
    {
        const string TITLE = "Commands";
        const string INVALID_ARGS = "Invalid show commands parameters. Requesting list name. etc. show logs";

        public static void DoLine(string line)
        {
            if (string.IsNullOrEmpty(line)) 
            {
                Console.WriteLine(INVALID_ARGS);
                return;
            }
            string[] args = line.Split(' ');
            if (args == null || args.Length < 1) 
            {
                Console.WriteLine(INVALID_ARGS);
                return;
            }

            switch (args[0].Trim()) 
            {
                case "logs": Logs(); break;

            }            
        }
        public static void Logs()
        {
            try
            {
                Console.WriteLine($"Log list: {Body.Environment.Logs.List.Count}");
                Console.WriteLine($"Columns: ID, Time, ");
                for (int i = 0; i < Body.Environment.Logs.List.Count; i++)
                {
                    LogRecord log = (LogRecord)Body.Environment.Logs.List[i];
                    StringBuilder o = new StringBuilder();
                    o.Append(log.ID);
                    o.Append(log.LogTime.ToString("dd.MM.yy HH:ss"));
                    Console.WriteLine(o.ToString());                
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem reading logs.", ex, TITLE);
            }
        }
    }
}
