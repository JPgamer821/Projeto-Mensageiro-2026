
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ComunicacaoCompartilhada
{
    public class ConexaoTcp : ICanalDeComunicacao, IDisposable
    {
        private TcpClient cliente;
        private NetworkStream fluxo;

        private List<byte> bufferRecebimento =
            new List<byte>();

        private readonly object bloqueioEnvio =
            new object();

        public ConexaoTcp(TcpClient cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            this.cliente = cliente;
            fluxo = cliente.GetStream();
        }

        public void Enviar(string mensagem)
        {
            if (cliente == null || !cliente.Connected)
                throw new IOException("Cliente desconectado.");

            string codificada = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(mensagem));

            byte[] dados = Encoding.UTF8.GetBytes(
                codificada + "\n");

            lock (bloqueioEnvio)
            {
                fluxo.Write(dados, 0, dados.Length);
                fluxo.Flush();
            }
        }

        public string Receber()
        {
            if (cliente == null || fluxo == null)
                return null;

            try
            {
                while (fluxo.DataAvailable)
                {
                    int valor = fluxo.ReadByte();

                    if (valor == -1)
                        return null;

                    if (valor == '\n')
                    {
                        string codificada =
                            Encoding.UTF8.GetString(
                                bufferRecebimento.ToArray());

                        bufferRecebimento.Clear();

                        try
                        {
                            byte[] dados =
                                Convert.FromBase64String(
                                    codificada.TrimEnd('\r'));

                            return Encoding.UTF8.GetString(dados);
                        }
                        catch (FormatException)
                        {
                            throw new IOException(
                                "Comando TCP recebido em formato inválido.");
                        }
                    }

                    bufferRecebimento.Add((byte)valor);

                    if (bufferRecebimento.Count > 15 * 1024 * 1024)
                        throw new IOException(
                            "Mensagem TCP muito grande.");
                }
            }
            catch (IOException)
            {
                Dispose();
                throw;
            }

            return null;
        }

        public void Dispose()
        {
            if (fluxo != null)
            {
                fluxo.Close();
                fluxo = null;
            }

            if (cliente != null)
            {
                cliente.Close();
                cliente = null;
            }
        }
    }
}
