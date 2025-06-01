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
            var serverForm = new CreateServerForm(); // ����� � ������� �������
            serverForm.Show();
        }

        private void btnJoinServer_Click(object sender, EventArgs e)
        {
            var connectForm = new JoinServerForm(); // ����� ����� IP � �����������
            connectForm.Show();
        }

        private void btnLocalPlay_Click(object sender, EventArgs e)
        {
            var gameForm = new GameForm();
            gameForm.Show();
            this.Hide(); // �������� ������� ����
        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
