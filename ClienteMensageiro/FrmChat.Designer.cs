namespace ClienteMensageiro
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabConversas = new System.Windows.Forms.TabControl();
            this.btnNovaConversa = new System.Windows.Forms.Button();
            this.txtMensagem = new System.Windows.Forms.TextBox();
            this.btnEnviar = new System.Windows.Forms.Button();
            this.btnAnexar = new System.Windows.Forms.Button();
            this.btnSair = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tabConversas
            // 
            this.tabConversas.Location = new System.Drawing.Point(22, 71);
            this.tabConversas.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabConversas.Name = "tabConversas";
            this.tabConversas.SelectedIndex = 0;
            this.tabConversas.Size = new System.Drawing.Size(968, 471);
            this.tabConversas.TabIndex = 1;
            // 
            // btnNovaConversa
            // 
            this.btnNovaConversa.Location = new System.Drawing.Point(22, 19);
            this.btnNovaConversa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnNovaConversa.Name = "btnNovaConversa";
            this.btnNovaConversa.Size = new System.Drawing.Size(163, 40);
            this.btnNovaConversa.TabIndex = 0;
            this.btnNovaConversa.Text = "+ Nova conversa";
            this.btnNovaConversa.UseVisualStyleBackColor = true;
            this.btnNovaConversa.Click += new System.EventHandler(this.btnNovaConversa_Click);
            // 
            // txtMensagem
            // 
            this.txtMensagem.Location = new System.Drawing.Point(22, 557);
            this.txtMensagem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMensagem.Name = "txtMensagem";
            this.txtMensagem.Size = new System.Drawing.Size(674, 26);
            this.txtMensagem.TabIndex = 2;
            // 
            // btnEnviar
            // 
            this.btnEnviar.Location = new System.Drawing.Point(889, 550);
            this.btnEnviar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(101, 40);
            this.btnEnviar.TabIndex = 4;
            this.btnEnviar.Text = "ENVIAR";
            this.btnEnviar.UseVisualStyleBackColor = true;
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
            // 
            // btnAnexar
            // 
            this.btnAnexar.Location = new System.Drawing.Point(702, 550);
            this.btnAnexar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAnexar.Name = "btnAnexar";
            this.btnAnexar.Size = new System.Drawing.Size(169, 40);
            this.btnAnexar.TabIndex = 3;
            this.btnAnexar.Text = "ANEXAR";
            this.btnAnexar.UseVisualStyleBackColor = true;
            this.btnAnexar.Click += new System.EventHandler(this.btnAnexar_Click);
            // 
            // btnSair
            // 
            this.btnSair.Location = new System.Drawing.Point(889, 19);
            this.btnSair.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(101, 40);
            this.btnSair.TabIndex = 5;
            this.btnSair.Text = "SAIR";
            this.btnSair.UseVisualStyleBackColor = true;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // FrmChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 596);
            this.Controls.Add(this.btnSair);
            this.Controls.Add(this.btnAnexar);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.txtMensagem);
            this.Controls.Add(this.tabConversas);
            this.Controls.Add(this.btnNovaConversa);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmChat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mensageiro";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TabControl tabConversas;
        private System.Windows.Forms.Button btnNovaConversa;
        private System.Windows.Forms.TextBox txtMensagem;
        private System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.Button btnAnexar;
        private System.Windows.Forms.Button btnSair;
    }
}