namespace kurs
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnCreateServer;
        private System.Windows.Forms.Button btnJoinServer;
        private System.Windows.Forms.Button btnLocalPlay;
        private System.Windows.Forms.Button btnExit;

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
            btnCreateServer = new Button();
            btnJoinServer = new Button();
            btnLocalPlay = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // btnCreateServer
            // 
            btnCreateServer.BackColor = Color.DarkRed;
            btnCreateServer.Font = new Font("Showcard Gothic", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            btnCreateServer.ForeColor = SystemColors.ButtonHighlight;
            btnCreateServer.Location = new Point(100, 50);
            btnCreateServer.Name = "btnCreateServer";
            btnCreateServer.Size = new Size(200, 40);
            btnCreateServer.TabIndex = 0;
            btnCreateServer.Text = "Создать сервер";
            btnCreateServer.UseVisualStyleBackColor = false;
            btnCreateServer.Click += btnCreateServer_Click;
            // 
            // btnJoinServer
            // 
            btnJoinServer.BackColor = Color.DarkRed;
            btnJoinServer.Font = new Font("Showcard Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnJoinServer.ForeColor = SystemColors.ButtonHighlight;
            btnJoinServer.Location = new Point(100, 113);
            btnJoinServer.Name = "btnJoinServer";
            btnJoinServer.Size = new Size(200, 51);
            btnJoinServer.TabIndex = 1;
            btnJoinServer.Text = "Присоединиться к серверу";
            btnJoinServer.UseVisualStyleBackColor = false;
            btnJoinServer.Click += btnJoinServer_Click;
            // 
            // btnLocalPlay
            // 
            btnLocalPlay.BackColor = Color.DarkRed;
            btnLocalPlay.Font = new Font("Showcard Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLocalPlay.ForeColor = SystemColors.ButtonHighlight;
            btnLocalPlay.Location = new Point(100, 192);
            btnLocalPlay.Name = "btnLocalPlay";
            btnLocalPlay.Size = new Size(200, 52);
            btnLocalPlay.TabIndex = 2;
            btnLocalPlay.Text = "Играть на одном компьютере";
            btnLocalPlay.UseVisualStyleBackColor = false;
            btnLocalPlay.Click += btnLocalPlay_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.DarkRed;
            btnExit.Font = new Font("Showcard Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExit.ForeColor = SystemColors.ButtonHighlight;
            btnExit.Location = new Point(100, 270);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(200, 40);
            btnExit.TabIndex = 3;
            btnExit.Text = "Выйти";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // Form1
            // 
            BackColor = Color.RosyBrown;
            ClientSize = new Size(400, 400);
            Controls.Add(btnCreateServer);
            Controls.Add(btnJoinServer);
            Controls.Add(btnLocalPlay);
            Controls.Add(btnExit);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Шашки – Главное меню";
            ResumeLayout(false);
        }
    }
}
