
namespace ComunicacaoCompartilhada
{
    public interface ICanalDeComunicacao
    {
        void Enviar(string mensagem);

        string Receber();
    }
}
