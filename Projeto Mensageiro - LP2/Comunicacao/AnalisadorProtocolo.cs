using System;
using System.Globalization;

namespace Projeto_Mensageiro___LP2.Comunicacao
{
    public static class AnalisadorProtocolo
    {
        private const string FormatoData = "o";

        public static ComandoProtocolo Analisar(string linha)
        {
            if (string.IsNullOrWhiteSpace(linha))
                throw new ProtocoloInvalidoException("A linha esta vazia.");

            string[] campos = linha.Split('|');
            string comando = campos[0].Trim().ToUpperInvariant();

            switch (comando)
            {
                case "LOGIN":
                    VerificarQuantidade(campos, 2);

                    return new ComandoProtocolo
                    {
                        Tipo = TipoComando.Login,
                        Apelido = ValidarApelido(campos[1])
                    };

                case "ENVIAR":
                    VerificarQuantidade(campos, 5);

                    DateTime data;

                    if (!DateTime.TryParseExact(
                        campos[3],
                        FormatoData,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out data))
                    {
                        throw new ProtocoloInvalidoException(
                            "Data e hora invalidas.");
                    }

                    string texto = DecodificarTexto(campos[4]);

                    if (string.IsNullOrWhiteSpace(texto))
                        throw new ProtocoloInvalidoException(
                            "O texto da mensagem esta vazio.");

                    return new ComandoProtocolo
                    {
                        Tipo = TipoComando.Enviar,
                        Remetente = ValidarApelido(campos[1]),
                        Destinatario = ValidarApelido(campos[2]),
                        DataHora = data,
                        Texto = texto
                    };

                case "ENVIARARQUIVO":
                    VerificarQuantidade(campos, 7);

                    DateTime dataArquivo;

                    if (!DateTime.TryParseExact(
                        campos[3],
                        FormatoData,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out dataArquivo))
                    {
                        throw new ProtocoloInvalidoException(
                            "Data e hora invalidas.");
                    }

                    string descricao = DecodificarTexto(campos[4]);
                    string nomeArquivo = DecodificarTexto(campos[5]);

                    if (string.IsNullOrWhiteSpace(nomeArquivo))
                        throw new ProtocoloInvalidoException(
                            "Nome do arquivo vazio.");

                    if (string.IsNullOrWhiteSpace(descricao))
                        throw new ProtocoloInvalidoException(
                            "Descricao do arquivo vazia.");

                    byte[] dadosArquivo;

                    try
                    {
                        dadosArquivo = Convert.FromBase64String(campos[6]);
                    }
                    catch (FormatException)
                    {
                        throw new ProtocoloInvalidoException(
                            "Conteudo do arquivo invalido.");
                    }

                    if (dadosArquivo.Length == 0)
                        throw new ProtocoloInvalidoException(
                            "O arquivo esta vazio.");

                    if (dadosArquivo.Length > 5 * 1024 * 1024)
                        throw new ProtocoloInvalidoException(
                            "O arquivo ultrapassa o limite de 5 MB.");

                    return new ComandoProtocolo
                    {
                        Tipo = TipoComando.EnviarArquivo,
                        Remetente = ValidarApelido(campos[1]),
                        Destinatario = ValidarApelido(campos[2]),
                        DataHora = dataArquivo,
                        Texto = descricao,
                        NomeArquivo = nomeArquivo,
                        DadosArquivo = dadosArquivo
                    };

                case "LISTAR":
                    if (campos.Length != 2 && campos.Length != 3)
                    {
                        throw new ProtocoloInvalidoException(
                            "Quantidade de campos invalida no comando LISTAR.");
                    }

                    return new ComandoProtocolo
                    {
                        Tipo = TipoComando.Listar,
                        Apelido = ValidarApelido(campos[1]),
                        Destinatario = campos.Length == 3
                            ? ValidarApelido(campos[2])
                            : null
                    };

                case "SAIR":
                    VerificarQuantidade(campos, 2);

                    return new ComandoProtocolo
                    {
                        Tipo = TipoComando.Sair,
                        Apelido = ValidarApelido(campos[1])
                    };

                default:
                    throw new ProtocoloInvalidoException(
                        "Comando desconhecido: " + comando);
            }
        }

        public static string CodificarTexto(string texto)
        {
            if (texto == null)
                throw new ArgumentNullException(nameof(texto));

            return texto.Replace("%", "%25")
                        .Replace("|", "%7C")
                        .Replace("\r", "%0D")
                        .Replace("\n", "%0A");
        }

        public static string DecodificarTexto(string texto)
        {
            if (texto == null)
                throw new ArgumentNullException(nameof(texto));

            System.Text.StringBuilder saida =
                new System.Text.StringBuilder();

            for (int i = 0; i < texto.Length; i++)
            {
                if (texto[i] != '%')
                {
                    saida.Append(texto[i]);
                    continue;
                }

                if (i + 2 >= texto.Length)
                    throw new ProtocoloInvalidoException(
                        "Escape incompleto no texto.");

                string codigo = texto.Substring(i + 1, 2)
                    .ToUpperInvariant();

                switch (codigo)
                {
                    case "25": saida.Append('%'); break;
                    case "7C": saida.Append('|'); break;
                    case "0D": saida.Append('\r'); break;
                    case "0A": saida.Append('\n'); break;

                    default:
                        throw new ProtocoloInvalidoException(
                            "Escape desconhecido: %" + codigo);
                }

                i += 2;
            }

            return saida.ToString();
        }

        private static string ValidarApelido(string apelido)
        {
            if (string.IsNullOrWhiteSpace(apelido))
                throw new ProtocoloInvalidoException("Apelido vazio.");

            if (apelido.Trim() != apelido ||
                apelido.Contains("%") ||
                apelido.Contains("|") ||
                apelido.Contains("\r") ||
                apelido.Contains("\n"))
            {
                throw new ProtocoloInvalidoException(
                    "Apelido invalido.");
            }

            return apelido;
        }

        private static void VerificarQuantidade(
            string[] campos, int esperada)
        {
            if (campos.Length != esperada)
            {
                throw new ProtocoloInvalidoException(
                    "Quantidade de campos invalida. Esperados: "
                    + esperada + ".");
            }
        }
    }
}