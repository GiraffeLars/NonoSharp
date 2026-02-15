using System;
using Picross;

class Program
{
    static void Main()
    {
        Grid grid = new Grid(5, 5);
        Console.Write(grid);
        int x;
        int y;
        SquareType fillType = SquareType.FILLED;
        while (true)
        {
            char read = Console.ReadKey().KeyChar;
            if (read == 'x')
            {
                fillType = fillType == SquareType.FILLED ? SquareType.CROSS : SquareType.FILLED;
                continue;
            }
            x = (int) Char.GetNumericValue(read);
            Console.WriteLine();
            y = (int) Char.GetNumericValue(Console.ReadKey().KeyChar);
            Console.WriteLine();

            try
            {
                grid.setCell(x, y, grid.getCell(x, y) == SquareType.BLANK ? fillType : SquareType.BLANK);
            } catch (ArgumentOutOfRangeException)
            {
                continue;
            }
            Console.Write(grid);
        }

    }
}