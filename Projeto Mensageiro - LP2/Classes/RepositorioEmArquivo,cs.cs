using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Projeto_Mensageiro___LP2.Comunicacao;

namespace Projeto_Mensageiro___LP2.Classes
{
    internal class RepositorioEmArquivo : IRepositorioDeMensagens
    {
        private string caminho;

        public RepositorioEmArquivo(string caminho)
        {
            this.caminho = caminho;

            if (!File.Exists(caminho))
                File.WriteAllText(caminho, "");
        }

        public void Guardar(Mensagem m)
        {
            if (m == null)
                throw new ArgumentNullException(nameof(m));

            string data = m.DataHora.ToString(
                "o", CultureInfo.InvariantCulture);

            string linha;

            MensagemSistema sistema = m as MensagemSistema;
            MensagemArquivo arquivo = m as MensagemArquivo;
            MensagemTexto texto = m as MensagemTexto;

            if (sistema != null)
            {
                string destinatario = sistema.Destinatario == null
                    ? ""
                    : sistema.Destinatario.Apelido;

                string relacionado = sistema.UsuarioRelacionado == null
                    ? ""
                    : sistema.UsuarioRelacionado.Apelido;

                linha = "SISTEMA|" + data + "|" +
                        destinatario + "|" +
                        sistema.Tipo + "|" +
                        relacionado + "|" +
                        AnalisadorProtocolo.CodificarTexto(
                            sistema.Conteudo);
            }
            else if (arquivo != null)
            {
                linha = "ARQUIVO|" + data + "|" +
                        arquivo.Remetente.Apelido + "|" +
                        arquivo.Destinatario.Apelido + "|" +
                        AnalisadorProtocolo.CodificarTexto(
                            arquivo.Conteudo) + "|" +
                        AnalisadorProtocolo.CodificarTexto(
                            arquivo.NomeArquivo) + "|" +
                        AnalisadorProtocolo.CodificarTexto(
                            arquivo.IdentificadorArquivo);
            }
            else if (texto != null)
            {
                linha = "TEXTO|" + data + "|" +
                        texto.Remetente.Apelido + "|" +
                        texto.Destinatario.Apelido + "|" +
                        AnalisadorProtocolo.CodificarTexto(
                            texto.Conteudo);
            }
            else
            {
                throw new NotSupportedException(
                    "Tipo de mensagem não suportado.");
            }

            File.AppendAllText(
                caminho,
                linha + Environment.NewLine,
                Encoding.UTF8);
        }

        public List<Mensagem> Listar()
        {
            List<Mensagem> mensagens = new List<Mensagem>();

            Dictionary<string, Usuario> usuarios =
                new Dictionary<string, Usuario>(
                    StringComparer.OrdinalIgnoreCase);

            string[] linhas = File.ReadAllLines(
                caminho, Encoding.UTF8);

            foreach (string linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                string[] partes = linha.Split('|');

                DateTime data = DateTime.ParseExact(
                    partes[1],
                    "o",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                if (partes[0] == "TEXTO" && partes.Length == 5)
                {
                    Usuario remetente = ObterUsuario(
                        usuarios, partes[2]);

                    Usuario destinatario = ObterUsuario(
                        usuarios, partes[3]);

                    string conteudo =
                        AnalisadorProtocolo.DecodificarTexto(partes[4]);

                    mensagens.Add(new MensagemTexto(
                        remetente, destinatario, conteudo, data));
                }
                else if (partes[0] == "ARQUIVO" &&
                         (partes.Length == 6 || partes.Length == 7))
                {
                    Usuario remetente = ObterUsuario(
                        usuarios, partes[2]);

                    Usuario destinatario = ObterUsuario(
                        usuarios, partes[3]);

                    string conteudo =
                        AnalisadorProtocolo.DecodificarTexto(partes[4]);

                    string nomeArquivo =
                        AnalisadorProtocolo.DecodificarTexto(partes[5]);

                    string identificador = partes.Length == 7
                        ? AnalisadorProtocolo.DecodificarTexto(partes[6])
                        : "";

                    mensagens.Add(new MensagemArquivo(
                        remetente,
                        destinatario,
                        conteudo,
                        nomeArquivo,
                        identificador,
                        data));
                }
                else if (partes[0] == "SISTEMA" && partes.Length == 6)
                {
                    Usuario destinatario = partes[2] == ""
                        ? null
                        : ObterUsuario(usuarios, partes[2]);

                    TipoAvisoSistema tipo =
                        (TipoAvisoSistema)Enum.Parse(
                            typeof(TipoAvisoSistema), partes[3]);

                    Usuario relacionado = partes[4] == ""
                        ? null
                        : ObterUsuario(usuarios, partes[4]);

                    string conteudo =
                        AnalisadorProtocolo.DecodificarTexto(partes[5]);

                    if (tipo == TipoAvisoSistema.Geral)
                    {
                        mensagens.Add(new MensagemSistema(
                            conteudo, destinatario, data));
                    }
                    else
                    {
                        mensagens.Add(new MensagemSistema(
                            relacionado, tipo, data));
                    }
                }
                else
                {
                    throw new FormatException(
                        "Registro inválido no histórico: " + linha);
                }
            }

            return mensagens;
        }

        private Usuario ObterUsuario(
            Dictionary<string, Usuario> usuarios,
            string apelido)
        {
            if (!usuarios.ContainsKey(apelido))
            {
                usuarios[apelido] = new Usuario(
                    apelido, usuarios.Count + 1);
            }

            return usuarios[apelido];
        }
    }
}