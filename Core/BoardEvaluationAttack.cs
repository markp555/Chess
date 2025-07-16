using GenericSearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrcChess2.Core
{
    public class BoardEvaluationAttack : IBoardEvaluation
    {

        /// <summary>
        /// Class constructor
        /// </summary>
        public BoardEvaluationAttack() { }

        /// <summary>Value of each piece/color.</summary>
        static private int[] PiecesPoint { get; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static BoardEvaluationAttack()
        {
            PiecesPoint = new int[16];
            PiecesPoint[(int)ChessBoard.PieceType.Pawn] = 100;
            PiecesPoint[(int)ChessBoard.PieceType.Rook] = 500;
            PiecesPoint[(int)ChessBoard.PieceType.Knight] = 300;
            PiecesPoint[(int)ChessBoard.PieceType.Bishop] = 325;
            PiecesPoint[(int)ChessBoard.PieceType.Queen] = 900;
            PiecesPoint[(int)ChessBoard.PieceType.King] = 1000000;
            PiecesPoint[(int)(ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black)] = -100;
            PiecesPoint[(int)(ChessBoard.PieceType.Rook | ChessBoard.PieceType.Black)] = -500;
            PiecesPoint[(int)(ChessBoard.PieceType.Knight | ChessBoard.PieceType.Black)] = -300;
            PiecesPoint[(int)(ChessBoard.PieceType.Bishop | ChessBoard.PieceType.Black)] = -325;
            PiecesPoint[(int)(ChessBoard.PieceType.Queen | ChessBoard.PieceType.Black)] = -900;
            PiecesPoint[(int)(ChessBoard.PieceType.King | ChessBoard.PieceType.Black)] = -1000000;
        }

        /// <summary>
        /// Name of the evaluation method
        /// </summary>
        public string Name => "Attack Version";

        /// <summary>
        /// Evaluates a board. The number of point is greater than 0 if white is in advantage, less than 0 if black is.
        /// </summary>
        /// <param name="board">            Board</param>
        /// <param name="countPerPiece">    Number of each pieces</param>
        /// <param name="attackPosInfo">    Information about attacking position</param>
        /// <param name="whiteKingPos">     Position of the white king</param>
        /// <param name="blackKingPos">     Position of the black king</param>
        /// <param name="whiteCastle">      White has castled</param>
        /// <param name="blackCastle">      Black has castled</param>
        /// <param name="moveCountDelta">   Number of possible white move - Number of possible black move</param>
        /// <returns>
        /// Points. > 0: White advantage, < 0: Black advantage
        /// </returns>
        public int Points(ChessBoard.PieceType[] board,
                          int[] countPerPiece,
                          AttackPosInfo attackPosInfo,
                          int whiteKingPos,
                          int blackKingPos,
                          bool whiteCastle,
                          bool blackCastle,
                          int moveCountDelta)
        {
            int retVal = 0;

            for (int i = 0; i < countPerPiece.Length; i++)
            {
                retVal += PiecesPoint[i] * countPerPiece[i];
            }
            if (board[12] == ChessBoard.PieceType.Pawn)
            {
                retVal -= 4;
            }
            if (board[52] == (ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black))
            {
                retVal += 4;
            }
            if (whiteCastle)
            {
                retVal += 50;
            }
            if (blackCastle)
            {
                retVal -= 50;
            }
            retVal += moveCountDelta;
            //retVal += attackPosInfo.PiecesAttacked * 3;
            //retVal += attackPosInfo.PiecesDefending * 2;

            //if (attackPosInfo.WhiteCheck)
            //    retVal -= 100;
            //if (attackPosInfo.BlackCheck)
            //    retVal += 100;

            int endgame = 55;
            int figcnt = 0;
            for (int i = 0; i < countPerPiece.Length; i++)
            {
                figcnt += Math.Abs(PiecesPoint[i]) * countPerPiece[i];
            }
            figcnt -= 2000000;
            if (figcnt < 1000)
                endgame = -15;
            bool rotateWhite = whiteKingPos < 32;
            bool rotateBlack = blackKingPos < 32;
            for (int i = 0; i < 64; i++)
            {
                if (board[i] == ChessBoard.PieceType.None)
                    continue;
                if ((board[i] & ChessBoard.PieceType.Black) == ChessBoard.PieceType.Black)
                {
                    int pos = rotateBlack ? i / 8 : 7 - i / 8;
                    if (board[i] == (ChessBoard.PieceType.King | ChessBoard.PieceType.Black))
                        retVal += pos * endgame;
                    else
                        retVal -= pos * 5;
                }
                else
                {
                    int pos = rotateWhite ? i / 8 : 7 - i / 8;
                    if (board[i] == ChessBoard.PieceType.King)
                        retVal -= pos * endgame;
                    else
                        retVal += pos * 5;
                }
            }

            return retVal;
        }
    }
}
