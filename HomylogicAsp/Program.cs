using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using X.Homylogic;

namespace HomylogicAsp
{
    public class Program
    {

        public static void Main(string[] args)
        {

            // ***** Testing ******
            //MikrotikAPI mikrotik = new MikrotikAPI("192.168.16.62");

            //if (mikrotik.Login("admin", "admin"))
            //{

            //    //mikrotik.Send("/ip/firewall/filter/add");
            //    //mikrotik.Send("=action=drop");
            //    //mikrotik.Send("=chain=forward");
            //    //mikrotik.Send("=dst-port=25");
            //    //mikrotik.Send("=protocol=tcp");
            //    //mikrotik.Send("=protocol=tcp");
            //    //mikrotik.Send(String.Format("=src-address={0}", "192.168.16.10"));

            //    //mikrotik.Send(".tag=firewall", true);
            //    mikrotik.Send("/ip/dns/static/print", true);

            //    foreach (string h in mikrotik.Read())
            //    {
            //        Console.WriteLine(h);
            //    }


            //    // ****** POKRAČOVAŤ ZALOŽENÍM CLASSU   Mikrotic functions alebo to dať rovno do Mikrotik API

            //}


            //return;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"                                                                           
 _     _   _____    __   __  _     _  _       _____    _____  _______   ___   
(_)   (_) (_____)  (__)_(__)(_)   (_)(_)     (_____)  (_____)(_______)_(___)_ 
(_)___(_)(_)   (_)(_) (_) (_)(_)_(_) (_)    (_)   (_)(_)  ___   (_)  (_)   (_)
(_______)(_)   (_)(_) (_) (_)  (_)   (_)    (_)   (_)(_) (___)  (_)  (_)    _ 
(_)   (_)(_)___(_)(_)     (_)  (_)   (_)____(_)___(_)(_)___(_)__(_)__(_)___(_)
(_)   (_) (_____) (_)     (_)  (_)   (______)(_____)  (_____)(_______) (___)  
                                                                              
                                                                              
                                                                              ");

            // * * * BODY INITIALIZATION  * * *
            // X.Homylogic.dll
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Initializing ...");
            try
            {
                // Read database configuration.
                X.Data.DBClient.ClientTypes dbClientType = X.Data.DBClient.ClientTypes.Sqlite; // Default database client type. 
                try
                {
                    string dbProvider = X.App.Settings.ConfigFile.Read("database-provider");
                    switch (dbProvider)
                    {
                        case "1": case "mysql":
                            dbClientType = X.Data.DBClient.ClientTypes.MySql;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Warning: Invalid database provider configuration.");
                    Console.WriteLine(ex.Message);
                }

                // Initialize 'X.Homylogic.Body' class which contains all instanced application objects.
                Body.Main(args, dbClientType);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * DATABASE CHECK OR TRANSFORM TO NEW VERSION * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Checking database ...");
            try
            {
                Body.Database.CreateOrTransform();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * LOADING APPLICATION ENVIRONMENT * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Loading environment ...");
            try
            {
                Body.Environment.Load();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * LOAD RUNTIME OBJECTS FROM DATABASE * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Loading objects ...");
            try
            {
                Body.Runtime.Load();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * LOADING APPLICATION ENVIRONMENT USERS * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Loading users ...");
            try
            {
                Body.Environment.LoadUsers();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * START RUNTIME OBJECTS * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Starting objects ...");
            try
            {
                Body.Runtime.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");

            // * * * START KESTREL ASP WEB SERVER * * *
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Starting web server ...");
            try
            {
                CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_skipInit;
            }
            /* Not needed 
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK"); */

g_skipInit:
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
 █     █░▓█████  ██▓     ▄████▄   ▒█████   ███▄ ▄███▓▓█████ 
▓█░ █ ░█░▓█   ▀ ▓██▒    ▒██▀ ▀█  ▒██▒  ██▒▓██▒▀█▀ ██▒▓█   ▀ 
▒█░ █ ░█ ▒███   ▒██░    ▒▓█    ▄ ▒██░  ██▒▓██    ▓██░▒███   
░█░ █ ░█ ▒▓█  ▄ ▒██░    ▒▓▓▄ ▄██▒▒██   ██░▒██    ▒██ ▒▓█  ▄ 
░░██▒██▓ ░▒████▒░██████▒▒ ▓███▀ ░░ ████▓▒░▒██▒   ░██▒░▒████▒
░ ▓░▒ ▒  ░░ ▒░ ░░ ▒░▓  ░░ ░▒ ▒  ░░ ▒░▒░▒░ ░ ▒░   ░  ░░░ ▒░ ░
  ▒ ░ ░   ░ ░  ░░ ░ ▒  ░  ░  ▒     ░ ▒ ▒░ ░  ░      ░ ░ ░  ░
  ░   ░     ░     ░ ░   ░        ░ ░ ░ ▒  ░      ░      ░   
    ░       ░  ░    ░  ░░ ░          ░ ░         ░      ░  ░
                        ░                                   ");

            // Spracovávanie zadaných príkazov.
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("write 'help' for list all available commands:");
g_readLine:
            try
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    string cmd = line;
                    string cmdLine = null;
                    int indexSpace = line.IndexOf(' ');
                    if (indexSpace > 0)
                    {
                        cmd = line.Substring(0, indexSpace).Trim();
                        cmdLine = line.Substring(indexSpace).Trim();
                    }
                    switch (cmd)
                    {
                        case "help": case "about": WriteAboutHomylogic(); break;
                        case "exit": case "quit": goto g_exit;
                        case "show": Commands.Show.DoLine(cmdLine); break;
                        case "db": Commands.DB.DoLine(cmdLine); break;
                        default: Console.WriteLine("Command not found."); break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
                goto g_readLine;
            }
g_exit:
            // Zastaviť vlákna objektov.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
▄▄▄▄·  ▄· ▄▌▄▄▄ .
▐█ ▀█▪▐█▪██▌▀▄.▀·
▐█▀▀█▄▐█▌▐█▪▐▀▀▪▄
██▄▪▐█ ▐█▀·.▐█▄▄▌
·▀▀▀▀   ▀ •  ▀▀▀ ");
            try
            {
                Body.Runtime.Stop();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
            }

            // Zatvorioť pripojenie k databáze.
            Body.Database.DBClient.Close();
            Body.Database.DBClientLogs.Close();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        /// <summary>
        /// Vypíše všetky dostupné príkazy.
        /// </summary>
        static void WriteAboutHomylogic()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
      ___                         ___           ___                   
     /\  \         _____         /\  \         /\  \                  
    /::\  \       /::\  \       /::\  \        \:\  \         ___     
   /:/\:\  \     /:/\:\  \     /:/\:\  \        \:\  \       /\__\    
  /:/ /::\  \   /:/ /::\__\   /:/  \:\  \   ___  \:\  \     /:/  /    
 /:/_/:/\:\__\ /:/_/:/\:|__| /:/__/ \:\__\ /\  \  \:\__\   /:/__/     
 \:\/:/  \/__/ \:\/:/ /:/  / \:\  \ /:/  / \:\  \ /:/  /  /::\  \     
  \::/__/       \::/_/:/  /   \:\  /:/  /   \:\  /:/  /  /:/\:\  \    
   \:\  \        \:\/:/  /     \:\/:/  /     \:\/:/  /   \/__\:\  \   
    \:\__\        \::/  /       \::/  /       \::/  /         \:\__\  
     \/__/         \/__/         \/__/         \/__/           \/__/  ");

            Console.WriteLine();
            Console.WriteLine($"App version: {X.Homylogic.Body.VERSION_NAME}");
            Console.WriteLine($"DB provider: {Body.Database.DBClient.ClientType}");
            Console.WriteLine($"DB version:  {X.Homylogic.Models.Database.VERSION.ToString().Insert(1, ".")}");
            Console.WriteLine();
            Console.WriteLine("List of all available commands:");
            Console.WriteLine();
            Commands.Show.WriteAllAvailableCommands();
            Console.WriteLine();
            Commands.DB.WriteAllAvailableCommands();
            Console.WriteLine();
            Console.WriteLine("exit, quit - Quits running Homylogic application.");
        }


    }
}
