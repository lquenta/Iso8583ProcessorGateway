using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using System.Net;


namespace Cerberus.ATC.GatewayV2Service
{
    static class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static int puerto = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["puerto"]);
        static Tunnel tunnel = new Tunnel(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["puerto"]));
        
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            #if(!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
              ServiceBase.Run(ServicesToRun);
#else
            Service1 myServ = new Service1();
            logger.Info("Servicio Cerberus.ATC.Gateway.V2.Service iniciado");
            TCPServer server = null;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(Tunnel.Get_ip_local_address());
                server = new TCPServer(ipAddress, puerto);

                server.StartServer();
            }
            catch (Exception ex)
            {
                logger.Error(" Error al iniciar servicio,detalle:" + ex.Message + ex.InnerException + ex.StackTrace);
            }
            // here Process is my Service function
            // that will run when my service onstart is call
            // you need to call your own method or function name here instead of Process();
#endif
        }
    }
}
