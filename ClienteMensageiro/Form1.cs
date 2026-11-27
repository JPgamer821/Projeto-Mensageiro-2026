
using System;
using System.IO;
using System.Windows.Forms;

namespace ClienteMensageiro
{
    public partial class FrmLogin : Form
    {
        private CanalArquivoCliente entrada;
        private CanalArquivoCliente saida;
        private Timer temporizador;

        public FrmLogin()
        {
            InitializeComponent();

            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado"
            );

            Directory.CreateDirectory(pasta);

            entrada = new CanalArquivoCliente(
                Path.Combine(pasta, "entrada.txt")
            );

            temporizador = new Timer();
            temporizador.Interval = 300;
            temporizador.Tick += Temporizador_Tick;
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            string apelido = txtApelido.Text.Trim();

            if (string.IsNullOrWhiteSpace(apelido))
            {
                MessageBox.Show("Digite seu apelido.");
                return;
            }

            string pasta = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MensageiroCompartilhado"
            );

            string caminhoSaida = Path.Combine(
                pasta, "saida_" + apelido + ".txt"
            );


            File.WriteAllText(caminhoSaida, "");

            saida = new CanalArquivoCliente(caminhoSaida);

            entrada.Enviar("LOGIN|" + apelido);

            btnEntrar.Enabled = false;
            temporizador.Start();
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            string resposta = saida.Receber();

            if (resposta == null)
                return;

            temporizador.Stop();

            btnEntrar.Enabled = true;

            if (resposta.StartsWith("OK|LOGIN|"))
            {
                string apelido = txtApelido.Text.Trim();

                FrmChat chat = new FrmChat(apelido);

                this.Hide();
                chat.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show(resposta, "Erro no login");
            }
        }
    }
}
