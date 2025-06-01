using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kurs
{
    public partial class GameForm : Form
    {
        private const int BoardSize = 8;
        private Panel[,] boardPanels = new Panel[BoardSize, BoardSize];
        private CheckerPiece selectedPiece = null;
        private Panel selectedPanel = null;
        private string currentPlayer = "white";
        private bool mustContinueCapture = false;

        private readonly Color HighlightColor = Color.LightGreen;
        private readonly Color CaptureHighlightColor = Color.OrangeRed;
        private readonly Color DefaultCellColor1 = Color.BurlyWood;
        private readonly Color DefaultCellColor2 = Color.SaddleBrown;

        // Сетевой режим
        private TcpClient netClient = null;
        private NetworkStream netStream = null;
        private bool isServer = false;
        private bool myTurn = false;
        private string myColor = null;

        // UI
        private Label lblStatus;
        private Button btnSurrender;
        private Button btnDraw;
        private Button btnMenu; // Кнопка «В главное меню» (только для локального режима)

        #region Конструкторы

        public GameForm()
        {
            InitializeComponent();
            InitializeBoard();
            AddControlButtons(localMode: true);
            lblStatus.Text = "Локальный режим: ходят по очереди, белые начинают.";
        }

        public GameForm(TcpClient client, bool isServer)
        {
            InitializeComponent();
            InitializeBoard();
            AddControlButtons(localMode: false);

            this.netClient = client;
            this.netStream = client.GetStream();
            this.isServer = isServer;

            myColor = isServer ? "white" : "black";
            currentPlayer = "white";
            myTurn = (myColor == "white");

            UpdateStatusLabel();
            Task.Run(() => ListenForMessages());
        }

        #endregion

        #region Инициализация компонентов и доски

        private void InitializeComponent()
        {
            this.lblStatus = new Label();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new Point(50, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(500, 20);
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            this.lblStatus.Font = new Font("Arial", 10, FontStyle.Bold);
            // 
            // GameForm
            // 
            this.ClientSize = new Size(600, 650);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Шашки";
            this.ResumeLayout(false);
        }

        private void InitializeBoard()
        {
            int offsetX = 50, offsetY = 40;
            int cellSize = 60;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Panel cell = new Panel
                    {
                        Size = new Size(cellSize, cellSize),
                        Location = new Point(offsetX + col * cellSize, offsetY + row * cellSize),
                        BackColor = (row + col) % 2 == 0 ? DefaultCellColor1 : DefaultCellColor2,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = new Point(row, col)
                    };
                    cell.Click += Cell_Click;
                    this.Controls.Add(cell);
                    boardPanels[row, col] = cell;

                    if ((row + col) % 2 != 0 && row < 3)
                        AddPiece(cell, "black");
                    else if ((row + col) % 2 != 0 && row > 4)
                        AddPiece(cell, "white");
                }
            }
        }

        private void AddPiece(Panel cell, string color)
        {
            var piece = new CheckerPiece(color);
            piece.Location = new Point((cell.Width - piece.Width) / 2,
                                       (cell.Height - piece.Height) / 2);
            cell.Controls.Add(piece);
        }

        /// <summary>
        /// Добавляет кнопки «Сдаться» и «Предложить ничью».
        /// Если localMode == true, добавляем ещё и «В главное меню».
        /// </summary>
        private void AddControlButtons(bool localMode)
        {
            // Кнопка "Сдаться"
            btnSurrender = new Button
            {
                Text = "Сдаться",
                Location = new Point(50, 520),
                Size = new Size(100, 30)
            };
            btnSurrender.Click += BtnSurrender_Click;
            this.Controls.Add(btnSurrender);

            // Кнопка "Предложить ничью"
            btnDraw = new Button
            {
                Text = "Предложить ничью",
                Location = new Point(170, 520),
                Size = new Size(150, 30)
            };
            btnDraw.Click += BtnDraw_Click;
            this.Controls.Add(btnDraw);

            if (localMode)
            {
                // Кнопка "В главное меню" только для локального режима
                btnMenu = new Button
                {
                    Text = "В главное меню",
                    Location = new Point(350, 520),
                    Size = new Size(150, 30)
                };
                btnMenu.Click += (s, e) =>
                {
                    this.Close();
                    new Form1().Show();
                };
                this.Controls.Add(btnMenu);
            }
        }

        #endregion

        #region Обработка клика по клетке

        private void Cell_Click(object sender, EventArgs e)
        {
            if (netClient != null && !myTurn)
                return;

            Panel targetCell = sender as Panel;
            if (targetCell == null) return;

            if (mustContinueCapture)
            {
                if (selectedPiece == null || selectedPanel == null) return;
                Panel capturedPanel;
                if (!IsValidMove(selectedPanel, targetCell, out capturedPanel)) return;
                PerformMove(targetCell, capturedPanel);
                return;
            }

            if (selectedPiece != null && selectedPanel != null)
            {
                Panel capPanel;
                if (IsValidMove(selectedPanel, targetCell, out capPanel))
                {
                    PerformMove(targetCell, capPanel);
                    return;
                }
            }

            if (targetCell.Controls.Count > 0)
            {
                var piece = targetCell.Controls[0] as CheckerPiece;

                if (netClient != null)
                {
                    if (!myTurn) return;
                    if ((string)piece.Tag != myColor) return;
                }
                else
                {
                    if ((string)piece.Tag != currentPlayer) return;
                }

                string checkColor = (netClient != null) ? myColor : currentPlayer;
                if (HasAnyCapture(checkColor) && !HasCaptureMoves(targetCell))
                    return;

                selectedPiece?.ToggleSelect();
                selectedPiece = piece;
                selectedPiece.ToggleSelect();
                selectedPanel = targetCell;

                HighlightValidMoves(targetCell);
            }
        }

        #endregion

        #region Выполнение хода и отправка по сети

        private void PerformMove(Panel target, Panel captured)
        {
            var fromPanel = selectedPanel;
            MoveSelectedPieceTo(target, captured);

            if (netClient != null)
            {
                SendMessage($"MOVE:{((Point)fromPanel.Tag).X},{((Point)fromPanel.Tag).Y}:" +
                            $"{((Point)target.Tag).X},{((Point)target.Tag).Y}");
                if (!mustContinueCapture)
                    myTurn = false;
                UpdateStatusLabel();
            }
        }

        private void MoveSelectedPieceTo(Panel target, Panel captured)
        {
            Point end = (Point)target.Tag;
            bool isCapture = (captured != null);

            target.Controls.Add(selectedPiece);
            selectedPiece.Location = new Point((target.Width - selectedPiece.Width) / 2,
                                               (target.Height - selectedPiece.Height) / 2);
            selectedPanel.Controls.Clear();

            if (isCapture)
                captured.Controls.Clear();

            if ((string)selectedPiece.Tag == "white" && end.X == 0) selectedPiece.IsKing = true;
            if ((string)selectedPiece.Tag == "black" && end.X == BoardSize - 1) selectedPiece.IsKing = true;
            selectedPiece.Invalidate();

            selectedPanel = target;
            ClearHighlights();

            if (isCapture && HasCaptureMoves(selectedPanel))
            {
                mustContinueCapture = true;
                HighlightValidMoves(selectedPanel);
                return;
            }

            mustContinueCapture = false;
            selectedPiece.ToggleSelect();
            selectedPiece = null;
            selectedPanel = null;

            currentPlayer = (currentPlayer == "white") ? "black" : "white";
            UpdateStatusLabel();
            CheckGameEnd();
        }

        private void SendMessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                netStream.Write(data, 0, data.Length);
            }
            catch
            {
                MessageBox.Show("Ошибка сети: связь потеряна.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Invoke((Action)(() =>
                {
                    this.Close();
                    new Form1().Show();
                }));
            }
        }

        private async void ListenForMessages()
        {
            var buffer = new byte[256];
            var sb = new StringBuilder();

            try
            {
                while (true)
                {
                    int bytesRead = await netStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) break;

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    while (true)
                    {
                        string str = sb.ToString();
                        int idx = str.IndexOf('\n');
                        if (idx < 0) break;

                        string line = str.Substring(0, idx).Trim();
                        sb.Remove(0, idx + 1);

                        ProcessIncoming(line);
                    }
                }
            }
            catch
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("Связь потеряна.", "Сетевая игра",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    new Form1().Show();
                }));
            }
        }

        private void ProcessIncoming(string line)
        {
            // MOVE:x1,y1:x2,y2
            if (line.StartsWith("MOVE:"))
            {
                var parts = line.Substring(5).Split(':');
                var src = parts[0].Split(',');
                var dst = parts[1].Split(',');
                int r1 = int.Parse(src[0]), c1 = int.Parse(src[1]);
                int r2 = int.Parse(dst[0]), c2 = int.Parse(dst[1]);

                var fromPanel = boardPanels[r1, c1];
                var toPanel = boardPanels[r2, c2];

                Panel capPanel = null;
                int dx = r2 - r1, dy = c2 - c1;
                if (Math.Abs(dx) >= 2 && Math.Abs(dy) >= 2)
                {
                    int mx = (r1 + r2) / 2, my = (c1 + c2) / 2;
                    capPanel = boardPanels[mx, my];
                }

                this.Invoke((Action)(() =>
                {
                    selectedPiece = fromPanel.Controls[0] as CheckerPiece;
                    selectedPanel = fromPanel;
                    MoveSelectedPieceFromNetwork(toPanel, capPanel);

                    myTurn = true;
                    UpdateStatusLabel();
                }));
            }
            // SURRENDER
            else if (line == "SURRENDER")
            {
                this.Invoke((Action)(() =>
                {
                    ShowEndGame("Соперник сдался. Вы выиграли!");
                }));
            }
            // DRAW_REQUEST
            else if (line == "DRAW_REQUEST")
            {
                this.Invoke((Action)(() =>
                {
                    var result = MessageBox.Show("Соперник предложил ничью. Принять?",
                        "Предложение ничьей", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        SendMessage("DRAW_ACCEPT");
                        ShowEndGame("Ничья по согласию!");
                    }
                }));
            }
            // DRAW_ACCEPT
            else if (line == "DRAW_ACCEPT")
            {
                this.Invoke((Action)(() =>
                {
                    ShowEndGame("Ничья по согласию!");
                }));
            }
        }

        private void MoveSelectedPieceFromNetwork(Panel target, Panel captured)
        {
            Point end = (Point)target.Tag;
            bool isCapture = (captured != null);

            target.Controls.Add(selectedPiece);
            selectedPiece.Location = new Point((target.Width - selectedPiece.Width) / 2,
                                               (target.Height - selectedPiece.Height) / 2);
            selectedPanel.Controls.Clear();

            if (isCapture) captured.Controls.Clear();

            if ((string)selectedPiece.Tag == "white" && end.X == 0) selectedPiece.IsKing = true;
            if ((string)selectedPiece.Tag == "black" && end.X == BoardSize - 1) selectedPiece.IsKing = true;
            selectedPiece.Invalidate();

            selectedPanel = target;
            ClearHighlights();

            if (isCapture && HasCaptureMoves(selectedPanel))
            {
                mustContinueCapture = true;
                HighlightValidMoves(selectedPanel);
                return;
            }

            mustContinueCapture = false;
            selectedPiece = null;
            selectedPanel = null;

            currentPlayer = (currentPlayer == "white") ? "black" : "white";
            UpdateStatusLabel();
            CheckGameEnd();
        }

        #endregion

        #region Проверка ходов и захватов

        private bool IsValidMove(Panel from, Panel to, out Panel capturedPanel)
        {
            capturedPanel = null;
            if (to.BackColor != HighlightColor && to.BackColor != CaptureHighlightColor)
                return false;

            Point start = (Point)from.Tag;
            Point end = (Point)to.Tag;
            int dx = end.X - start.X;
            int dy = end.Y - start.Y;
            string color = (string)selectedPiece.Tag;

            // Дамка
            if (selectedPiece.IsKing)
            {
                if (Math.Abs(dx) != Math.Abs(dy)) return false;

                int stepX = dx / Math.Abs(dx);
                int stepY = dy / Math.Abs(dy);
                int x = start.X + stepX, y = start.Y + stepY;
                bool foundEnemy = false;
                Panel enemyCell = null;

                while (x != end.X && y != end.Y)
                {
                    var cell = boardPanels[x, y];
                    if (cell.Controls.Count > 0)
                    {
                        var mid = cell.Controls[0] as CheckerPiece;
                        if ((string)mid.Tag != color && !foundEnemy)
                        {
                            foundEnemy = true;
                            enemyCell = cell;
                        }
                        else return false;
                    }
                    x += stepX; y += stepY;
                }

                if (foundEnemy) capturedPanel = enemyCell;
                return true;
            }

            // Обычная шашка: взятие
            bool captureMove = Math.Abs(dx) == 2 && Math.Abs(dy) == 2;
            if (captureMove)
            {
                int mx = (start.X + end.X) / 2;
                int my = (start.Y + end.Y) / 2;
                var midCell = boardPanels[mx, my];
                if (midCell.Controls.Count > 0)
                {
                    var midPiece = midCell.Controls[0] as CheckerPiece;
                    if ((string)midPiece.Tag != color)
                        capturedPanel = midCell;
                    else return false;
                }
                else return false;

                return true;
            }

            int dir = (color == "white") ? -1 : 1;
            bool simpleMove = dx == dir && Math.Abs(dy) == 1;
            if (simpleMove && !HasAnyCapture(color))
                return true;

            return false;
        }

        private void HighlightValidMoves(Panel fromCell)
        {
            ClearHighlights();
            Point from = (Point)fromCell.Tag;
            CheckerPiece piece = fromCell.Controls[0] as CheckerPiece;
            string color = (string)piece.Tag;
            int[] dirs = { -1, 1 };

            bool globalCapture = HasAnyCapture(currentPlayer);

            if (piece.IsKing)
            {
                foreach (int dx in dirs)
                {
                    foreach (int dy in dirs)
                    {
                        int x = from.X + dx, y = from.Y + dy;
                        bool foundEnemy = false;

                        while (IsInBounds(x, y))
                        {
                            var cell = boardPanels[x, y];
                            if (cell.Controls.Count == 0 && !foundEnemy)
                            {
                                if (!globalCapture && !mustContinueCapture)
                                    cell.BackColor = HighlightColor;
                            }
                            else if (cell.Controls.Count > 0 && !foundEnemy)
                            {
                                var mid = cell.Controls[0] as CheckerPiece;
                                if ((string)mid.Tag != color)
                                    foundEnemy = true;
                                else break;
                            }
                            else if (cell.Controls.Count == 0 && foundEnemy)
                            {
                                cell.BackColor = CaptureHighlightColor;
                            }
                            else break;

                            x += dx; y += dy;
                        }
                    }
                }
            }
            else
            {
                if (!globalCapture && !mustContinueCapture)
                {
                    int forward = (color == "white") ? -1 : 1;
                    foreach (int dy in dirs)
                    {
                        int nx = from.X + forward;
                        int ny = from.Y + dy;
                        if (IsInBounds(nx, ny) && boardPanels[nx, ny].Controls.Count == 0)
                            boardPanels[nx, ny].BackColor = HighlightColor;
                    }
                }

                foreach (int dx in dirs)
                {
                    foreach (int dy in dirs)
                    {
                        int mx = from.X + dx;
                        int my = from.Y + dy;
                        int cx = from.X + 2 * dx;
                        int cy = from.Y + 2 * dy;

                        if (IsInBounds(mx, my) && IsInBounds(cx, cy))
                        {
                            var mid = boardPanels[mx, my];
                            var end = boardPanels[cx, cy];
                            if (mid.Controls.Count > 0 &&
                                (string)(mid.Controls[0] as CheckerPiece).Tag != color &&
                                end.Controls.Count == 0)
                            {
                                end.BackColor = CaptureHighlightColor;
                            }
                        }
                    }
                }
            }

            if (mustContinueCapture)
            {
                bool anyCap = false;
                for (int r = 0; r < BoardSize && !anyCap; r++)
                {
                    for (int c = 0; c < BoardSize; c++)
                    {
                        if (boardPanels[r, c].BackColor == CaptureHighlightColor)
                        {
                            anyCap = true;
                            break;
                        }
                    }
                }

                if (!anyCap)
                {
                    selectedPiece.ToggleSelect();
                    selectedPiece = null;
                    selectedPanel = null;
                    mustContinueCapture = false;
                    currentPlayer = (currentPlayer == "white") ? "black" : "white";
                    UpdateStatusLabel();
                    ClearHighlights();
                    CheckGameEnd();
                }
            }
        }

        private void ClearHighlights()
        {
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    boardPanels[r, c].BackColor = (r + c) % 2 == 0
                        ? DefaultCellColor1
                        : DefaultCellColor2;
                }
            }
        }

        private bool HasCaptureMoves(Panel fromCell)
        {
            Point from = (Point)fromCell.Tag;
            CheckerPiece piece = fromCell.Controls[0] as CheckerPiece;
            string color = (string)piece.Tag;
            int[] dirs = { -1, 1 };

            if (piece.IsKing)
            {
                foreach (int dx in dirs)
                {
                    foreach (int dy in dirs)
                    {
                        int x = from.X + dx, y = from.Y + dy;
                        bool foundEnemy = false;

                        while (IsInBounds(x, y))
                        {
                            var cell = boardPanels[x, y];
                            if (cell.Controls.Count == 0 && !foundEnemy)
                            {
                            }
                            else if (cell.Controls.Count > 0 && !foundEnemy)
                            {
                                var mid = cell.Controls[0] as CheckerPiece;
                                if ((string)mid.Tag != color)
                                    foundEnemy = true;
                                else break;
                            }
                            else if (cell.Controls.Count == 0 && foundEnemy)
                            {
                                return true;
                            }
                            else break;

                            x += dx; y += dy;
                        }
                    }
                }
            }
            else
            {
                foreach (int dx in dirs)
                {
                    foreach (int dy in dirs)
                    {
                        int mx = from.X + dx;
                        int my = from.Y + dy;
                        int cx = from.X + 2 * dx;
                        int cy = from.Y + 2 * dy;

                        if (IsInBounds(mx, my) && IsInBounds(cx, cy))
                        {
                            var mid = boardPanels[mx, my];
                            var end = boardPanels[cx, cy];
                            if (mid.Controls.Count > 0 &&
                                (string)(mid.Controls[0] as CheckerPiece).Tag != color &&
                                end.Controls.Count == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool HasAnyCapture(string playerColor)
        {
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    var cell = boardPanels[r, c];
                    if (cell.Controls.Count == 0) continue;

                    var piece = cell.Controls[0] as CheckerPiece;
                    if ((string)piece.Tag != playerColor) continue;

                    if (HasCaptureMoves(cell))
                        return true;
                }
            }
            return false;
        }

        private bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < BoardSize && y < BoardSize;

        #endregion

        #region Окончание игры

        private void CheckGameEnd()
        {
            if (!PlayerHasMoves(currentPlayer))
                ShowEndGame(currentPlayer == "white" ? "Победа чёрных!" : "Победа белых!");
        }

        private bool PlayerHasMoves(string color)
        {
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    var cell = boardPanels[r, c];
                    if (cell.Controls.Count == 0) continue;

                    var piece = cell.Controls[0] as CheckerPiece;
                    if ((string)piece.Tag != color) continue;

                    selectedPiece = piece;
                    selectedPanel = cell;
                    HighlightValidMoves(cell);

                    for (int i = 0; i < BoardSize; i++)
                    {
                        for (int j = 0; j < BoardSize; j++)
                        {
                            if (boardPanels[i, j].BackColor == HighlightColor ||
                                boardPanels[i, j].BackColor == CaptureHighlightColor)
                            {
                                ClearHighlights();
                                selectedPiece = null;
                                selectedPanel = null;
                                return true;
                            }
                        }
                    }

                    ClearHighlights();
                    selectedPiece = null;
                    selectedPanel = null;
                }
            }
            return false;
        }

        private void ShowEndGame(string message)
        {
            Form endForm = new Form
            {
                Text = "Игра окончена",
                Size = new Size(300, 200),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label
            {
                Text = message,
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            endForm.Controls.Add(label);

            Button menuButtonLocal = new Button
            {
                Text = "В главное меню",
                Size = new Size(150, 40),
                Location = new Point((endForm.ClientSize.Width - 150) / 2, 100)
            };
            menuButtonLocal.Click += (s, e) =>
            {
                netStream?.Close();
                netClient?.Close();
                endForm.Close();
                this.Close();
                new Form1().Show();
            };
            endForm.Controls.Add(menuButtonLocal);

            endForm.ShowDialog();
        }

        #endregion

        #region Обновление статуса

        private void UpdateStatusLabel()
        {
            if (netClient == null)
            {
                lblStatus.Text = "Локальный режим: ходят по очереди, белые начинают.";
            }
            else
            {
                string ruColor = myColor == "white" ? "белыми" : "чёрными";
                if (myTurn)
                    lblStatus.Text = $"Вы играете {ruColor}. Ваш ход.";
                else
                    lblStatus.Text = $"Вы играете {ruColor}. Ход соперника.";
            }
        }

        #endregion

        #region Кнопки «Сдаться» и «Ничья»

        private void BtnSurrender_Click(object sender, EventArgs e)
        {
            if (netClient == null)
            {
                // Локальный режим: сдался – победил другой
                string winner = (currentPlayer == "white") ? "чёрные" : "белые";
                ShowEndGame($"Игрок {currentPlayer} сдался. Победа {winner}!");
            }
            else
            {
                // Сетевой режим: сообщаем и завершаем
                SendMessage("SURRENDER");
                ShowEndGame("Вы сдались. Вы проиграли!");
            }
        }

        private void BtnDraw_Click(object sender, EventArgs e)
        {
            if (netClient == null)
            {
                // Локальный режим: показываем запрос второму игроку
                var res = MessageBox.Show("Игрок предлагает ничью. Принять?",
                    "Предложение ничьей", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                    ShowEndGame("Ничья по согласию!");
                // Иначе – ничего не происходит, игра продолжается
            }
            else
            {
                // Сетевой режим: отправляем запрос о ничье
                SendMessage("DRAW_REQUEST");
                MessageBox.Show("Предложение ничьей отправлено. Ждём ответа.",
                    "Ничья", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region HandleCellClick

        internal void HandleCellClick(Panel cell)
        {
            Cell_Click(cell, EventArgs.Empty);
        }

        #endregion
    }
}
