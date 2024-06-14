using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    //Dictionary<string, int> chessSquareValues = GenerateChessSquareValues();
    int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };

    bool isWhite = false;

    int[] PawnSquareTable = PieceSquareTables.GeneratePieceSquareTable(PieceType.Pawn);
    int[] KnightSquareTable = PieceSquareTables.GeneratePieceSquareTable(PieceType.Knight);
    int[] BishopSquareTable = PieceSquareTables.GeneratePieceSquareTable(PieceType.Bishop);
    int[] RookSquareTable = PieceSquareTables.GeneratePieceSquareTable(PieceType.Rook);
    int[] QueenSquareTable = PieceSquareTables.GeneratePieceSquareTable(PieceType.Queen);
    int[] KingSquareTableOpening = PieceSquareTables.GeneratePieceSquareTable(PieceType.King);

    public Move Think(Board board, Timer timer)
    {
        int depth = 3;
        //Console.WriteLine(board.GetLegalMoves().Length);

        if (board.IsWhiteToMove)
        {
            isWhite = true;
        }
        else
        {
            isWhite = false;
        }

        Move move = GetBestMove(board, timer, depth);

        Console.WriteLine(
        move.TargetSquare.Name);

        string s = move.ToString();
        s = s.ToLower();
        s = s.Replace("move", "");
        s = s.Replace(":", "");
        s = s.Replace("'", "");
        s = s.Trim();

        UCIHandler.UCIString += s + " ";

        return move;
    }

    Move GetBestMove(Board board, Timer timer, int maxDepth)
    {
        int alpha = int.MinValue;
        int beta = int.MaxValue;
        int bestMoveVal = int.MinValue;
        Move bestMove = board.GetLegalMoves()[0];

        foreach (Move move in board.GetLegalMoves())
        {
            if (timer.MillisecondsRemaining < 5000)
            {
                GetBestMove(board, timer, 4);
            }

            board.MakeMove(move);

            if(board.IsInCheckmate())
            {
                return move;
            }

            int eval = Minimax(board, maxDepth -1, alpha, beta, false);

            board.UndoMove(move);

            //Console.Write("eval attempt " + move.MovePieceType + " " + move.ToString());
            //Console.Write(" Is capture? " + move.IsCapture.ToString() + " ");
            //Console.Write("eval is " + eval);
            //Console.WriteLine();

            if (eval > bestMoveVal)
            {
                //Console.Write("Best move is now " + move.MovePieceType + " " + move.ToString());
                //Console.Write(" Is capture? " + move.IsCapture.ToString() + " ");
                //Console.Write("best eval is " + eval);
                //Console.WriteLine();
                bestMoveVal = eval;
                bestMove = move;
            }

        }
        //Console.WriteLine("Turn number is now " + turnNumber);
        return bestMove;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0)
        {
            return EvaluateBoard(board);
        }

        if(maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (Move legalMove in board.GetLegalMoves())
            {
                board.MakeMove(legalMove);
                int eval = Minimax(board, depth - 1, alpha, beta, false);
                board.UndoMove(legalMove);

                maxEval = Math.Max(maxEval, eval);

                alpha = Math.Max(alpha, maxEval);

                if (beta <= alpha)
                    break;
                
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (Move legalMove in board.GetLegalMoves())
            {
                board.MakeMove(legalMove);
                int eval = Minimax(board,depth - 1, alpha, beta, true);
                board.UndoMove(legalMove);

                minEval = Math.Min(minEval, eval);

                beta = Math.Min(beta, minEval);

                if(beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

    int EvaluateBoard(Board board)
    {
        int whiteScore = 0;
        int blackScore = 0;

        foreach (var pieceList in board.GetAllPieceLists())
        {
            foreach (var piece in pieceList)
            {
                if (piece.IsWhite)
                {
                    whiteScore += pieceValues[(int)piece.PieceType];
                    if (piece.IsPawn && piece.Square.Rank == 7)
                    {
                        // Promotion score
                        whiteScore += 900;
                    }
                    whiteScore += EvaluatePieceSquare(piece, true);
                }
                else
                {
                    blackScore += pieceValues[(int)piece.PieceType];
                    if (piece.IsPawn && piece.Square.Rank == 0)
                    {
                        // Promotion score
                        blackScore += 900;
                    }
                    blackScore += EvaluatePieceSquare(piece, false);
                }
            }
        }

        // Evaluate other factors like mobility, pawn structure, etc.

        if (isWhite)
        {
            return whiteScore - blackScore;
        }
        else
        {
            return blackScore - whiteScore;
        }
    }

    int EvaluatePieceSquare(Piece piece, bool isWhite)
    {
        int[] squareTable = null;
        switch (piece.PieceType)
        {
            case PieceType.Pawn:
                squareTable = PawnSquareTable;
                break;
            case PieceType.Knight:
                squareTable = KnightSquareTable;
                break;
            case PieceType.Bishop:
                squareTable = BishopSquareTable;
                break;
            case PieceType.Rook:
                squareTable = RookSquareTable;
                break;
            case PieceType.Queen:
                squareTable = QueenSquareTable;
                break;
            case PieceType.King:
                squareTable = KingSquareTableOpening; // Adjust this if you have separate tables for different game phases
                break;
            default:
                break;
        }

        if (squareTable != null)
        {
            int squareIndex = piece.Square.Index;
            int score = squareTable[squareIndex];
            return isWhite ? score : -score; // Return positive score for white and negative for black
        }
        return 0; // If no square table is available for the piece
    }
}

static class PieceSquareTables
{
    public static int[] GeneratePieceSquareTable(PieceType pieceType)
    {
        int[] pieceSquareTable = new int[64];
        int index = 0;

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int squareValue = 0;

                switch (pieceType)
                {
                    case PieceType.Pawn:
                        squareValue = GeneratePawnSquareValue(rank, file);
                        break;
                    case PieceType.Knight:
                        squareValue = GenerateKnightSquareValue(rank, file);
                        break;
                    case PieceType.Bishop:
                        squareValue = GenerateBishopSquareValue(rank, file);
                        break;
                    case PieceType.Rook:
                        squareValue = GenerateRookSquareValue(rank, file);
                        break;
                    case PieceType.Queen:
                        squareValue = GenerateQueenSquareValue(rank, file);
                        break;
                    case PieceType.King:
                        squareValue = GenerateKingSquareValue(rank, file);
                        break;
                    default:
                        break;
                }

                Square square = new Square(index);

                Console.WriteLine(pieceType.ToString() + " has square table on square " + square.ToString() + " with value of " + squareValue);

                pieceSquareTable[index++] = squareValue;
            }
        }

        return pieceSquareTable;
    }

    static int GeneratePawnSquareValue(int rank, int file)
    {
        // Pawns are more valuable when they advance towards the enemy's side
        int value = 100;

        // Bonus for having advanced pawns
        int centerFileDistance = Math.Abs(file - 3); // Distance to the central file (d-file)
        int centerRankDistance = Math.Abs(rank - 3); // Distance to the central rank (4th rank)
        int centerDistance = centerFileDistance + centerRankDistance;

        if (centerDistance == 0) // Central square
            value += 200;
        else if (centerDistance == 1) // Next to central square
            value += 200;
        else if (centerDistance == 2) // Two squares away from central square
            value += 100;

        return value;
    }

    static int GenerateKnightSquareValue(int rank, int file)
    {
        // Knights are generally more valuable towards the center and sides of the board
        int value = 300;
        int centerDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(file - 3));
        value += (4 - centerDistance) * 50;

        return value;
    }

    static int GenerateBishopSquareValue(int rank, int file)
    {
        // Bishops are generally more valuable towards the center and sides of the board
        int value = 325;
        int centerDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(file - 3));
        value += (4 - centerDistance) * 30;

        return value;
    }

    static int GenerateRookSquareValue(int rank, int file)
    {
        // Rooks are more valuable towards the center and enemy's side of the board
        int value = 500;
        int centerDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(file - 3));
        if (rank == 7) // Rook on the enemy's side
            value += 50;
        else if (centerDistance == 0) // Rook in the center
            value += 30;

        return value;
    }

    static int GenerateQueenSquareValue(int rank, int file)
    {
        // Queens are generally more valuable towards the center and enemy's side of the board
        int value = 900;
        int centerDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(file - 3));
        if (rank == 7) // Queen on the enemy's side
            value += 100;
        else if (centerDistance == 0) // Queen in the center
            value += 50;

        return value;
    }

    static int GenerateKingSquareValue(int rank, int file)
    {
        // In the endgame, the King should move towards the center of the board
        int value = 20000;
        int centerDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(file - 3));
        value -= centerDistance * 200;

        return value;
    }
}