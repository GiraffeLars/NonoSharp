using Core;
using System.Diagnostics;

namespace GUI
{
    public partial class MainWindow : Form
    {
        private GameAPI game = new GameAPI(10, 10);
        public MainWindow()
        {
            InitializeComponent();
        }

        private int colWidth { get { return panelBoard.Width / game.Width; } }
        private int colHeight { get { return panelBoard.Height / game.Height; } }

        private void panelBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush b = game.IsPuzzleSolved() ? new SolidBrush(Color.Green) : new SolidBrush(Color.Black);
            Pen pCross = new Pen(Color.Black, 4);

            for (int x = 0; x < game.Width; x++)
            {
                for (int y = 0; y < game.Height; y++)
                {
                    // Coordinates of the top-left and bottom-right of square
                    int x0 = x * colWidth;
                    int y0 = y * colHeight;
                    int x1 = x0 + colWidth;
                    int y1 = y0 + colHeight;

                    if (game.IsSquareEmpty(x, y))
                    {
                        continue;
                    }

                    if (game.IsSquareFilled(x, y))
                    {
                        g.FillRectangle(b, x0, y0, colWidth, colHeight);
                        continue;
                    }

                    g.DrawLine(pCross, x0, y0, x1, y1);
                    g.DrawLine(pCross, x0, y1, x1, y0);
                }
            }

            DrawLines(g);
        }

        private void DrawLines(Graphics g)
        {
            Pen p = new Pen(Brushes.Gray, 2);

            for (int x = 0; x <= game.Width; x++)
            {
                if (x % 5 == 0)
                {
                    p.Width = 4;
                }
                else
                {
                    p.Width = 2;
                }

                g.DrawLine(p, x * colWidth, 0, x * colWidth, game.Height * colHeight);
            }

            for (int y = 0; y <= game.Height; y++)
            {
                if (y % 5 == 0)
                {
                    p.Width = 4;
                }
                else
                {
                    p.Width = 2;
                }

                g.DrawLine(p, 0, y * colHeight, game.Width * colWidth, y * colHeight);
            }
        }

        private void panelVerHints_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 12);
            StringFormat stringFormat = new StringFormat();

            for (int x = 0; x < game.Width; x++)
            {
                Hints hints = game.VerticalHints[x];

                for (int y = hints.Count - 1; y >= 0; y--)
                {
                    Hint hint = hints.GetHint(y);
                    Brush colour = hint.Completed ? Brushes.Gray : Brushes.Black;
                    g.DrawString(hint.Number.ToString(), font, colour,
                        colWidth * x + colWidth / 2, panelVerHints.Height - (hints.Count - y) * 20, stringFormat);
                }
            }
        }

        private void panelHorHints_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 12);
            StringFormat stringFormat = new StringFormat();

            for (int y = 0; y < game.Height; y++)
            {
                Hints hints = game.HorizontalHints[y];

                for (int x = hints.Count - 1; x >= 0; x--)
                {
                    Hint hint = hints.GetHint(x);
                    Brush colour = hint.Completed ? Brushes.Gray : Brushes.Black;

                    g.DrawString(hint.Number.ToString(), font, colour,
                        panelHorHints.Width - (hints.Count - x) * 20, y * colHeight + colHeight / 2, stringFormat);
                }
            }
        }

        private void panelBoard_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X / colWidth;
            int y = e.Y / colHeight;
            MouseButtons mouseButton = e.Button;

            try
            {
                if (game.IsSquareFilled(x, y))
                {
                    HandleClickFilledSquare(x, y, mouseButton);
                }
                else if (game.IsSquareCrossed(x, y))
                {
                    HandleClickCrossedSquare(x, y, mouseButton);
                }
                else
                {
                    HandleClickEmptySquare(x, y, mouseButton);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Silently fail, unimport for user as they missclicked
                return;
            }

            // Repaint necessary for the clicked square
            if (game.IsPuzzleSolved())
            {
                panelBoard.Invalidate();
            } 
            else
            {
                panelBoard.Invalidate(new Rectangle(x * colWidth, y * colHeight, colWidth, colHeight));
            }
            panelVerHints.Invalidate();
            panelHorHints.Invalidate();
        }

        private void HandleClickFilledSquare(int x, int y, MouseButtons but)
        {
            if (but == MouseButtons.Right)
            {
                game.CrossCell(x, y);
                return;
            }

            if (but == MouseButtons.Left && game.IsSquareEmpty(x, y))
            {
                game.FillCell(x, y);
                return;
            }

            // For all other mouse clicks we empty the cell
            game.EmptyCell(x, y);
        }

        private void HandleClickCrossedSquare(int x, int y, MouseButtons but)
        {
            if (but == MouseButtons.Left)
            {
                game.FillCell(x, y);
                return;
            }

            if (but == MouseButtons.Right && game.IsSquareEmpty(x, y))
            {
                game.CrossCell(x, y);
                return;
            }

            // For all other mouse clicks we empty the cell
            game.EmptyCell(x, y);
        }

        private void HandleClickEmptySquare(int x, int y, MouseButtons but)
        {
            if (but == MouseButtons.Left)
            {
                game.FillCell(x, y);
                return;
            }

            if (but == MouseButtons.Right)
            {
                game.CrossCell(x, y);
                return;
            }

            // Ignore other mouse buttons
        }

        private void MainWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'z')
            {
                game.Undo();
                panelBoard.Invalidate();
            } else if (e.KeyChar == 'y')
            {
                game.Redo();
                panelBoard.Invalidate();
            }
        }
    }
}