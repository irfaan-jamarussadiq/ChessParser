using System;

namespace ChessParser
{
    class Program
    {
        static void Main(string[] args)
        {
            GameParser gameParser = new GameParser("game2.txt");
            Console.WriteLine(gameParser.Board.WhiteKing);
            Console.WriteLine(gameParser.Board.BlackKing);
        }
    }
}
