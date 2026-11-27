using System;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class Pessoa
    {
        public string Nome { get; private set; }
        public Usuario Usuario { get; private set; }

        public Pessoa(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Informe o nome da pessoa.", nameof(nome));
            Nome = nome.Trim();
        }

        public void AssociarUsuario(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (Usuario != null && !Usuario.Equals(usuario))
                throw new InvalidOperationException("Esta pessoa ja possui um usuario associado.");
            usuario.AssociarPessoa(this);
            Usuario = usuario;
        }
    }
}
