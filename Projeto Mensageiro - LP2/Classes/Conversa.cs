using System;
using System.Collections.Generic;
using System.Linq;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class Conversa
    {
        public Usuario Usuario1 { get; private set; }
        public Usuario Usuario2 { get; private set; }
        private readonly List<Mensagem> mensagens = new List<Mensagem>();

        public Conversa(Usuario usuario1, Usuario usuario2)
        {
            if (usuario1 == null) throw new ArgumentNullException(nameof(usuario1));
            if (usuario2 == null) throw new ArgumentNullException(nameof(usuario2));
            if (usuario1.Equals(usuario2))
                throw new ArgumentException("Uma conversa precisa de dois usuarios diferentes.");
            Usuario1 = usuario1;
            Usuario2 = usuario2;
        }

        public void Enviar(Usuario remetente, string texto)
        {
            Usuario destinatario = ObterOutroParticipante(remetente);
            AdicionarMensagem(new MensagemTexto(remetente, destinatario, texto));
        }

        public void EnviarArquivo(Usuario remetente, string texto, string nomeArquivo)
        {
            Usuario destinatario = ObterOutroParticipante(remetente);
            AdicionarMensagem(new MensagemArquivo(remetente, destinatario, texto, nomeArquivo));
        }

        public void AdicionarMensagem(Mensagem mensagem)
        {
            if (mensagem == null) throw new ArgumentNullException(nameof(mensagem));
            if (mensagem is MensagemSistema)
            {

                if (mensagem.Destinatario != null && !Participa(mensagem.Destinatario))
                    throw new InvalidOperationException("O aviso nao pertence a esta conversa.");
            }
            else
            {
                if (!Participa(mensagem.Remetente) || !Participa(mensagem.Destinatario))
                    throw new InvalidOperationException("A mensagem nao pertence a esta conversa.");
                if (mensagem.Remetente.Equals(mensagem.Destinatario))
                    throw new InvalidOperationException("Destinatario precisa ser diferente do remetente.");
            }
            mensagens.Add(mensagem);
        }

        public bool Participa(Usuario usuario)
        {
            return usuario != null && (usuario.Equals(Usuario1) || usuario.Equals(Usuario2));
        }

        public Usuario ObterOutroParticipante(Usuario usuario)
        {
            if (usuario != null && usuario.Equals(Usuario1)) return Usuario2;
            if (usuario != null && usuario.Equals(Usuario2)) return Usuario1;
            throw new InvalidOperationException("O usuario nao participa desta conversa.");
        }

        public IReadOnlyList<Mensagem> ObterMensagens()
        {
            return mensagens.OrderBy(m => m.DataHora).ToList().AsReadOnly();
        }

        public void MostrarMensagens()
        {
            foreach (Mensagem mensagem in ObterMensagens())
                Console.WriteLine(mensagem.ToString());
        }
    }
}
