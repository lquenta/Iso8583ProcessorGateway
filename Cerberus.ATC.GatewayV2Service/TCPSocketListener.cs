using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Cerberus.ATC.GatewayV2Service
{
    public class TCPSocketListener
    {
        public static String DEFAULT_FILE_STORE_LOC = "C:\\TCP\\";

        public enum STATE { FILE_NAME_READ, DATA_READ, FILE_CLOSED };

        /// <summary>
        /// Variables that are accessed by other classes indirectly.
        /// </summary>
        private Socket m_clientSocket = null;
        private bool m_stopClient = false;
        private Thread m_clientListenerThread = null;
        private bool m_markedForDeletion = false;

        /// <summary>
        /// Working Variables.
        /// </summary>
        private StringBuilder m_oneLineBuf = new StringBuilder();
        private STATE m_processState = STATE.FILE_NAME_READ;
        private long m_totalClientDataSize = 0;
        private StreamWriter m_cfgFile = null;
        private DateTime m_lastReceiveDateTime;
        private DateTime m_currentReceiveDateTime;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Client Socket Listener Constructor.
        /// </summary>
        /// <param name="clientSocket"></param>
        public TCPSocketListener(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
        ~TCPSocketListener()
        {
            StopSocketListener();
        }

        /// <summary>
        /// Method that starts SocketListener Thread.
        /// </summary>
        public void StartSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_clientListenerThread =
                    new Thread(new ThreadStart(SocketListenerThreadStart));

                m_clientListenerThread.Start();
            }
        }

        /// <summary>
        /// Thread method that does the communication to the client. This 
        /// thread tries to receive from client and if client sends any data
        /// then parses it and again wait for the client data to come in a
        /// loop. The recieve is an indefinite time receive.
        /// </summary>
        private void SocketListenerThreadStart()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[1024];

            m_lastReceiveDateTime = DateTime.Now;
            m_currentReceiveDateTime = DateTime.Now;
            int idle_time = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["idle_time"]);
            Timer t = new Timer(new TimerCallback(CheckClientCommInterval),
                null, idle_time, idle_time);

            while (!m_stopClient)
            {
                 byteBuffer = new Byte[1024];
                try
                {
                    //if (byteBuffer.Length > 0)
                    {
                        size = m_clientSocket.Receive(byteBuffer);
                        m_currentReceiveDateTime = DateTime.Now;
                        byte[] response = ParseReceiveBuffer(byteBuffer, size);
                        m_clientSocket.Send(response);
                    }
                   // size = m_clientSocket.Receive(byteBuffer);
                    //m_currentReceiveDateTime = DateTime.Now;

                    //byte[] echo = byteBuffer;
                    
                    //byte[] response = ParseReceiveBuffer(byteBuffer, size);
                    //m_clientSocket.Send(response);
                    /*byte[] longitudSalida = new byte[] { 35, 11 };
                    byte[] longitudSalida3 = Encoding.ASCII.GetBytes((size + 6).ToString());
                    byte[] longitudSalida1 = new byte[] { byteBuffer[0], byteBuffer[1] };
                    byte[] transaccionFinanciera = new byte[] { byteBuffer[2] };
                    byte[] destinoNII = new byte[] { byteBuffer[3], byteBuffer[4] };
                    byte[] origen = new byte[] { byteBuffer[5], byteBuffer[6] };
                    byte[] responseFinal = longitudSalida3.Concat(transaccionFinanciera).Concat(destinoNII).Concat(origen).Concat(response).ToArray();
                    //byte[] responseFinal = longitudSalida3.Concat(transaccionFinanciera).Concat(origen).Concat(destinoNII).Concat(response).ToArray();
                    logger.Info("Mensaje serializado de respuesta enviado" + BitConverter.ToString(responseFinal));
                    //logger.Info("Echo:" + BitConverter.ToString(echo));
                    //m_clientSocket.Send(echo);
                    string messageHex = "00d060000002330110200000000200000601000030300013313931393139313931393139310171447c424e7c42454e495c447c43427c434f43484142414d42415c447c43487c434855515549534143415c447c4c507c4c412050415a5c477c323031387c323031395c477c323031397c323031395c557c317c504152544943554c41525c557c327c5055424c49434f5c557c337c454a45524349544f5c557c347c504f4c494349415c557c357c4f46494349414c5c567c317c4d4f544f4349434c4554415c567c327c4155544f4d4f56494c";
                    // Translate the passed message into ASCII and store it as a byte array.
                    Byte[] responseWPOS = new Byte[1024];
                    responseWPOS = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Parse(messageHex).Value;

                    //m_clientSocket.Send(responseWPOS);
                    m_clientSocket.Send(responseFinal);
                    //GetWayServicePoint.FileLog("mensaje enviado");
                    //EventLog.WriteEntry("GetWayServiceNuevoLP", "mensaje enviado");
                      */
                }
                catch (SocketException se)
                {
                    logger.Error("Error Socket en el envio:" + se.Message + se.InnerException + se.StackTrace);
                    m_stopClient = true;
                    m_markedForDeletion = true;
                }
            }
            t.Change(Timeout.Infinite, Timeout.Infinite);
            t = null;
        }

        /// <summary>
        /// Method that stops Client SocketListening Thread.
        /// </summary>
        public void StopSocketListener()
        {
            try
            {
                if (m_clientSocket == null) return;
                m_stopClient = true;
                m_clientSocket.Close();

                // Wait for one second for the the thread to stop.
                if (m_clientListenerThread == null) return;
                m_clientListenerThread.Join(1000);

                // If still alive; Get rid of the thread.
                if (m_clientListenerThread.IsAlive)
                {
                    m_clientListenerThread.Abort();
                }
                m_clientListenerThread = null;
                m_clientSocket = null;
                m_markedForDeletion = true;
            }
            catch (Exception ex)
            {
                logger.Error("Error en stop Listener:"+ex.Message+ex.InnerException+ex.StackTrace);
                //throw;
            }
        }

        /// <summary>
        /// Method that returns the state of this object i.e. whether this
        /// object is marked for deletion or not.
        /// </summary>
        /// <returns></returns>
        public bool IsMarkedForDeletion()
        {
            return m_markedForDeletion;
        }

        /// <summary>
        /// This method parses data that is sent by a client using TCP/IP.
        /// As per the "Protocol" between client and this Listener, client 
        /// sends each line of information by appending "CRLF" (Carriage Return
        /// and Line Feed). But since the data is transmitted from client to 
        /// here by TCP/IP protocol, it is not guarenteed that each line that
        /// arrives ends with a "CRLF". So the job of this method is to make a
        /// complete line of information that ends with "CRLF" from the data
        /// that comes from the client and get it processed.
        /// </summary>
        /// <param name="byteBuffer"></param>
        /// <param name="size"></param>
        private byte[] ParseReceiveBuffer(Byte[] byteBuffer, int size)
        {
            
            //string data = Encoding.ASCII.GetString(byteBuffer, 0, size);
            //GetWayServicePoint.FileLog("mensaje recibido");
            //EventLog.WriteEntry("GetWayServiceNuevoLP", "mensaje recibido");
            ////->SocketHelper socketBisa = new SocketHelper();
            ////->byte[] byteRespuesta = socketBisa.EnviarTramaBisa(byteBuffer);
            //aca debemos poner el proceso de gateway pa la llamada al BISA
            byte[] byteRespuesta=new byte[]{};
            MTI_ProcessingCodeRouter.ProcesarMensaje(byteBuffer, out byteRespuesta);
            return byteRespuesta;
            ///->return byteRespuesta;


            //int lineEndIndex = 0;

            /* do
             {
                 lineEndIndex = data.IndexOf("\r\n");
                 if (lineEndIndex != -1)
                 {
                     m_oneLineBuf = m_oneLineBuf.Append(data, 0, lineEndIndex + 2);
                     ProcessClientData(m_oneLineBuf.ToString());
                     m_oneLineBuf.Remove(0, m_oneLineBuf.Length);
                     data = data.Substring(lineEndIndex + 2,
                         data.Length - lineEndIndex - 2);
                 }
                 else
                 {
                     // Just append to the existing buffer.
                     m_oneLineBuf = m_oneLineBuf.Append(data);
                 }
             } while (lineEndIndex != -1);*/

        }

        /// <summary>
        /// Method that Process the client data as per the protocol. 
        /// The protocol works like this. 
        /// 1. Client first send a file name that ends with "CRLF".
        /// 
        /// 2. This SocketListener has to return the length of the file name
        /// to the client for validation. If the length matches with the length
        /// what  client had sent earlier, then client starts sending lines of
        /// information. Otherwise socket will be closed by the client.
        /// 
        /// 3. Each line of information that client sends will end with "CRLF".
        /// 
        /// 4. This socketListener has to store each line of information in 
        /// a text file whoose file name has been sent by the client earlier.
        /// 
        /// 5. As a last line of information client sends "[EOF]" line which
        /// also ends with "CRLF" ("\r\n"). This signals this SocketListener
        /// for an end of file and intern this SocketListener sends the total
        /// length of the data (lines of information excludes file name that
        /// was sent earlier) back to client for validation.
        /// </summary>
        /// <param name="oneLine"></param>
        private void ProcessClientData(String oneLine)
        {
            switch (m_processState)
            {
                case STATE.FILE_NAME_READ:
                    m_processState = STATE.DATA_READ;
                    int length = oneLine.Length;
                    if (length <= 2)
                    {
                        m_processState = STATE.FILE_CLOSED;
                        length = -1;
                    }
                    else
                    {
                        try
                        {
                            m_cfgFile = new StreamWriter(DEFAULT_FILE_STORE_LOC + oneLine.Substring(0, length - 2));
                        }
                        catch (Exception e)
                        {
                            m_processState = STATE.FILE_CLOSED;
                            length = -1;
                        }
                    }

                    try
                    {
                        m_clientSocket.Send(BitConverter.GetBytes(length));
                    }
                    catch (SocketException se)
                    {
                        m_processState = STATE.FILE_CLOSED;
                    }
                    break;
                case STATE.DATA_READ:
                    m_totalClientDataSize += oneLine.Length;
                    m_cfgFile.Write(oneLine);
                    m_cfgFile.Flush();
                    if (oneLine.ToUpper().Equals("[EOF]\r\n"))
                    {
                        try
                        {
                            m_cfgFile.Close();
                            m_cfgFile = null;
                            m_clientSocket.Send(BitConverter.GetBytes(m_totalClientDataSize));
                        }
                        catch (SocketException se)
                        {
                        }
                        m_processState = STATE.FILE_CLOSED;
                        m_markedForDeletion = true;
                    }
                    break;
                case STATE.FILE_CLOSED:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Method that checks whether there are any client calls for the
        /// last 15 seconds or not. If not this client SocketListener will
        /// be closed.
        /// </summary>
        /// <param name="o"></param>
        private void CheckClientCommInterval(object o)
        {
            if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
            {
                this.StopSocketListener();
            }
            else
            {
                m_lastReceiveDateTime = m_currentReceiveDateTime;
            }
        }
    }
}
