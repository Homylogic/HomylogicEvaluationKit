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
                default: Console.WriteLine(INVALID_ARGS); break;
            }            
        }
        public static void Logs()
        {
            try
            {
                Console.WriteLine($"Log list: {Body.Environment.Logs.List.Count}");
                Console.WriteLine($"Columns: ID, Time, Text, Description, Source");
                for (int i = 0; i < Body.Environment.Logs.List.Count; i++)
                {
                    LogRecord log = (LogRecord)Body.Environment.Logs.List[i];
                    StringBuilder o = new StringBuilder();
                    o.Append(log.ID);
                    o.Append(log.LogTime.ToString("dd.MM.yy HH:ss"));
                    o.Append(log.Text);
                    o.Append(log.Description);
                    o.Append(log.Source);
                    Console.WriteLine(o.ToString());                
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }
        public static void WriteAllAvailableCommands()
        {
            Console.WriteLine("show [list-name] - Shows data from list.");
            Console.WriteLine("show logs - Shows logs.");
        }

    }
}
