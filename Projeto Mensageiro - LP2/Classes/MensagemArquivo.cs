using System;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class MensagemArquivo : Mensagem
    {
        public string NomeArquivo { get; private set; }

        public string IdentificadorArquivo { get; private set; }

        public MensagemArquivo(
            Usuario remetente,
            Usuario destinatario,
            string texto,
            string nomeArquivo)
            : this(remetente, destinatario, texto,
                   nomeArquivo, "", DateTime.Now)
        {
        }

        public MensagemArquivo(
            Usuario remetente,
            Usuario destinatario,
            string texto,
            string nomeArquivo,
            DateTime dataHora)
            : this(remetente, destinatario, texto,
                   nomeArquivo, "", dataHora)
        {
        }

        public MensagemArquivo(
            Usuario remetente,
            Usuario destinatario,
            string texto,
            string nomeArquivo,
            string identificadorArquivo,
            DateTime dataHora)
            : base(remetente, destinatario, texto, dataHora)
        {
            if (string.IsNullOrWhiteSpace(nomeArquivo))
                throw new ArgumentException("Informe o nome do arquivo.");

            NomeArquivo = nomeArquivo.Trim();
            IdentificadorArquivo = identificadorArquivo ?? "";
        }

        public override string ToString()
        {
            return base.ToString()
                + " [Arquivo: " + NomeArquivo + "]"
                + " arquivo://" + IdentificadorArquivo;
        }
    }
}