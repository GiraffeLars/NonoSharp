using Microsoft.Maui.Graphics;
using Picross.Game;

namespace Maui.Drawables;

internal class BoardDrawable : IDrawable
{
    private GameAPI game;
    private float cellSize;
    internal bool filling = true;

    public BoardDrawable(GameAPI game)
    {
        this.game = game;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float boardSize = Math.Min(dirtyRect.Width, dirtyRect.Height);
        cellSize = boardSize / game.Width;

        DrawSquares(canvas);
        DrawLines(canvas);
    }

    private void DrawSquares(ICanvas canvas)
    {
        canvas.FillColor = game.IsPuzzleSolved() ? Colors.Green : Colors.Black;
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4;

        for (int x = 0; x < game.Width; x++)
        {
            for (int y = 0; y < game.Height; y++)
            {
                if (game.IsSquareEmpty(x, y))
                {
                    continue;
                }

                float x0 = x * cellSize;
                float y0 = y * cellSize;
                float x1 = x0 + cellSize;
                float y1 = y0 + cellSize;

                if (game.IsSquareFilled(x, y))
                {
                    canvas.FillRectangle(x0, y0, cellSize, cellSize);
                } else
                {
                    canvas.DrawLine(x0, y0, x1, y1);
                    canvas.DrawLine(x0, y1, x1, y0);
                }
            }
        }
    }
    private void DrawLines(ICanvas canvas)
    {
        canvas.StrokeColor = Colors.Gray;
        for (int x = 0; x <= game.Width; x++)
        {
            if (x % 5 == 0)
            {
                canvas.StrokeSize = 4;
            }
            else
            {
                canvas.StrokeSize = 2;
            }

            canvas.DrawLine(x * cellSize, 0, x * cellSize, game.Height * cellSize);
        }

        for (int y = 0; y <= game.Height; y++)
        {
            if (y % 5 == 0)
            {
                canvas.StrokeSize = 4;
            }
            else
            {
                canvas.StrokeSize = 2;
            }


            canvas.DrawLine(0, y * cellSize, game.Width * cellSize, y * cellSize);
        }
    }

    /// <summary>
    /// Converts touch coordinates to cell coordinates.
    /// </summary>
    /// <param name="touchX">x coordinate of touch</param>
    /// <param name="touchY">y coordinate of touch</param>
    /// <returns><c>Point</c> of cell touched.</returns>
    public Point ConvertTouchToCell(double touchX, double touchY)
    {
        return new Point(touchX / cellSize, touchY / cellSize);
    }

    public Point ConvertTouchToCell(Point touchCoordinates)
    {
        return ConvertTouchToCell(touchCoordinates.X, touchCoordinates.Y);
    }

    /// <summary>
    /// Converts touch coordinates to cell coordinates, then properly handles the cell. 
    /// See <seealso cref="ConvertTouchToCell(double, double)"/> and <seealso cref="HandleCell(int, int)"/>
    /// </summary>
    /// <param name="touchX"></param>
    /// <param name="touchY"></param>
    public void HandleTouch(float touchX, float touchY)
    {
        Point cell = ConvertTouchToCell(touchX, touchY);
        HandleCell((int) cell.X, (int) cell.Y);
    }

    /// <summary>
    /// Handles a clicked square by updating its state according to selected mode.
    /// </summary>
    /// <param name="x">x coordinate of cell</param>
    /// <param name="y">y coordinate of cell</param>
    public void HandleCell(int x, int y)
    {
        if (x < 0 || x >= game.Width || y < 0 || y >= game.Height)
        {
            return;
        }

        if (game.IsSquareFilled(x, y))
        {
            HandleClickFilledSquare(x, y);
        }
        else if (game.IsSquareCrossed(x, y))
        {
            HandleClickCrossedSquare(x, y);
        }
        else
        {
            HandleClickEmptySquare(x, y);
        }
    }

    /// <summary>
    /// Handles a clicked square by updating its state according to selected mode. See also <seealso cref="HandleCell(int, int)"/>.
    /// </summary>
    /// <param name="cell">The coordinates of the clicked cell (so in cell coordinates)</param>
    public void HandleCell(Point cell)
    {
        HandleCell((int) cell.X, (int) cell.Y);
    }

    private void HandleClickFilledSquare(int x, int y)
    {
        if (!filling)
        {
            game.CrossCell(x, y);
            return;
        }

        if (game.IsSquareEmpty(x, y))
        {
            game.FillCell(x, y);
            return;
        }

        // For all other mouse clicks we empty the cell
        game.EmptyCell(x, y);
    }

    private void HandleClickCrossedSquare(int x, int y)
    {
        if (filling)
        {
            game.FillCell(x, y);
            return;
        }

        if (game.IsSquareEmpty(x, y))
        {
            game.CrossCell(x, y);
            return;
        }

        // For all other mouse clicks we empty the cell
        game.EmptyCell(x, y);
    }

    private void HandleClickEmptySquare(int x, int y)
    {
        if (filling)
        {
            game.FillCell(x, y);
            return;
        }

        if (!filling)
        {
            game.CrossCell(x, y);
            return;
        }

        game.FillCell(x, y);

        // Ignore other mouse buttons
    }
}