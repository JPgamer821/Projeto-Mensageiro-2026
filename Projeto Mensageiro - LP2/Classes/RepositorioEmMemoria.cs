
using System;
using System.Collections.Generic;
using Projeto_Mensageiro___LP2.Classes;

internal class RepositorioEmMemoria : IRepositorioDeMensagens
{
    private List<Mensagem> mensagens = new List<Mensagem>();

    public void Guardar(Mensagem m)
    {
        if (m == null)
            throw new ArgumentNullException(nameof(m));

        mensagens.Add(m);
    }

    public List<Mensagem> Listar()
    {
        return new List<Mensagem>(mensagens);
    }
}
