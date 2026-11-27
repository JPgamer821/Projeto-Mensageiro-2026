using System;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class MensagemTexto : Mensagem
    {
        public string Texto { get { return Conteudo; } }

        public MensagemTexto(Usuario remetente, Usuario destinatario, string texto)
            : base(remetente, destinatario, texto) { }

        public MensagemTexto(Usuario remetente, Usuario destinatario, string texto, DateTime dataHora)
            : base(remetente, destinatario, texto, dataHora) { }
    }
}
