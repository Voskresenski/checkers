using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kurs
{
    public partial class CreateServerForm : Form
    {
        private TcpListener listener;
        private const int Port = 9000;

        private Label lblInfo;
        private Button btnCancel;
        private bool isListening = false;

        public CreateServerForm()
        {
            InitializeComponent();
            // Запускаем асинхронное ожидание подключений
            StartListening();
        }

        private void InitializeComponent()
        {
            this.lblInfo = new Label();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(20, 20);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(360, 60);
            this.lblInfo.Text = "Инициализация сервера...";
            this.lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(140, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += BtnCancel_Click;
            // 
            // CreateServerForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 160);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateServerForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Создать сервер";
            this.FormClosing += CreateServerForm_FormClosing;
            this.ResumeLayout(false);
        }

        private async void StartListening()
        {
            try
            {
                // Определяем локальный IP (IPv4)
                string localIP = "Не удалось определить IP";
                foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ni.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ni.ToString();
                        break;
                    }
                }

                lblInfo.Text = $"Сервер запущен.\nIP: {localIP}\nПорт: {Port}\nОжидание соперника...";
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Start();
                isListening = true;

                TcpClient client = null;
                try
                {
                    // Ждём подключение клиента (асинхронно)
                    client = await listener.AcceptTcpClientAsync();
                }
                catch
                {
                    // Ловим ситуацию, когда listener.Stop() был вызван
                    return;
                }

                if (client != null)
                {
                    // Когда подключились — запускаем сетевую игру (сервер)
                    this.Hide();
                    var game = new GameForm(client, isServer: true);
                    game.FormClosed += (s, e) =>
                    {
                        this.Close();
                        new Form1().Show();
                    };
                    game.Show();
                }
            }
            catch (Exception ex)
            {
                // Если не удалось запустить listener
                MessageBox.Show($"Ошибка при запуске сервера:\n{ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                this.Close();
                new Form1().Show();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Останавливаем listener, если он работает
            try
            {
                isListening = false;
                listener?.Stop();
            }
            catch
            {
                // ничего — общий try/catch на случай, если listener уже был остановлен
            }

            // Закрываем форму и возвращаемся в главное меню
            this.Close();
            new Form1().Show();
        }

        private void CreateServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Гарантированно остановим listener
            try
            {
                isListening = false;
                listener?.Stop();
            }
            catch
            {
                // игнорируем любые ошибки здесь
            }
        }
    }
}
