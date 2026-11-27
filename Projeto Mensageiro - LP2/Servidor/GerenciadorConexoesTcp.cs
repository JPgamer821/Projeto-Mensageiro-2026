
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using ComunicacaoCompartilhada;
using Projeto_Mensageiro___LP2.Comunicacao;

namespace Projeto_Mensageiro___LP2.Servidor
{
    internal class GerenciadorConexoesTcp
    {
        private CanalTcpServidor canalServidor;
        private ServidorMensageiro servidorMensageiro;

        private Thread threadAceitacao;
        private bool executando;

        private readonly int porta;

        private readonly object bloqueioProcessamento =
            new object();

        private readonly object bloqueioConexoes =
            new object();

        private Dictionary<string, ConexaoTcp> conexoes =
            new Dictionary<string, ConexaoTcp>(
                StringComparer.OrdinalIgnoreCase);

        public GerenciadorConexoesTcp(
            ServidorMensageiro servidorMensageiro,
            int porta)
        {
            if (servidorMensageiro == null)
                throw new ArgumentNullException(
                    nameof(servidorMensageiro));

            this.servidorMensageiro = servidorMensageiro;
            this.porta = porta;
        }

        public void Iniciar()
        {
            if (executando)
                return;

            canalServidor = new CanalTcpServidor(porta);
            canalServidor.Iniciar();

            executando = true;

            threadAceitacao = new Thread(AguardarConexoes);
            threadAceitacao.IsBackground = true;
            threadAceitacao.Start();

            Console.WriteLine(
                "Gerenciador TCP iniciado na porta " + porta);
        }

        private void AguardarConexoes()
        {
            while (executando)
            {
                try
                {
                    TcpClient cliente =
                        canalServidor.AceitarConexao();

                    Console.WriteLine(
                        "Novo cliente TCP conectado: " +
                        cliente.Client.RemoteEndPoint);

                    Thread threadCliente = new Thread(
                        () => AtenderCliente(cliente));

                    threadCliente.IsBackground = true;
                    threadCliente.Start();
                }
                catch (SocketException)
                {
                    if (executando)
                        Console.WriteLine(
                            "Erro ao aceitar conexão TCP.");
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private void AtenderCliente(TcpClient cliente)
        {
            ConexaoTcp conexao = null;
            string apelidoConectado = null;

            try
            {
                conexao = new ConexaoTcp(cliente);

                while (executando && cliente.Connected)
                {
                    string mensagem = conexao.Receber();

                    if (mensagem == null)
                    {
                        if (cliente.Client.Poll(
                            0, SelectMode.SelectRead) &&
                            cliente.Client.Available == 0)
                        {
                            break;
                        }

                        Thread.Sleep(10);
                        continue;
                    }

                    Console.WriteLine(
                        "Comando TCP recebido: " + mensagem);

                    ComandoProtocolo comando;

                    try
                    {
                        comando = AnalisadorProtocolo.Analisar(
                            mensagem);
                    }
                    catch (ProtocoloInvalidoException)
                    {
                        string erro = servidorMensageiro.Processar(
                            mensagem);

                        conexao.Enviar(erro);
                        continue;
                    }

                    string apelido = ObterApelido(comando);

                    if (!string.IsNullOrWhiteSpace(apelido))
                    {
                        RegistrarConexao(apelido, conexao);
                        apelidoConectado = apelido;
                    }

                    string resposta;

                    lock (bloqueioProcessamento)
                    {
                        resposta =
                            servidorMensageiro.Processar(mensagem);
                    }

                    conexao.Enviar(resposta);

                    Console.WriteLine(
                        "Resposta enviada: " + resposta);

                    bool mensagemRegistrada =
                        resposta.StartsWith("OK|ENVIAR|") ||
                        resposta.StartsWith("OK|ENVIARARQUIVO|");

                    if (mensagemRegistrada &&
                        !string.IsNullOrWhiteSpace(comando.Destinatario))
                    {
                        NotificarUsuario(comando.Destinatario);
                    }
                }
            }
            catch (Exception erro)
            {
                Console.WriteLine(
                    "Erro na conexão TCP: " + erro.Message);
            }
            finally
            {
                if (apelidoConectado != null && conexao != null)
                {
                    RemoverConexao(apelidoConectado, conexao);
                }

                if (conexao != null)
                    conexao.Dispose();
                else
                    cliente.Close();

                Console.WriteLine("Cliente TCP desconectado.");
            }
        }

        private string ObterApelido(ComandoProtocolo comando)
        {
            switch (comando.Tipo)
            {
                case TipoComando.Listar:
                case TipoComando.Sair:
                    return comando.Apelido;

                case TipoComando.Enviar:
                case TipoComando.EnviarArquivo:
                    return comando.Remetente;

                default:
                    return null;
            }
        }

        private void RegistrarConexao(
            string apelido,
            ConexaoTcp conexao)
        {
            lock (bloqueioConexoes)
            {
                conexoes[apelido] = conexao;
            }

            Console.WriteLine(
                "Conexão associada ao usuário: " + apelido);
        }

        private void RemoverConexao(
            string apelido,
            ConexaoTcp conexao)
        {
            lock (bloqueioConexoes)
            {
                ConexaoTcp atual;

                if (conexoes.TryGetValue(apelido, out atual) &&
                    ReferenceEquals(atual, conexao))
                {
                    conexoes.Remove(apelido);
                }
            }
        }

        private void NotificarUsuario(string apelido)
        {
            ConexaoTcp conexao;

            lock (bloqueioConexoes)
            {
                if (!conexoes.TryGetValue(apelido, out conexao))
                {
                    Console.WriteLine(
                        "Usuário sem conexão TCP: " + apelido);

                    return;
                }
            }

            try
            {
                conexao.Enviar("NOVA_MENSAGEM");

                Console.WriteLine(
                    "Notificação TCP enviada para: " + apelido);
            }
            catch (Exception erro)
            {
                Console.WriteLine(
                    "Não foi possível notificar " +
                    apelido + ": " + erro.Message);

                RemoverConexao(apelido, conexao);
            }
        }

        public void Encerrar()
        {
            executando = false;

            if (canalServidor != null)
                canalServidor.Encerrar();

            lock (bloqueioConexoes)
            {
                foreach (ConexaoTcp conexao in conexoes.Values)
                {
                    conexao.Dispose();
                }

                conexoes.Clear();
            }

            Console.WriteLine("Servidor TCP encerrado.");
        }
    }
}
