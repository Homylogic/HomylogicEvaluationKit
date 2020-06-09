using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace HomylogicInstallSSLWin
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting to Homylogic Evaluation Kit");
            Console.WriteLine("http://homylogic.go");
            Console.WriteLine("Downloading certificate ...");

            /* V .net core fungovalu, tu nefunguje ...
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // Umožní ignorovať nekorektný SSL certifikát.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            */

            try
            {
                using (WebClient myWebClient = new WebClient())
                {
                    myWebClient.DownloadFile("http://homylogic.go/Home/Download?file=ssl/homylogic.crt", "homylogic.crt");
                }
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't connect to Homylogic Evaluation Kit.");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Installing homylogic SSL certificate ...");

            try
            {
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile("homylogic.crt")));
                store.Close();
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't install homylogic SSL certificate.");
                Console.WriteLine(ex.Message);
            }

            if (File.Exists("homylogic.crt"))
                File.Delete("homylogic.crt");

            Console.WriteLine("Homylogic SSL certificate has been succesfully installed.");
            Console.WriteLine("Please restart your browser ...");
            Console.ReadLine();
        }
    }
}
