using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ComunicacaoCompartilhada;

namespace ClienteMensageiro
{
    public partial class FrmChat : Form
    {
        private const int LimiteArquivo = 5 * 1024 * 1024;

        private string apelido;

        private CanalTcpCliente canalTcp;

        private Timer temporizador;

        private bool aguardandoLista = false;
        private bool atualizacaoPendente = false;

        private DateTime inicioSolicitacao;

        private string contatoAguardando;

        private Dictionary<string, RichTextBox> conversas =
            new Dictionary<string, RichTextBox>(
                StringComparer.OrdinalIgnoreCase);

        public FrmChat(string apelido)
        {
            InitializeComponent();

            this.apelido = apelido;

            canalTcp = new CanalTcpCliente("127.0.0.1", 5000);

            temporizador = new Timer();
            temporizador.Interval = 500;
            temporizador.Tick += Temporizador_Tick;

            tabConversas.SelectedIndexChanged +=
                TabConversas_SelectedIndexChanged;

            this.Text = "Mensageiro - " + apelido;

            CarregarContatos();

            temporizador.Start();
        }

        private string ObterCaminhoContatos()
        {
            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado"
            );

            Directory.CreateDirectory(pasta);

            string identificador = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(apelido)
            );

            identificador = identificador
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");

            return Path.Combine(
                pasta,
                "contatos_" + identificador + ".txt"
            );
        }

        private void SalvarContatos()
        {
            string caminho = ObterCaminhoContatos();

            List<string> contatos = new List<string>(
                conversas.Keys
            );

            File.WriteAllLines(caminho, contatos);
        }

        private void CarregarContatos()
        {
            string caminho = ObterCaminhoContatos();

            if (!File.Exists(caminho))
                return;

            string[] contatos = File.ReadAllLines(caminho);

            foreach (string contato in contatos)
            {
                if (!string.IsNullOrWhiteSpace(contato))
                {
                    AdicionarConversa(contato.Trim());
                }
            }
        }

        private string ObterContatoSelecionado()
        {
            if (tabConversas.SelectedTab == null)
                return null;

            return tabConversas.SelectedTab.Tag as string;
        }


        private RichTextBox ObterCampoMensagens()
        {
            string contato = ObterContatoSelecionado();

            if (contato == null)
                return null;

            if (conversas.ContainsKey(contato))
                return conversas[contato];

            return null;
        }


        private void btnNovaConversa_Click(object sender, EventArgs e)
        {
            using (Form janela = new Form())
            {
                janela.Text = "Nova conversa";
                janela.StartPosition = FormStartPosition.CenterParent;
                janela.FormBorderStyle = FormBorderStyle.FixedDialog;
                janela.ClientSize = new System.Drawing.Size(320, 125);
                janela.MaximizeBox = false;
                janela.MinimizeBox = false;

                Label lbl = new Label();
                lbl.Text = "Digite o apelido do contato:";
                lbl.Location = new System.Drawing.Point(15, 15);
                lbl.AutoSize = true;

                TextBox txt = new TextBox();
                txt.Location = new System.Drawing.Point(18, 42);
                txt.Size = new System.Drawing.Size(280, 25);

                Button btnAdicionar = new Button();
                btnAdicionar.Text = "Adicionar";
                btnAdicionar.Location = new System.Drawing.Point(125, 80);
                btnAdicionar.Size = new System.Drawing.Size(85, 30);
                btnAdicionar.DialogResult = DialogResult.OK;

                Button btnCancelar = new Button();
                btnCancelar.Text = "Cancelar";
                btnCancelar.Location = new System.Drawing.Point(215, 80);
                btnCancelar.Size = new System.Drawing.Size(85, 30);
                btnCancelar.DialogResult = DialogResult.Cancel;

                janela.Controls.Add(lbl);
                janela.Controls.Add(txt);
                janela.Controls.Add(btnAdicionar);
                janela.Controls.Add(btnCancelar);

                janela.AcceptButton = btnAdicionar;
                janela.CancelButton = btnCancelar;

                txt.Focus();

                if (janela.ShowDialog(this) != DialogResult.OK)
                    return;

                string contato = txt.Text.Trim();

                if (string.IsNullOrWhiteSpace(contato))
                {
                    MessageBox.Show("Informe o apelido do contato.");
                    return;
                }

                if (contato.Equals(
                    apelido,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Você não pode iniciar uma conversa consigo mesmo."
                    );
                    return;
                }

                AdicionarConversa(contato);
            }
        }


        private void AdicionarConversa(string contato)
        {
            if (conversas.ContainsKey(contato))
            {
                SelecionarConversa(contato);
                return;
            }

            TabPage pagina = new TabPage(contato);
            pagina.Tag = contato;

            RichTextBox mensagens = new RichTextBox();

            mensagens.Dock = DockStyle.Fill;
            mensagens.ReadOnly = true;
            mensagens.DetectUrls = true;
            mensagens.BackColor = System.Drawing.Color.White;
            mensagens.Font = new System.Drawing.Font(
                "Segoe UI", 10F);

            mensagens.LinkClicked += RtbMensagens_LinkClicked;

            pagina.Controls.Add(mensagens);

            conversas.Add(contato, mensagens);

            tabConversas.TabPages.Add(pagina);
            tabConversas.SelectedTab = pagina;

            SalvarContatos();

            SolicitarLista();
        }


        private void SelecionarConversa(string contato)
        {
            foreach (TabPage pagina in tabConversas.TabPages)
            {
                if (string.Equals(
                    pagina.Tag as string,
                    contato,
                    StringComparison.OrdinalIgnoreCase))
                {
                    tabConversas.SelectedTab = pagina;
                    return;
                }
            }
        }

        private void SolicitarLista()
        {
            string contato = ObterContatoSelecionado();

            if (contato == null)
                return;

            if (aguardandoLista)
            {
                atualizacaoPendente = true;
                return;
            }

            contatoAguardando = contato;

            canalTcp.Enviar(
                "LISTAR|" + apelido + "|" + contato
            );

            aguardandoLista = true;
            inicioSolicitacao = DateTime.Now;
        }

        private void AtualizarMensagens(
            string contato,
            string conteudo)
        {
            if (!conversas.ContainsKey(contato))
                return;

            RichTextBox mensagens = conversas[contato];

            conteudo = Regex.Replace(
                conteudo,
                @"arquivo://(\S+)",
                "https://arquivo.local/$1",
                RegexOptions.IgnoreCase
            );

            mensagens.Text = conteudo;

            mensagens.SelectionStart =
                mensagens.Text.Length;

            mensagens.SelectionLength = 0;

            mensagens.ScrollToCaret();
        }

        private void TabConversas_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            SolicitarLista();
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            string destinatario = ObterContatoSelecionado();
            string texto = txtMensagem.Text;

            if (destinatario == null)
            {
                MessageBox.Show("Adicione ou selecione uma conversa.");
                return;
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                MessageBox.Show("Digite uma mensagem.");
                return;
            }

            string dataHora = DateTime.Now.ToString("o");

            string textoCodificado = CodificarTexto(texto);

            string comando =
                "ENVIAR|" + apelido + "|" +
                destinatario + "|" +
                dataHora + "|" +
                textoCodificado;

            canalTcp.Enviar(comando);

            txtMensagem.Clear();
            txtMensagem.Focus();
        }

        private void btnAnexar_Click(object sender, EventArgs e)
        {
            string destinatario = ObterContatoSelecionado();

            if (destinatario == null)
            {
                MessageBox.Show("Adicione ou selecione uma conversa.");
                return;
            }

            using (OpenFileDialog seletor = new OpenFileDialog())
            {
                seletor.Title = "Selecionar arquivo";
                seletor.CheckFileExists = true;
                seletor.Multiselect = false;

                if (seletor.ShowDialog() != DialogResult.OK)
                    return;

                FileInfo informacoes =
                    new FileInfo(seletor.FileName);

                if (informacoes.Length > LimiteArquivo)
                {
                    MessageBox.Show(
                        "O arquivo ultrapassa o limite de 5 MB.",
                        "Arquivo muito grande",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                try
                {
                    byte[] dados =
                        File.ReadAllBytes(seletor.FileName);

                    string conteudoBase64 =
                        Convert.ToBase64String(dados);

                    string nomeArquivo =
                        Path.GetFileName(seletor.FileName);

                    string descricao = "Arquivo enviado";

                    string comando =
                        "ENVIARARQUIVO|" +
                        apelido + "|" +
                        destinatario + "|" +
                        DateTime.Now.ToString("o") + "|" +
                        CodificarTexto(descricao) + "|" +
                        CodificarTexto(nomeArquivo) + "|" +
                        conteudoBase64;

                    canalTcp.Enviar(comando);

                    MessageBox.Show(
                        "Arquivo enviado para processamento pelo servidor.",
                        "Anexo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception erro)
                {
                    MessageBox.Show(
                        "Não foi possível enviar o arquivo:\n" +
                        erro.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void RtbMensagens_LinkClicked(
            object sender,
            LinkClickedEventArgs e)
        {
            const string prefixo = "https://arquivo.local/";

            if (!e.LinkText.StartsWith(
                prefixo,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string identificador =
                e.LinkText.Substring(prefixo.Length);

            AbrirAnexo(identificador);
        }

        private void AbrirAnexo(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador) ||
                Path.GetFileName(identificador) != identificador ||
                identificador.IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(
                    "Identificador de arquivo inválido."
                );
                return;
            }

            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado",
                "Arquivos"
            );

            string caminho = Path.Combine(
                pasta,
                identificador
            );

            if (!File.Exists(caminho))
            {
                MessageBox.Show(
                    "O arquivo não foi encontrado na pasta compartilhada."
                );
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminho,
                    UseShellExecute = true
                });
            }
            catch (Exception erro)
            {
                MessageBox.Show(
                    "Não foi possível abrir o arquivo:\n" +
                    erro.Message
                );
            }
        }

        private string CodificarTexto(string texto)
        {
            return texto.Replace("%", "%25")
                        .Replace("|", "%7C")
                        .Replace("\r", "%0D")
                        .Replace("\n", "%0A");
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            try
            {
                string resposta = canalTcp.Receber();

                if (resposta != null)
                {
                    if (resposta.StartsWith("NOVA_MENSAGEM"))
                    {
                        if (aguardandoLista)
                            atualizacaoPendente = true;
                        else
                            SolicitarLista();
                    }
                    else if (resposta.StartsWith("OK|ENVIAR|") ||
                             resposta.StartsWith("OK|ENVIARARQUIVO|"))
                    {
                        if (aguardandoLista)
                            atualizacaoPendente = true;
                        else
                            SolicitarLista();
                    }
                    else if (resposta.StartsWith("OK|LISTAR|"))
                    {
                        string contatoRespondido = contatoAguardando;

                        string conteudo = resposta.Substring(
                            "OK|LISTAR|".Length
                        );

                        conteudo = conteudo.Replace(
                            "\\n",
                            Environment.NewLine
                        );

                        aguardandoLista = false;
                        contatoAguardando = null;

                        if (contatoRespondido != null)
                        {
                            AtualizarMensagens(
                                contatoRespondido,
                                conteudo
                            );
                        }

                        bool precisaAtualizar =
                            atualizacaoPendente ||
                            !string.Equals(
                                contatoRespondido,
                                ObterContatoSelecionado(),
                                StringComparison.OrdinalIgnoreCase
                            );

                        atualizacaoPendente = false;

                        if (precisaAtualizar)
                            SolicitarLista();
                    }
                    else if (resposta.StartsWith("ERRO|"))
                    {
                        aguardandoLista = false;
                        contatoAguardando = null;

                        MessageBox.Show(
                            resposta,
                            "Erro no mensageiro"
                        );

                        if (atualizacaoPendente)
                        {
                            atualizacaoPendente = false;
                            SolicitarLista();
                        }
                    }
                }

                if (aguardandoLista &&
                    (DateTime.Now - inicioSolicitacao).TotalSeconds > 5)
                {
                    aguardandoLista = false;
                    contatoAguardando = null;
                    SolicitarLista();
                }
            }
            catch (Exception erro)
            {
                temporizador.Stop();

                MessageBox.Show(
                    "Erro na comunicação:\n" + erro.Message
                );
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            temporizador.Stop();

            try
            {
                canalTcp.Enviar("SAIR|" + apelido);
            }
            catch
            {

            }

            canalTcp.Dispose();

            this.Close();
        }
    }
}