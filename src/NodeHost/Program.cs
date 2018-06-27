/*****************************************************************
 * Node Host
 * Distributed as part of MPAPI - Message Passing API
 * 
 * Author   : Frank Thomsen
 * Web      : http://sector0.dk
 * Contact  : mpapi@sector0.dk
 * License  : New BSD licence
 * 
 * Copyright (c) 2012, Frank Thomsen
 * 
 * Feel free to contact me with bugs and ideas.
 *****************************************************************/

using System;
using System.Configuration;
using System.Reflection;

namespace NodeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get the information in the config file
            string regServerAddress = ConfigurationManager.AppSettings["RegistrationServerAddress"];
            int regServerPort = int.Parse(ConfigurationManager.AppSettings["RegistrationServerPort"]);
            int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            using (var host = new MPAPI.NodeHosting.NodeHost())
            {
                if (args.Length == 0)
                    host.Open(regServerAddress, regServerPort, port);
                else
                {
                    var hostMainFileName = args[0];
                    var typeName = args[1];
                    //Log.Info("Opening '{0}', type '{1}'", hostMainFileName, typeName);
                    var asm = Assembly.LoadFrom(hostMainFileName);
                    var tHostMain = asm.GetType(typeName);
                    host.Open(tHostMain, regServerAddress, regServerPort, port);
                }

                Console.ReadLine();
            }
        }
    }
}