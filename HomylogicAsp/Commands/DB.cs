/* DB COMMAND
 * 
 * Configure connection to database Sqlite to MySQL migration.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X.App.Logs;
using X.Data;
using X.Data.Factory;
using X.Homylogic;

namespace HomylogicAsp.Commands
{
    public static class DB
    {
        const string INVALID_ARGS = "Invalid DB commands parameters.";

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
                case "set-provider": SetProvider(args); break;
                case "set-mysql": SetMySqlConnection(args); break;
                default: Console.WriteLine(INVALID_ARGS); break;
            }            
        }
        public static void SetProvider(string[] args)
        {
            try
            {
                switch (args[1].Trim())
                {
                    case "0": case "sqlite":
                        if (Body.Database.DBClient.ClientType == X.Data.DBClient.ClientTypes.Sqlite)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Database provider is already connected to Sqlite.");
                        }
                        else
                        {
                            X.App.Settings.ConfigFile.Write("database-provider", "sqlite");
                            Console.WriteLine("Database provider changed to Sqlite, restart application to use new connection.");
                        }
                        break;

                    case "1": case "mysql":
                        if (Body.Database.DBClient.ClientType == X.Data.DBClient.ClientTypes.MySql) {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Database provider is already connected to MySQL.");
                        }
                        else { 
                            X.App.Settings.ConfigFile.Write("database-provider", "mysql");
                            Console.WriteLine("Database provider changed to MySQL, you can set connection values or restart application to use new connection.");
                        }
                        break;

                    default: Console.WriteLine(INVALID_ARGS); break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }
        public static void SetMySqlConnection(string[] args)
        {
            try
            {
                StringBuilder settingsValue = new StringBuilder();
                foreach (string arg in args) 
                {
                    if (arg.StartsWith("host")) settingsValue.Append($"host={arg.Substring(arg.IndexOf("=") + 1).Trim()};");
                    if (arg.StartsWith("user")) settingsValue.Append($"user={arg.Substring(arg.IndexOf("=") + 1).Trim()};");
                    if (arg.StartsWith("pass")) settingsValue.Append($"pass={arg.Substring(arg.IndexOf("=") + 1).Trim()};");
                }
                string encrypted = X.Basic.Text.Crypto.Encrypt(settingsValue.ToString(), X.Homylogic.Models.Database.CRYPTO_KEY);
                X.App.Settings.ConfigFile.Write("database-mysql", encrypted);
                Console.WriteLine("MySQL connection has been changed, please restart application.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }

        public static void WriteAllAvailableCommands()
        {
            Console.WriteLine("db set-provider [provider-type:{0-sqlite,1-mysql}] - Sets database provider.");
            Console.WriteLine("db set-mysql [host] [user] [pass] - Sets MySql connection values.");
        }
    }
}
