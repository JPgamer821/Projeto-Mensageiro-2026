using System;

namespace Projeto_Mensageiro___LP2.Comunicacao
{
    public enum TipoComando
    {
        Login,
        Enviar,
        EnviarArquivo,
        Listar,
        Sair
    }

    public class ComandoProtocolo
    {
        public TipoComando Tipo { get; internal set; }
        public string Apelido { get; internal set; }
        public string Remetente { get; internal set; }
        public string Destinatario { get; internal set; }
        public DateTime? DataHora { get; internal set; }
        public string Texto { get; internal set; }
        public string NomeArquivo { get; internal set; }

        public byte[] DadosArquivo { get; internal set; }
    }
}