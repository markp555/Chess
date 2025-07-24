using GenericSearchEngine;

namespace SrcChess2.Core {
    /// <summary>
    /// Test board evaluation function
    /// </summary>
    public class BoardEvaluationTest : IBoardEvaluation {

        /// <summary>
        /// Class constructor
        /// </summary>
        public BoardEvaluationTest() {}

        /// <summary>Value of each piece/color.</summary>
        static private int[] PiecesPoint { get; }

        static private int[] PiecesMobilityPoint { get; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static BoardEvaluationTest() {
            PiecesPoint = new int[16];
            PiecesPoint[(int)ChessBoard.PieceType.Pawn]   = 100;
            PiecesPoint[(int)ChessBoard.PieceType.Rook]   = 500;
            PiecesPoint[(int)ChessBoard.PieceType.Knight] = 300;
            PiecesPoint[(int)ChessBoard.PieceType.Bishop] = 325;
            PiecesPoint[(int)ChessBoard.PieceType.Queen]  = 900;
            PiecesPoint[(int)ChessBoard.PieceType.King]   = 1000000;
            PiecesPoint[(int)(ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black)]   = -100;
            PiecesPoint[(int)(ChessBoard.PieceType.Rook | ChessBoard.PieceType.Black)]   = -500;
            PiecesPoint[(int)(ChessBoard.PieceType.Knight | ChessBoard.PieceType.Black)] = -300;
            PiecesPoint[(int)(ChessBoard.PieceType.Bishop | ChessBoard.PieceType.Black)] = -325;
            PiecesPoint[(int)(ChessBoard.PieceType.Queen | ChessBoard.PieceType.Black)]  = -900;
            PiecesPoint[(int)(ChessBoard.PieceType.King | ChessBoard.PieceType.Black)]   = -1000000;
            PiecesMobilityPoint = new int[16];
            PiecesMobilityPoint[(int)ChessBoard.PieceType.Pawn] = 1;
            PiecesMobilityPoint[(int)ChessBoard.PieceType.Rook] = 3;
            PiecesMobilityPoint[(int)ChessBoard.PieceType.Knight] = 8;
            PiecesMobilityPoint[(int)ChessBoard.PieceType.Bishop] = 5;
            PiecesMobilityPoint[(int)ChessBoard.PieceType.Queen] = 3;
            PiecesMobilityPoint[(int)ChessBoard.PieceType.King] = 0;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black)] = -1;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.Rook | ChessBoard.PieceType.Black)] = -3;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.Knight | ChessBoard.PieceType.Black)] = -8;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.Bishop | ChessBoard.PieceType.Black)] = -5;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.Queen | ChessBoard.PieceType.Black)] = -3;
            PiecesMobilityPoint[(int)(ChessBoard.PieceType.King | ChessBoard.PieceType.Black)] = 0;
        }

        /// <summary>
        /// Name of the evaluation method
        /// </summary>
        public string Name => "Enhanced Version";

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
                          int[]                  countPerPiece,
                          AttackPosInfo          attackPosInfo,
                          int                    whiteKingPos,
                          int                    blackKingPos,
                          bool                   whiteCastle,
                          bool                   blackCastle,
                          int                    moveCountDelta) {
            int retVal = 0;
            
            for (int i = 0; i < countPerPiece.Length; i++) {
                retVal += PiecesPoint[i] * countPerPiece[i];
            }
            if (board[12] == ChessBoard.PieceType.Pawn) {
                retVal -= 4;
            }
            if (board[52] == (ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black)) {
                retVal += 4;
            }
            if (whiteCastle) {
                retVal += 50;
            }
            if (blackCastle) {
                retVal -= 50;
            }
            retVal += moveCountDelta;
            //retVal += attackPosInfo.PiecesAttacked * 3;
            //retVal += attackPosInfo.PiecesDefending * 2;

            if (attackPosInfo.WhiteCheck)
                retVal -= 15;
            if (attackPosInfo.BlackCheck)
                retVal += 15;

            for (int i = 0; i < countPerPiece.Length; i++)
            {
                retVal += PiecesMobilityPoint[i] * attackPosInfo.PiecesMobility[i];
            }

            int[,] pawnCount = new int[8, 2];
            for (int i = 0; i < 64; i++)
            {
                if ((board[i] & ChessBoard.PieceType.PieceMask) == ChessBoard.PieceType.Pawn)
                {
                    pawnCount[i % 8, (board[i] & ChessBoard.PieceType.Black) == ChessBoard.PieceType.Black ? 1 : 0] += 1;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                retVal -= 13 * pawnCount[i, 0] * (pawnCount[i, 0] - 1);
                retVal += 13 * pawnCount[i, 1] * (pawnCount[i, 1] - 1);
            }

            for (int i = 0; i < 7; i++)
            {
                if (pawnCount[i, 0] > 0 && pawnCount[i + 1, 0] > 0)
                    retVal += 10;
                if (pawnCount[i, 1] > 0 && pawnCount[i + 1, 1] > 0)
                    retVal -= 10;
            }

            retVal += 33 * attackPosInfo.WhiteKingShield;
            retVal -= 33 * attackPosInfo.BlackKingShield;

            if (countPerPiece[(int)ChessBoard.PieceType.Bishop] >= 2)
                retVal += 30;
            if (countPerPiece[(int)(ChessBoard.PieceType.Bishop | ChessBoard.PieceType.Black)] >= 2)
                retVal -= 30;
            //int endgame = 55;
            //int figcnt = 0;
            //for (int i = 0; i < countPerPiece.Length; i++)
            //{
            //    figcnt += System.Math.Abs(PiecesPoint[i]) * countPerPiece[i];
            //}
            //figcnt -= 2000000;
            //if (figcnt < 1000)
            //    endgame = -15;
            //for (int i = 0; i < 64; i++)
            //{
            //    if (board[i] == ChessBoard.PieceType.None)
            //        continue;
            //    if ((board[i] & ChessBoard.PieceType.Black) == ChessBoard.PieceType.Black)
            //    {
            //        if (board[i] == (ChessBoard.PieceType.King | ChessBoard.PieceType.Black))
            //            retVal -= (i / 8) * endgame;
            //        else
            //            retVal += (i / 8) * 5;
            //    }
            //    else
            //    {
            //        if (board[i] == ChessBoard.PieceType.King)
            //            retVal -= (7 - i / 8) * endgame;
            //        else
            //            retVal += (7 - i / 8) * 5;
            //    }
            //}

            return retVal;
        }
    } // Class BoardEvaluationTest
} // Namespace
