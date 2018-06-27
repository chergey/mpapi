using System;
using System.Configuration;
using MPAPI.NodeRegistrationServer;

namespace RegistrationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string sPort = ConfigurationManager.AppSettings["Port"];
            if (!int.TryParse(sPort, out int port))
            {
                Console.WriteLine($"Cannot parse '{sPort}' as an integer");
                return;
            }

            var registrationServer = new RegistrationServerBootstrap();
            registrationServer.Open(port);
        }
    }
}