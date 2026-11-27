using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using ComunicacaoCompartilhada;

namespace Projeto_Mensageiro___LP2.Comunicacao
{
    internal class CanalArquivo : ICanalDeComunicacao
    {
        private string caminho;
        private string nomeMutex;

        public CanalArquivo(string caminho)
        {
            this.caminho = Path.GetFullPath(caminho);
            nomeMutex = ObterNomeMutex(this.caminho);

            ComBloqueio(() =>
            {
                if (!File.Exists(this.caminho))
                {
                    using (File.Create(this.caminho))
                    {
                    }
                }

                return true;
            });
        }

        public void Enviar(string mensagem)
        {
            ComBloqueio(() =>
            {
                File.AppendAllText(
                    caminho,
                    mensagem + Environment.NewLine
                );

                return true;
            });
        }

        public string Receber()
        {
            return ComBloqueio(() =>
            {
                string[] mensagens = File.ReadAllLines(caminho);

                if (mensagens.Length == 0)
                    return null;

                string mensagem = mensagens[0];

                File.WriteAllLines(
                    caminho,
                    mensagens.Skip(1),
                    Encoding.UTF8
                );

                return mensagem;
            });
        }

        private T ComBloqueio<T>(Func<T> operacao)
        {
            using (Mutex mutex = new Mutex(false, nomeMutex))
            {
                bool possuiBloqueio = false;

                try
                {
                    try
                    {
                        possuiBloqueio =
                            mutex.WaitOne(TimeSpan.FromSeconds(10));
                    }
                    catch (AbandonedMutexException)
                    {
                        possuiBloqueio = true;
                    }

                    if (!possuiBloqueio)
                    {
                        throw new IOException(
                            "Tempo esgotado ao tentar acessar o arquivo."
                        );
                    }

                    return operacao();
                }
                finally
                {
                    if (possuiBloqueio)
                        mutex.ReleaseMutex();
                }
            }
        }

        private static string ObterNomeMutex(string caminho)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(
                        caminho.ToUpperInvariant()
                    )
                );

                return @"Local\Mensageiro_" +
                    BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
