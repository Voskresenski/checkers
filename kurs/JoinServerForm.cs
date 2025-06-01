using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kurs
{
    public partial class JoinServerForm : Form
    {
        private TextBox txtIP;
        private Button btnConnect;
        private Button btnCancel;

        private const int Port = 9000;

        public JoinServerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtIP = new TextBox();
            this.btnConnect = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(20, 20);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(360, 27);
            this.txtIP.PlaceholderText = "Введите IP сервера";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(80, 70);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 30);
            this.btnConnect.Text = "Подключиться";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += BtnConnect_Click;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(220, 70);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += BtnCancel_Click;
            // 
            // JoinServerForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 120);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "JoinServerForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Присоединиться к серверу";
            this.FormClosing += JoinServerForm_FormClosing;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show("Введите корректный IP.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(ip, Port);

                // Успешно подключились, запускаем GameForm в режиме клиента
                this.Hide();
                var game = new GameForm(client, isServer: false);
                game.FormClosed += (s2, e2) => { this.Close(); new Form1().Show(); };
                game.Show();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                new Form1().Show();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            new Form1().Show();
        }

        private void JoinServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ничего особенного не делаем
        }
    }
}
