using System;
using System.Windows.Forms;

namespace kurs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateServer_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Открываем форму создания сервера
            var serverForm = new CreateServerForm();
            serverForm.Show();
        }

        private void btnJoinServer_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Открываем форму подключения к серверу
            var connectForm = new JoinServerForm();
            connectForm.Show();
        }

        private void btnLocalPlay_Click(object sender, EventArgs e)
        {
            // Открываем локальную игру
            var gameForm = new GameForm();
            gameForm.Show();
            this.Hide(); // Скрываем главное меню
            gameForm.FormClosed += (s, args) => this.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
