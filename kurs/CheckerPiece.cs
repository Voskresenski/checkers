using kurs;
using System.Drawing;
using System.Windows.Forms;

public class CheckerPiece : Panel
{
    public bool IsKing { get; set; } = false;

    public CheckerPiece(string color)
    {
        this.Width = 40;
        this.Height = 40;
        this.BackColor = Color.Transparent;
        this.Tag = color;
        this.Paint += CheckerPiece_Paint;
    }

    private void CheckerPiece_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Color pieceColor = (string)Tag == "white" ? Color.White : Color.Black;
        Brush brush = new SolidBrush(pieceColor);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.FillEllipse(brush, 0, 0, Width - 1, Height - 1);
        g.DrawEllipse(Pens.Gray, 0, 0, Width - 1, Height - 1);

        if (IsKing)
        {
            Font font = new Font("Arial", 14, FontStyle.Bold);
            TextRenderer.DrawText(g, "♛", font, new Rectangle(0, 0, Width, Height),
                pieceColor == Color.White ? Color.Black : Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public void ToggleSelect()
    {
        this.BorderStyle = this.BorderStyle == BorderStyle.None ? BorderStyle.FixedSingle : BorderStyle.None;
    }
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (this.Parent is Panel panel && this.FindForm() is GameForm form)
        {
            form.HandleCellClick(panel);
        }
    }





}
