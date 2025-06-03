using System;
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
                MessageBox.Show("Введите корректный IP.",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            TcpClient client = null;
            try
            {
                client = new TcpClient();
                // Попытка подключения с ожиданием до 5 секунд
                var connectTask = client.ConnectAsync(ip, Port);
                var timeout = Task.Delay(5000);

                var completed = await Task.WhenAny(connectTask, timeout);
                if (completed == timeout)
                {
                    // Таймаут
                    client.Close();
                    MessageBox.Show("Не удалось подключиться к серверу: таймаут.",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                // Если подключились успешно:
                this.Hide();
                var game = new GameForm(client, isServer: false);
                game.FormClosed += (s2, e2) =>
                {
                    this.Close();
                    new Form1().Show();
                };
                game.Show();
            }
            catch (Exception ex)
            {
                // Ловим любую ошибку сокета
                client?.Close();
                MessageBox.Show($"Не удалось подключиться к серверу:\n{ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                // Закрываем форму и возвращаемся в главное меню
                this.Close();
                new Form1().Show();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Закрываем форму и возвращаемся в главное меню
            this.Close();
            new Form1().Show();
        }

        private void JoinServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Здесь ничего не делаем (без ошибок)
        }
    }
}
