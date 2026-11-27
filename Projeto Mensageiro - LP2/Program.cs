
using System;
using System.IO;
using System.Threading;
using Projeto_Mensageiro___LP2.Classes;
using Projeto_Mensageiro___LP2.Servidor;
using Projeto_Mensageiro___LP2.Comunicacao;

namespace Projeto_Mensageiro___LP2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GerenciadorDeUsuarios gerenciador =
                new GerenciadorDeUsuarios();

            RepositorioEmArquivo repositorio =
                new RepositorioEmArquivo("historico.txt");

            ServicoDeHistorico historico =
                new ServicoDeHistorico(repositorio);

            ServidorMensageiro servidor =
                new ServidorMensageiro(gerenciador, historico);
            GerenciadorConexoesTcp gerenciadorTcp =
                new GerenciadorConexoesTcp(servidor, 5000);

            gerenciadorTcp.Iniciar();
            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado"
            );

            Directory.CreateDirectory(pasta);

            CanalArquivo entrada = new CanalArquivo(
                Path.Combine(pasta, "entrada.txt")
            );

            Console.WriteLine("=== SERVIDOR MENSAGEIRO ===");
            Console.WriteLine("Aguardando comandos...");

            while (true)
            {
                string comando = entrada.Receber();

                if (comando == null)
                {
                    Thread.Sleep(200);
                    continue;
                }

                if (string.Equals(comando, "FIM",
                    StringComparison.OrdinalIgnoreCase))
                    break;

                if (comando.StartsWith(
                    "ENVIARARQUIVO|",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string[] dados = comando.Split('|');

                    string nome = dados.Length >= 6
                        ? AnalisadorProtocolo.DecodificarTexto(dados[5])
                        : "arquivo";

                    Console.WriteLine(
                        "Recebido: ENVIARARQUIVO | Anexo: " + nome
                    );
                }
                else
                {
                    Console.WriteLine("Recebido: " + comando);
                }

                string resposta = servidor.Processar(comando);

                string[] campos = comando.Split('|');

                if (campos.Length < 2)
                    continue;

                string apelido = campos[1].Trim();

                if (string.IsNullOrWhiteSpace(apelido))
                    continue;

                EnviarParaUsuario(
                    pasta,
                    apelido,
                    resposta
                );

                bool mensagemRegistrada =
                    resposta.StartsWith("OK|ENVIAR|") ||
                    resposta.StartsWith("OK|ENVIARARQUIVO|");

                if (mensagemRegistrada && campos.Length >= 3)
                {
                    string remetente = campos[1].Trim();
                    string destinatario = campos[2].Trim();

                    Usuario usuarioDestino =
                        gerenciador.Buscar(destinatario);

                    if (usuarioDestino != null && usuarioDestino.Online)
                    {
                        EnviarParaUsuario(
                            pasta,
                            destinatario,
                            "NOVA_MENSAGEM"
                        );

                        Console.WriteLine(
                            "Notificação enviada para: " + destinatario);
                    }
                    else
                    {
                        Console.WriteLine(
                            "Mensagem armazenada para usuário offline: " +
                            destinatario);
                    }

                    if (!remetente.Equals(
                        destinatario,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        EnviarParaUsuario(
                            pasta,
                            destinatario,
                            "NOVA_MENSAGEM"
                        );
                    }

                    Console.WriteLine(
                        "Notificação enviada para: " +
                        destinatario
                    );
                }

                Console.WriteLine("Resposta: " + resposta);
                Console.WriteLine();
            }

            gerenciadorTcp.Encerrar();

            Console.WriteLine("Servidor encerrado.");
            Console.ReadLine();
        }

        private static void EnviarParaUsuario(
            string pasta,
            string apelido,
            string mensagem)
        {
            string nomeArquivo = "saida_" + apelido + ".txt";

            string caminhoResposta = Path.Combine(
                pasta,
                nomeArquivo
            );

            CanalArquivo saida =
                new CanalArquivo(caminhoResposta);

            mensagem = mensagem
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\n");

            saida.Enviar(mensagem);
        }
    }
}
