using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestFlujoWebServiceSOAT.wsParametricasUNIVIDA;
using TestFlujoWebServiceSOAT.wsSeguridadSOAT;
using TestFlujoWebServiceSOAT.wsVentasUNIVIDA;

namespace TestFlujoWebServiceSOAT
{
    class Program
    {
        static void Main(string[] args)
        {
            int token = 0;
            //llamada al metodo de seguridad
            IwsSeguridadClient client = new IwsSeguridadClient();
            CESeg01Autenticacion oCeSeg01Autenticacion = new CESeg01Autenticacion();
            oCeSeg01Autenticacion.Contrasenia = "123456";
            oCeSeg01Autenticacion.Ip = "127.0.0.1";
            oCeSeg01Autenticacion.SoatTIntermediarioFk = 0;
            oCeSeg01Autenticacion.Usuario = "EDWIN";

            CSSeg01Autenticacion res= client.Seg01Autenticacion(oCeSeg01Autenticacion);
            if (res.Exito)
            {
                Console.Write("SUCCESS");
                token = res.SeguridadToken;
            }
            else
            {
                Console.WriteLine("ERR");
                return;
            }
            //parametricas

            IwsParametricasClient clientParametricasClient = new IwsParametricasClient();
            CEParObtenerDepartamentos reqParObtenerDepartamentos = new CEParObtenerDepartamentos();
            reqParObtenerDepartamentos.SeguridadToken = token;
            reqParObtenerDepartamentos.SoatTIntermediarioFk = 0;
            reqParObtenerDepartamentos.Usuario = "EDWIN";
            CSParObtenerDepartamentos resParObtenerDepartamentos= clientParametricasClient.ParObtenerDepartamentos(reqParObtenerDepartamentos);
            if (resParObtenerDepartamentos.Exito)
            {
                foreach (var depto in resParObtenerDepartamentos.lDepartamento)
                {
                    Console.WriteLine((depto.CodigoDepartamento+"_"+depto.Descripcion));
                    
                }
            }
            else
            {
                return;
            }


            //intento de validacion 
            IwsVentasClient clientVentasClient=new IwsVentasClient();
            CEVen03EfectivizarFactCicl reqCeVen03EfectivizarFactCicl=new CEVen03EfectivizarFactCicl();
            //reqCeVen03EfectivizarFactCicl.
            //clientVentasClient.Ven03EfectivizarFactCicl()

            Console.ReadLine();
        }
    }
}
