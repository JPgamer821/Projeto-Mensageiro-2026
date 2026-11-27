using System;
using System.Collections.Generic;

namespace Projeto_Mensageiro___LP2.Classes
{
    public class Usuario
    {
        public string Apelido { get; private set; }
        public int Id { get; private set; }
        public bool Online { get; private set; }
        public Pessoa Pessoa { get; private set; }
        public List<Usuario> Contatos { get; }

        public Usuario(string apelido, int id)
        {
            if (string.IsNullOrWhiteSpace(apelido))
                throw new ArgumentException("Informe um apelido.", nameof(apelido));
            if (apelido.Contains("|") || apelido.Contains("%") || apelido.Contains("\r") || apelido.Contains("\n"))
                throw new ArgumentException("O apelido contem caracteres reservados do protocolo.", nameof(apelido));

            Apelido = apelido.Trim();
            Id = id;
            Online = false;
            Contatos = new List<Usuario>();
        }

        public Usuario(string apelido) : this(apelido, 0) { }

        public void Entrar() { Online = true; }
        public void Sair() { Online = false; }

        public void AdicionarContato(Usuario contato)
        {
            if (contato == null) throw new ArgumentNullException(nameof(contato));
            if (Equals(contato)) throw new ArgumentException("Nao adicione voce mesmo como contato.");
            if (!Contatos.Contains(contato)) Contatos.Add(contato);
        }

        internal void AssociarPessoa(Pessoa pessoa)
        {
            if (pessoa == null) throw new ArgumentNullException(nameof(pessoa));
            if (Pessoa != null && !object.ReferenceEquals(Pessoa, pessoa))
                throw new InvalidOperationException("Este usuario ja esta associado a outra pessoa.");
            Pessoa = pessoa;
        }

        public override bool Equals(object obj)
        {
            Usuario outro = obj as Usuario;
            return outro != null && string.Equals(Apelido, outro.Apelido, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Apelido);
        }

        public override string ToString() { return Apelido; }
    }
}
