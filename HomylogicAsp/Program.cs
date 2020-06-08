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

            Console.WriteLine("Welcome to Homylogic Evaluation Kit Pi");


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


            //    // ****** POKRA�OVA� ZALO�EN�M CLASSU   Mikrotic functions alebo to da� rovno do Mikrotik API

            //}


            //return;



            // Inicializ�cia kni�nice X.Homylogic.Body.
            Console.WriteLine("Initializing ...");
            try
            {
                Body.Main(args, X.Data.DBClient.ClientTypes.Sqlite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Vytvorenie novej datab�zy, alebo transform�cia do novej verzie.
            Console.WriteLine("Checking database ...");
            try
            {
                Body.Database.CreateOrTransform();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Na��tanie prostredia napr. logovanie �dajov a nastavenia programu.
            Console.WriteLine("Loading environment ...");
            try
            {
                Body.Environment.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Na��tanie zoznamu v�etk�ch objektov pod�a datab�zy.
            Console.WriteLine("Loading objects ...");
            try
            {
                Body.Runtime.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Spustenie vl�kien v�etk�ch objektov.
            Console.WriteLine("Starting objects ...");
            try
            {
                Body.Runtime.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); ;
            }

            // Spustenie ASP str�nky.
            Console.WriteLine("Starting web server ...");
            try
            {
                CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); ;
            }

            // Spracov�vanie zadan�ch pr�kazov.
            Console.WriteLine("write 'help' for list all available commands:");
g_readLine:
            try
            {
                while (true)
                {
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
                        case "help": WriteAllAvailableCommands(); break;
                        case "exit": case "quit": goto g_exit;
                        case "show": Commands.Show.DoLine(cmdLine); break;
                        default: Console.WriteLine("Command not found."); break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                goto g_readLine;
            }
g_exit:
            // Zastavi� vl�kna objektov.
            Console.WriteLine("bye ...");
            try
            {
                Body.Runtime.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        /// <summary>
        /// Vyp�e v�etky dostupn� pr�kazy.
        /// </summary>
        static void WriteAllAvailableCommands()
        {
            Console.WriteLine("exit, quit - Quits running Homylogic application.");
        }


    }
}
