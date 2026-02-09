using System;
using Picross;

class Program
{
    static void Main()
    {
        Grid grid = new Grid(10, 10);
        Console.Write(grid);
        char x;
        char y;
        while (true)
        {
            x = Console.ReadKey().KeyChar;
            Console.WriteLine();
            y = Console.ReadKey().KeyChar;
            Console.WriteLine();

            grid.setCell((int) Char.GetNumericValue(x), (int) Char.GetNumericValue(y), SquareType.FILLED);
            Console.Write(grid);
        }

    }
}