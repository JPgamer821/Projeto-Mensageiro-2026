using System;

namespace Projeto_Mensageiro___LP2.Classes
{
    public enum TipoAvisoSistema { Geral, Entrada, Saida }

    public class MensagemSistema : Mensagem
    {
        public TipoAvisoSistema Tipo { get; private set; }
        public Usuario UsuarioRelacionado { get; private set; }

        public MensagemSistema(string texto, Usuario destinatario = null)
            : base(texto, destinatario)
        {
            Tipo = TipoAvisoSistema.Geral;
        }

        public MensagemSistema(string texto, Usuario destinatario,
                               DateTime dataHora)
            : base(texto, destinatario, dataHora)
        {
            Tipo = TipoAvisoSistema.Geral;
        }

        public MensagemSistema(Usuario usuario, TipoAvisoSistema tipo)
            : this(usuario, tipo, DateTime.Now)
        {
        }

        public MensagemSistema(Usuario usuario, TipoAvisoSistema tipo,
                               DateTime dataHora)
            : base(CriarTexto(usuario, tipo), null, dataHora)
        {
            UsuarioRelacionado = usuario;
            Tipo = tipo;
        }

        private static string CriarTexto(Usuario usuario, TipoAvisoSistema tipo)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (tipo == TipoAvisoSistema.Entrada)
                return usuario.Apelido + " entrou no sistema.";

            if (tipo == TipoAvisoSistema.Saida)
                return usuario.Apelido + " saiu do sistema.";

            throw new ArgumentException(
                "Use Entrada ou Saida para avisos associados a usuario.");
        }

        public override string ToString()
        {
            return $"{DataHora:HH:mm} - [SISTEMA] {Conteudo}";
        }
    }
}