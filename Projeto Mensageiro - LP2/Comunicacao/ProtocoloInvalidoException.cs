using System;

namespace Projeto_Mensageiro___LP2.Comunicacao
{
    public class ProtocoloInvalidoException : Exception
    {
        public ProtocoloInvalidoException(string mensagem) : base(mensagem) { }
    }
}
