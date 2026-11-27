using System;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class Mensagem
    {

        public Usuario Remetente { get; private set; }
        public Usuario Destinatario { get; private set; }
        public DateTime DataHora { get; private set; }
        public string Conteudo { get; private set; }

        public Mensagem(Usuario remetente, Usuario destinatario, string conteudo)
            : this(remetente, destinatario, conteudo, DateTime.Now, false)
        {
        }
        protected Mensagem(string aviso, Usuario destinatario, DateTime dataHora)
    : this(null, destinatario, aviso, dataHora, true)
        {
        }

        public Mensagem(Usuario remetente, Usuario destinatario, string conteudo, DateTime dataHora)
            : this(remetente, destinatario, conteudo, dataHora, false)
        {
        }

        protected Mensagem(string aviso, Usuario destinatario = null)
            : this(null, destinatario, aviso, DateTime.Now, true)
        {
        }

        private Mensagem(Usuario remetente, Usuario destinatario, string conteudo,
                         DateTime dataHora, bool avisoSistema)
        {
            if (!avisoSistema && remetente == null)
                throw new ArgumentNullException(nameof(remetente));
            if (!avisoSistema && destinatario == null)
                throw new ArgumentNullException(nameof(destinatario));
            if (string.IsNullOrWhiteSpace(conteudo))
                throw new ArgumentException("A mensagem nao pode estar vazia.", nameof(conteudo));
            if (dataHora == DateTime.MinValue)
                throw new ArgumentException("Informe uma data valida.", nameof(dataHora));

            Remetente = remetente;
            Destinatario = destinatario;
            Conteudo = conteudo;
            DataHora = dataHora;
        }

        public override string ToString()
        {
            string autor = Remetente == null ? "SISTEMA" : Remetente.Apelido;
            return $"{DataHora:HH:mm} - {autor}: {Conteudo}";
        }
    }
}
