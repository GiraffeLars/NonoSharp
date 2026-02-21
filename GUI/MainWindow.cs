using Picross;
using System.Diagnostics;

namespace GUI
{
    public partial class MainWindow : Form
    {
        private Grid grid = Game.grid;
        public MainWindow()
        {
            InitializeComponent();
        }

        private int colWidth { get { return panelBoard.Width / grid.width; } }
        private int colHeight { get { return  panelBoard.Height / grid.height; }  }

        private void panelBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush b = grid.isCorrect() ? new SolidBrush(Color.Green) : new SolidBrush(Color.Black);
            Pen pCross = new Pen(Color.Black, 4);
            Pen pBox = new Pen(Color.Gray, 2);

            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    // Coordinates of the top-left and bottom-right of square
                    int x0 = x * colWidth;
                    int y0 = y * colHeight;
                    int x1 = x0 + colWidth;
                    int y1 = y0 + colHeight;

                    switch (grid.getCell(x, y))
                    {
                        case SquareType.BLANK:
                            break;
                        case SquareType.FILLED:
                            // Fill Square
                            g.FillRectangle(b, x0, y0, colWidth, colHeight);
                            break;
                        case SquareType.CROSS:
                            // Draw a cross
                            g.DrawLine(pCross,
                                x0, y0, x1, y1);
                            g.DrawLine(pCross,
                                x0, y1, x1, y0);

                            break;
                    }

                    g.DrawRectangle(pBox,
                            x0, y0, x1, y1);
                }
            }
        }

        private void panelVerHints_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 12);
            StringFormat stringFormat = new StringFormat();

            for (int x = 0; x < grid.width; x++)
            {
                List<int> hints = grid.verticalHints[x];

                for (int y = 0; y < hints.Count; y++)
                {
                    g.DrawString(hints[y].ToString(), font, Brushes.Black,
                        colWidth * x + colWidth / 2, y * 20, stringFormat);
                }
            }
        }

        private void panelHorHints_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 12);
            StringFormat stringFormat = new StringFormat();

            for (int y = 0; y < grid.height; y++)
            {
                List<int> hints = grid.horizontalHints[y];

                for (int x = 0; x < hints.Count; x++)
                {
                    g.DrawString(hints[x].ToString(), font, Brushes.Black,
                        x * 20, y * colHeight + colHeight / 2, stringFormat);
                }
            }
        }

        private void panelBoard_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X / colWidth;
            int y = e.Y / colHeight;

            SquareType sType;
            if (e.Button == MouseButtons.Left) {
                sType = grid.getCell(x, y) == SquareType.FILLED ? SquareType.BLANK : SquareType.FILLED;
            } else if (e.Button == MouseButtons.Right)
            {
                sType = grid.getCell(x, y) == SquareType.CROSS ? SquareType.BLANK : SquareType.CROSS;
            } else
            {
                sType = SquareType.BLANK;
            }

            try
            {
                grid.setCell(x, y, sType);
            }
            catch (ArgumentOutOfRangeException)
            {
                return; // Silently fail, it is not important to let users know this click is out of bounds
            }

            Debug.WriteLine(grid);
            Debug.WriteLine($"Correct? {grid.isCorrect()}");

            // Repaint necessary
            panelBoard.Invalidate();
        }
    }
}