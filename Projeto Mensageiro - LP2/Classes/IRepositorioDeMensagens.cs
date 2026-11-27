using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_Mensageiro___LP2.Classes
{
    internal interface IRepositorioDeMensagens
    {
        void Guardar(Mensagem m);
        List<Mensagem> Listar();
    }
}
