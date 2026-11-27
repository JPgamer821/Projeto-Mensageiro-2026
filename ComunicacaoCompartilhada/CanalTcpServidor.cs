
using System;
using System.Net;
using System.Net.Sockets;

namespace ComunicacaoCompartilhada
{
    public class CanalTcpServidor
    {
        private TcpListener servidor;

        public CanalTcpServidor(int porta)
        {
            servidor = new TcpListener(IPAddress.Any, porta);
        }

        public void Iniciar()
        {
            servidor.Start();

            Console.WriteLine(
                "Servidor TCP aguardando conexoes...");
        }

        public TcpClient AceitarConexao()
        {
            return servidor.AcceptTcpClient();
        }

        public void Encerrar()
        {
            if (servidor != null)
                servidor.Stop();
        }
    }
}
