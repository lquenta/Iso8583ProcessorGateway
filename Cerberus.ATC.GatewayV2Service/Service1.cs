using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using NLog;

namespace Cerberus.ATC.GatewayV2Service
{
    public partial class Service1 : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static int puerto = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["puerto"]);
        static Tunnel tunnel = new Tunnel(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["puerto"]));
        private TCPServer server = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Servicio Cerberus.ATC.Gateway.V2.Service iniciado");
            try
            {
                IPAddress ipAddress = IPAddress.Parse(Tunnel.Get_ip_local_address());
                server = new TCPServer(ipAddress, puerto);

                server.StartServer();
            }
            catch (Exception ex)
            {
                logger.Error(" Error al iniciar servicio,detalle:"+ex.Message + ex.InnerException + ex.StackTrace);
                EventLog.WriteEntry(ex.Message + ex.InnerException + ex.StackTrace);
            }
        }

        protected override void OnStop()
        {
            logger.Info("Servicio Cerberus.ATC.Gateway.V2.Service detenido");
            server.StopServer();
            server = null;
        }
    }
}
