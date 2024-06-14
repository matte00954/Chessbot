using ChessChallenge.API;
using ChessChallenge.Chess;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {
        static Process stockfishProcess = new Process();
        static bool hasStarted = false;
        int depth = 2;

        public API.Move Think(API.Board board, Timer timer)
        {

            if (hasStarted == false)
            {
                stockfishProcess.StartInfo.FileName = "D:/stockfish/stockfish.exe";
                stockfishProcess.StartInfo.UseShellExecute = false;
                stockfishProcess.StartInfo.RedirectStandardInput = true;
                stockfishProcess.StartInfo.RedirectStandardOutput = true;
                stockfishProcess.Start();

                // Send UCI commands to Stockfish
                stockfishProcess.StandardInput.WriteLine("uci");
                stockfishProcess.StandardInput.WriteLine("isready");
                stockfishProcess.StandardInput.WriteLine("ucinewgame");

                hasStarted = true;
            }

            //Console.WriteLine("Stockfish will see FEN as " + board.GetFenString());

            //Console.WriteLine("Stockfish will see UCI as " + UCIHandler.UCIString);

            return PlayMove(board);
        }

        public API.Move PlayMove(API.Board board)
        {
            // Play a move in your C# chess game and send the move to Stockfish
            //stockfishProcess.StandardInput.WriteLine("position startpos moves " + move);
            stockfishProcess.StandardInput.WriteLine("position fen " + board.GetFenString());
            stockfishProcess.StandardInput.WriteLine("go depth " + depth);

            // Read the engine's response to get the best move and evaluation score
            string bestMove = "";
            int evaluation = 0;
            string engineOutput;

            while (bestMove == string.Empty)
            {
                // Print engine output for debugging (optional)
                engineOutput = stockfishProcess.StandardOutput.ReadLine();

                if (engineOutput.StartsWith("bestmove"))
                {
                    // Extract the best move from the output
                    string[] parts = engineOutput.Split(' ');
                    bestMove = parts[1];
                    break;
                }
            }

            // Use the best move and evaluation in your chess game
            //Console.WriteLine("Best Move: " + bestMove);
            //Console.WriteLine("Evaluation: " + evaluation);

            UCIHandler.UCIString += bestMove + " ";

            var toPlay = new API.Move(bestMove,board);

            /*
            if(toPlay.ToString() == "" || toPlay.ToString() == "(none)" || toPlay.ToString().Contains("none"))
            {
                return board.GetLegalMoves()[0];
            }
            */

            return toPlay;
        }
    }
}