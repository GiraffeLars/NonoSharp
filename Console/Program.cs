using System;
using Picross;

class Program
{
    static void Main()
    {
        GameAPI game = new GameAPI(5, 5);
        Console.Write(game.ToString());
        int x;
        int y;
        
        Action<int, int> changeCellAction = game.FillCell;
        bool fillingSquares = true;

        while (true)
        {
            char read = Console.ReadKey().KeyChar;
            if (read == 'x')
            {
                if (fillingSquares)
                {
                    fillingSquares = false;
                    changeCellAction = game.CrossCell;
                } else
                {
                    fillingSquares = true;
                    changeCellAction = game.FillCell;
                }
                continue;
            }
            x = (int) Char.GetNumericValue(read);
            Console.WriteLine();
            y = (int) Char.GetNumericValue(Console.ReadKey().KeyChar);
            Console.WriteLine();

            try
            {
                if (!game.IsSquareEmpty(x, y))
                {
                    changeCellAction(x, y);
                }
                else
                {
                    game.EmptyCell(x, y);
                }
            } catch (ArgumentOutOfRangeException)
            {
                continue;
            }
            Console.Write(game);
        }

    }
}