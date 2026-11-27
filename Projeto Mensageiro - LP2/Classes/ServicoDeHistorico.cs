
using System;
using System.Collections.Generic;

namespace Projeto_Mensageiro___LP2.Classes
{
    internal class ServicoDeHistorico
    {
        private IRepositorioDeMensagens repositorio;

        public ServicoDeHistorico(IRepositorioDeMensagens repositorio)
        {
            this.repositorio = repositorio;
        }

        public void Guardar(Mensagem mensagem)
        {
            repositorio.Guardar(mensagem);
        }

        public List<Mensagem> Listar()
        {
            return repositorio.Listar();
        }

        public int Resumo()
        {
            return repositorio.Listar().Count;
        }
    }
}
