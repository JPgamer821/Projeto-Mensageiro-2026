
using System;
using System.Collections.Generic;
using System.Linq;
using Projeto_Mensageiro___LP2.Classes;

namespace Projeto_Mensageiro___LP2.Servidor
{
    internal class GerenciadorDeUsuarios
    {
        private List<Usuario> usuarios = new List<Usuario>();

        public void Cadastrar(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (Buscar(usuario.Apelido) != null)
                throw new InvalidOperationException(
                    "Este apelido já está cadastrado.");

            usuarios.Add(usuario);
        }

        public Usuario Buscar(string apelido)
        {
            return usuarios.FirstOrDefault(u =>
                u.Apelido.Equals(
                    apelido,
                    StringComparison.OrdinalIgnoreCase));
        }

        public Usuario ObterOuCadastrar(string apelido)
        {
            Usuario usuario = Buscar(apelido);

            if (usuario != null)
                return usuario;

            usuario = new Usuario(
                apelido,
                usuarios.Count + 1);

            usuarios.Add(usuario);

            return usuario;
        }

        public List<Usuario> Listar()
        {
            return new List<Usuario>(usuarios);
        }
    }
}
