using System;
using System.IO;
using System.Linq;
using Projeto_Mensageiro___LP2.Classes;
using Projeto_Mensageiro___LP2.Comunicacao;

namespace Projeto_Mensageiro___LP2.Servidor
{
    internal class ServidorMensageiro
    {
        private GerenciadorDeUsuarios gerenciador;
        private ServicoDeHistorico historico;
        private string pastaArquivos;

        public ServidorMensageiro(
            GerenciadorDeUsuarios gerenciador,
            ServicoDeHistorico historico)
        {
            this.gerenciador = gerenciador;
            this.historico = historico;

            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado"
            );

            pastaArquivos = Path.Combine(pasta, "Arquivos");

            Directory.CreateDirectory(pastaArquivos);

            Console.WriteLine("Pasta de arquivos: " + pastaArquivos);
        }

        public string Processar(string linha)
        {
            try
            {
                ComandoProtocolo comando =
                    AnalisadorProtocolo.Analisar(linha);

                switch (comando.Tipo)
                {
                    case TipoComando.Login:
                        return Login(comando.Apelido);

                    case TipoComando.Enviar:
                        return Enviar(comando);

                    case TipoComando.EnviarArquivo:
                        return EnviarArquivo(comando);

                    case TipoComando.Listar:
                        return Listar(
                            comando.Apelido,
                            comando.Destinatario
                        );

                    case TipoComando.Sair:
                        return Sair(comando.Apelido);

                    default:
                        return "ERRO|COMANDO_INVALIDO";
                }
            }
            catch (ProtocoloInvalidoException erro)
            {
                return "ERRO|PROTOCOLO_INVALIDO|" + erro.Message;
            }
        }

        private string Login(string apelido)
        {
            Usuario usuario = gerenciador.Buscar(apelido);

            if (usuario == null)
            {
                usuario = new Usuario(
                    apelido, gerenciador.Listar().Count + 1);

                gerenciador.Cadastrar(usuario);
            }

            if (usuario.Online)
                return "ERRO|USUARIO_JA_ONLINE";

            usuario.Entrar();

            historico.Guardar(
                new MensagemSistema(
                    usuario, TipoAvisoSistema.Entrada));

            return "OK|LOGIN|" + usuario.Apelido;
        }


        private string Enviar(ComandoProtocolo comando)
        {
            Usuario remetente =
                gerenciador.Buscar(comando.Remetente);

            if (remetente == null || !remetente.Online)
                return "ERRO|REMETENTE_NAO_CONECTADO";

            Usuario destinatario =
                gerenciador.ObterOuCadastrar(comando.Destinatario);

            Mensagem mensagem = new MensagemTexto(
                remetente,
                destinatario,
                comando.Texto,
                comando.DataHora.Value
            );

            historico.Guardar(mensagem);

            return "OK|ENVIAR|Mensagem registrada.";
        }



        private string EnviarArquivo(ComandoProtocolo comando)
        {
            Usuario remetente =
                gerenciador.Buscar(comando.Remetente);

            if (remetente == null || !remetente.Online)
                return "ERRO|REMETENTE_NAO_CONECTADO";

            Usuario destinatario =
                gerenciador.ObterOuCadastrar(comando.Destinatario);

            if (comando.DadosArquivo == null ||
                comando.DadosArquivo.Length == 0)
            {
                return "ERRO|ARQUIVO_VAZIO";
            }

            if (comando.DadosArquivo.Length > 5 * 1024 * 1024)
                return "ERRO|ARQUIVO_MUITO_GRANDE";

            string nomeOriginal = Path.GetFileName(
                comando.NomeArquivo);

            if (string.IsNullOrWhiteSpace(nomeOriginal))
                return "ERRO|NOME_ARQUIVO_INVALIDO";

            string extensao = Path.GetExtension(nomeOriginal);

            extensao = new string(
                extensao.Where(c =>
                    char.IsLetterOrDigit(c) || c == '.').ToArray());

            string identificador =
                Guid.NewGuid().ToString("N") + extensao;

            string caminhoCompleto = Path.Combine(
                pastaArquivos, identificador);

            try
            {
                File.WriteAllBytes(
                    caminhoCompleto, comando.DadosArquivo);

                Console.WriteLine("Arquivo salvo em: " + caminhoCompleto);

                MensagemArquivo mensagem = new MensagemArquivo(
                    remetente,
                    destinatario,
                    comando.Texto,
                    nomeOriginal,
                    identificador,
                    comando.DataHora.Value
                );

                historico.Guardar(mensagem);

                return "OK|ENVIARARQUIVO|Arquivo recebido e registrado.";
            }
            catch (Exception)
            {
                if (File.Exists(caminhoCompleto))
                    File.Delete(caminhoCompleto);

                return "ERRO|FALHA_AO_SALVAR_ARQUIVO";
            }
        }


        private string Listar(string apelido, string contato)
        {
            Usuario usuario = gerenciador.Buscar(apelido);

            if (usuario == null || !usuario.Online)
                return "ERRO|USUARIO_NAO_CONECTADO";

            var mensagens = historico.Listar()
                .Where(m =>
                    !(m is MensagemSistema) &&
                    (

                        (contato == null &&
                            (
                                (m.Remetente != null &&
                                 m.Remetente.Equals(usuario)) ||

                                (m.Destinatario != null &&
                                 m.Destinatario.Equals(usuario))
                            )
                        )

                        ||

                        (contato != null &&
                            m.Remetente != null &&
                            m.Destinatario != null &&
                            (
                                (
                                    m.Remetente.Equals(usuario) &&
                                    string.Equals(
                                        m.Destinatario.Apelido,
                                        contato,
                                        StringComparison.OrdinalIgnoreCase)
                                )
                                ||
                                (
                                    m.Destinatario.Equals(usuario) &&
                                    string.Equals(
                                        m.Remetente.Apelido,
                                        contato,
                                        StringComparison.OrdinalIgnoreCase)
                                )
                            )
                        )
                    )
                );

            if (!mensagens.Any())
                return "OK|LISTAR|Nenhuma mensagem encontrada.";

            return "OK|LISTAR|" +
                string.Join(Environment.NewLine,
                mensagens.Select(m => m.ToString()));
        }

        private string Sair(string apelido)
        {
            Usuario usuario = gerenciador.Buscar(apelido);

            if (usuario == null || !usuario.Online)
                return "ERRO|USUARIO_NAO_CONECTADO";

            historico.Guardar(
                new MensagemSistema(
                    usuario, TipoAvisoSistema.Saida));

            usuario.Sair();

            return "OK|SAIR|" + usuario.Apelido;
        }
    }
}