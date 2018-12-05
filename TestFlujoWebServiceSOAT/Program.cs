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
            string usuario = "ESEGOBIANO";
            string placa = "1930CUL";
            int canalVenta = 28;
            int gestionFk = 2019;
            int medioPago = 29;
            oCeSeg01Autenticacion.Contrasenia = "123456";
            oCeSeg01Autenticacion.Ip = "127.0.0.1";
            oCeSeg01Autenticacion.SoatTIntermediarioFk = 0;
            oCeSeg01Autenticacion.Usuario = usuario;

            CSSeg01Autenticacion res = client.Seg01Autenticacion(oCeSeg01Autenticacion);
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
            reqParObtenerDepartamentos.Usuario = usuario;
            CSParObtenerDepartamentos resParObtenerDepartamentos = clientParametricasClient.ParObtenerDepartamentos(reqParObtenerDepartamentos);
            if (resParObtenerDepartamentos.Exito)
            {
                foreach (var depto in resParObtenerDepartamentos.lDepartamento)
                {
                    Console.WriteLine((depto.CodigoDepartamento + "_" + depto.Descripcion));

                }
            }
            else
            {
                return;
            }



            //intento de validacion 
            IwsVentasClient clientVentasClient = new IwsVentasClient();

            //validar placa vendible
            CEVen01ValidarVendibleYObtenerDatos reqCEVen01ValidarVendibleYObtenerDatos = new CEVen01ValidarVendibleYObtenerDatos();
            reqCEVen01ValidarVendibleYObtenerDatos.SeguridadToken = token;
            reqCEVen01ValidarVendibleYObtenerDatos.SoatTIntermediarioFk = 0;
            reqCEVen01ValidarVendibleYObtenerDatos.SoatTParGestionFk = gestionFk;
            reqCEVen01ValidarVendibleYObtenerDatos.SoatTParVentaCanalFk = canalVenta;
            reqCEVen01ValidarVendibleYObtenerDatos.SoatVentaCajero = "";
            reqCEVen01ValidarVendibleYObtenerDatos.SoatVentaVendedor = usuario;
            reqCEVen01ValidarVendibleYObtenerDatos.Usuario = usuario;
            reqCEVen01ValidarVendibleYObtenerDatos.VehiPlaca = placa;

            CSVen01ValidarVendibleYObtenerDatos resCSVen01ValidarVendibleYObtenerDatos = clientVentasClient.Ven01ValidarVendibleYObtenerDatos(reqCEVen01ValidarVendibleYObtenerDatos);
            if (resCSVen01ValidarVendibleYObtenerDatos.Exito)
            {
                Console.WriteLine("Placa VENDIBLE");
                if (resCSVen01ValidarVendibleYObtenerDatos.oSoatDatosIniciales != null)
                {
                    Console.WriteLine("Sin datos");
                    string parDeptoPcFk = "LP";
                    
                    int parVehiculoTipo = 1;
                    int parVehiculoUso = 1;
                    
                    
                    //calculo de prima con datos de ingreso desde pos
                    CEVen02ObtenerPrima reqCEVen02ObtenerPrima = new CEVen02ObtenerPrima();
                    reqCEVen02ObtenerPrima.SeguridadToken = token;
                    reqCEVen02ObtenerPrima.SoatTIntermediarioFk = 0;
                    reqCEVen02ObtenerPrima.SoatTParDepartamentoPcFk = parDeptoPcFk;
                    reqCEVen02ObtenerPrima.SoatTParGestionFk = gestionFk;
                    reqCEVen02ObtenerPrima.SoatTParVehiculoTipoFk = parVehiculoTipo;
                    reqCEVen02ObtenerPrima.SoatTParVehiculoUsoFk = parVehiculoUso;
                    reqCEVen02ObtenerPrima.SoatTParVentaCanalFk = canalVenta;
                    reqCEVen02ObtenerPrima.SoatVentaCajero = "";
                    reqCEVen02ObtenerPrima.SoatVentaVendedor = usuario;
                    reqCEVen02ObtenerPrima.VehiPlaca = placa;
                    reqCEVen02ObtenerPrima.Usuario = usuario;

                    CSVen02ObtenerPrima resCsVen02ObtenerPrima = clientVentasClient.Ven02ObtenerPrima(reqCEVen02ObtenerPrima);
                    Console.WriteLine("Obtener Prima:" + resCsVen02ObtenerPrima.Mensaje);
                    if (resCsVen02ObtenerPrima.Exito)
                    {
                        Console.WriteLine("Prima obtenida:" + resCsVen02ObtenerPrima.Prima.ToString());
                        string nitCi = "4850644";
                        string razonSocial = "Leo QUenta";
                        int primaCobrar = resCsVen02ObtenerPrima.Prima;

                        //efectivizacion de factura
                        CEVen03EfectivizarFactCicl reqCeVen03EfectivizarFactCicl = new CEVen03EfectivizarFactCicl();
                        reqCeVen03EfectivizarFactCicl.FactCorreoCliente = "";
                        reqCeVen03EfectivizarFactCicl.FactNitCi = nitCi;
                        reqCeVen03EfectivizarFactCicl.FactPrima = primaCobrar;
                        reqCeVen03EfectivizarFactCicl.FactRazonSocial = razonSocial;
                        reqCeVen03EfectivizarFactCicl.FactSucursalFk = res.oTransUsuarioDatos.SucursalCodigo;
                        reqCeVen03EfectivizarFactCicl.FactTelefonoCliente = "";
                        reqCeVen03EfectivizarFactCicl.SeguridadToken = token;
                        reqCeVen03EfectivizarFactCicl.SoatRosetaNumero = 0;
                        reqCeVen03EfectivizarFactCicl.SoatTIntermediarioFk = 0;
                        reqCeVen03EfectivizarFactCicl.SoatTParDepartamentoPcFk = parDeptoPcFk;
                        reqCeVen03EfectivizarFactCicl.SoatTParDepartamentoVtFk = parDeptoPcFk;
                        reqCeVen03EfectivizarFactCicl.SoatTParGestionFk = gestionFk;
                        reqCeVen03EfectivizarFactCicl.SoatTParMedioPagoFk = medioPago;
                        reqCeVen03EfectivizarFactCicl.SoatTParVehiculoTipoFk = parVehiculoTipo;
                        reqCeVen03EfectivizarFactCicl.SoatTParVehiculoUsoFk = parVehiculoUso;
                        reqCeVen03EfectivizarFactCicl.SoatTParVentaCanalFk = canalVenta;
                        reqCeVen03EfectivizarFactCicl.SoatVentaCajero = "";
                        reqCeVen03EfectivizarFactCicl.SoatVentaDatosAdi = "VENTA POR POS ATC";
                        reqCeVen03EfectivizarFactCicl.SoatVentaVendedor = usuario;
                        reqCeVen03EfectivizarFactCicl.Usuario = usuario;
                        reqCeVen03EfectivizarFactCicl.VehiPlaca = placa;
                        var resCsVen03EfectivizarFactCicl = clientVentasClient.Ven03EfectivizarFactCicl(reqCeVen03EfectivizarFactCicl);
                        if (resCsVen03EfectivizarFactCicl.Exito)
                        {
                            Console.WriteLine("Exito en la venta" + resCsVen03EfectivizarFactCicl.CodigoRetorno + resCsVen03EfectivizarFactCicl.Mensaje);
                            Console.WriteLine("datos venta" + resCsVen03EfectivizarFactCicl.oSoatDatosCompletosFactura.SoatRosetaNumero.ToString());
                            Console.WriteLine("datos factura" + resCsVen03EfectivizarFactCicl.oSoatDatosCompletosFactura.FactNumero);

                        }
                        else
                        {
                            Console.WriteLine("Error en la efectivizacion factura:"+resCsVen03EfectivizarFactCicl.Mensaje + resCsVen03EfectivizarFactCicl.CodigoRetorno);

                        }

                    }
                    else
                    {
                        Console.WriteLine("Prima no obtenida,error");
                    }




                }
                else
                {
                    Console.WriteLine("Con Datos:" + resCSVen01ValidarVendibleYObtenerDatos.oSoatDatosIniciales.SoatRosetaNumero);

                }

            }
            else
            {

                Console.WriteLine("Placa NO VENDIBLE");
                Console.WriteLine(resCSVen01ValidarVendibleYObtenerDatos.Mensaje);
            }

            /*
           
            */


            //reqCeVen03EfectivizarFactCicl.
            //clientVentasClient.Ven03EfectivizarFactCicl()

            Console.ReadLine();
        }
    }
}
