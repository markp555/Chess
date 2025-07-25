﻿//#define IterativeActivated
using SrcChess2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;


namespace GenericSearchEngine {

    /// <summary>
    /// Alpha Beta search engine
    /// </summary>
    /// <remarks>
    /// Class constructor
    /// </remarks>
    /// <param name="trace">  Trace object or null</param>
    /// <param name="rnd">    Random object</param>
    /// <param name="rndRep"> Repetitive random object</param>
    public sealed class SearchEngineAlphaBeta<TBoard, TMove> : SearchEngine<TBoard, TMove>
                                                               where TBoard : IGameBoard<TMove>
#if MOVEISCLASS
                                                               where TMove : class {
#else
                                                               where TMove : struct {

#endif
        static int[]? PiecesPoint;

        public SearchEngineAlphaBeta(ISearchTrace<TMove>? trace, Random rnd, Random rndRep) : base(trace, rnd, rndRep)
        {
            PiecesPoint = new int[16];
            PiecesPoint[(int)ChessBoard.PieceType.Pawn] = 100;
            PiecesPoint[(int)ChessBoard.PieceType.Rook] = 500;
            PiecesPoint[(int)ChessBoard.PieceType.Knight] = 300;
            PiecesPoint[(int)ChessBoard.PieceType.Bishop] = 325;
            PiecesPoint[(int)ChessBoard.PieceType.Queen] = 900;
            PiecesPoint[(int)ChessBoard.PieceType.King] = 1000000;
            PiecesPoint[(int)(ChessBoard.PieceType.Pawn | ChessBoard.PieceType.Black)] = 100;
            PiecesPoint[(int)(ChessBoard.PieceType.Rook | ChessBoard.PieceType.Black)] = 500;
            PiecesPoint[(int)(ChessBoard.PieceType.Knight | ChessBoard.PieceType.Black)] = 300;
            PiecesPoint[(int)(ChessBoard.PieceType.Bishop | ChessBoard.PieceType.Black)] = 325;
            PiecesPoint[(int)(ChessBoard.PieceType.Queen | ChessBoard.PieceType.Black)] = 900;
            PiecesPoint[(int)(ChessBoard.PieceType.King | ChessBoard.PieceType.Black)] = 1000000;
        }

        /// <summay>
        /// Alpha Beta pruning function.
        /// </summary>
        /// <param name="moveListPlayerId">   Player doing the move</param>
        /// <param name="board">              Chess board</param>
        /// <param name="moveList">           Move list</param>
        /// <param name="alpha">              Alpha limit</param>
        /// <param name="beta">               Beta limit</param>
        /// <param name="minMaxInfo">         MinMax information</param>
        /// <param name="bestMovePos">        Best move position or -1 if none found</param>
        /// <param name="ptsPerMove">         Points per move if not null</param>
        /// <param name="evaluatedMoveCount"> Number of moves evaluated</param>
        /// <param name="isFullyEvaluated">   true if all moves has been evaluated</param>
        /// <param name="maximizing">         Maximizing</param>
        /// <returns>
        /// Points to give for this move or int.MinValue for timed out
        /// </returns>
        private static int AlphaBeta(int            moveListPlayerId,
                                     TBoard         board,
                                     List<TMove>    moveList,
                                     int            alpha,
                                     int            beta,
                                     ref MinMaxInfo minMaxInfo,
                                     out TMove?      bestMovePos,
                                     int[]?         ptsPerMove,
                                     out int        evaluatedMoveCount,
                                     out bool       isFullyEvaluated,
                                     bool           maximizing) { 
            int         value;
            int         lastMovePlayerId;
            List<TMove> childMoveList;
            int         boardExtraInfo;

            evaluatedMoveCount     = 0;
            bestMovePos            = null;
            lastMovePlayerId       = 1 - moveListPlayerId;
            minMaxInfo.HasTimedOut = DateTime.Now >= minMaxInfo.TimeOut;
            isFullyEvaluated       = true;

            int[] eatBalance = board.CalculateAttackMap(out int[] minAttack);

            moveList.Sort((TMove a, TMove b) => {
                if (a is not Move am || b is not Move bm || board is not ChessGameBoardAdaptor brd || PiecesPoint == null)
                    return 0;
                int ap = (int)brd.ChessBoard[am.StartPos];
                int bp = (int)brd.ChessBoard[bm.StartPos];
                int w1 = am.OriginalPiece == ChessBoard.PieceType.None ? 0 : PiecesPoint[(int)am.OriginalPiece];
                int w2 = bm.OriginalPiece == ChessBoard.PieceType.None ? 0 : PiecesPoint[(int)bm.OriginalPiece];
                if (eatBalance[am.StartPos] < 0) // this piece can be eaten
                {
                    w2 -= PiecesPoint[ap];
                }
                else if (minAttack[am.StartPos] < ap)
                {
                    w2 -= PiecesPoint[ap];
                    w2 += PiecesPoint[minAttack[am.StartPos]];
                }
                if (eatBalance[bm.StartPos] < 0)
                {
                    w1 -= PiecesPoint[bp];
                }
                else if (minAttack[bm.StartPos] < bp)
                {
                    w1 -= PiecesPoint[bp];
                    w1 += PiecesPoint[minAttack[bm.StartPos]];
                }
                if (eatBalance[am.EndPos] <= 0)
                {
                    w1 -= PiecesPoint[ap];
                }
                else if (minAttack[am.StartPos] < ap)
                {
                    w1 -= PiecesPoint[ap];
                    w1 += PiecesPoint[minAttack[am.StartPos]];
                }
                if (eatBalance[bm.EndPos] <= 0)
                {
                    w2 -= PiecesPoint[bp];
                }
                else if (minAttack[bm.StartPos] < bp)
                {
                    w2 -= PiecesPoint[bp];
                    w2 += PiecesPoint[minAttack[bm.StartPos]];
                }
                // if it is better to make second move (by take)
                if (w1 != w2)
                {
                    return w2 - w1;
                }
                int prob1 = eatBalance[am.EndPos];
                int prob2 = eatBalance[bm.EndPos];
                //if (!maximizing)
                //{
                //    prob1 *= -1;
                //    prob2 *= -1;
                //}
                return prob2 - prob1;
                //if ((am.OriginalPiece == ChessBoard.PieceType.None) != (bm.OriginalPiece == ChessBoard.PieceType.None))
                //{
                //    if (am.OriginalPiece == ChessBoard.PieceType.None)
                //        return 1;
                //    return -1;
                //}
                //if (am.OriginalPiece != ChessBoard.PieceType.None && bm.OriginalPiece != ChessBoard.PieceType.None)
                //{
                //    int w1 = PiecesPoint[(int)am.OriginalPiece] - PiecesPoint[(int)brd.ChessBoard[(int)am.StartPos]];
                //    int w2 = PiecesPoint[(int)bm.OriginalPiece] - PiecesPoint[(int)brd.ChessBoard[(int)bm.StartPos]];
                //    return w2 - w1;
                //}
                //bool ap = (brd.ChessBoard[am.StartPos] & ChessBoard.PieceType.PieceMask) == ChessBoard.PieceType.Pawn;
                //bool bp = (brd.ChessBoard[bm.StartPos] & ChessBoard.PieceType.PieceMask) == ChessBoard.PieceType.Pawn;
                //ChessBoard.PieceType enemyPawn = ChessBoard.PieceType.Pawn | (brd.ChessBoard[am.StartPos] ^ (brd.ChessBoard[am.StartPos] & ChessBoard.PieceType.PieceMask) ^ ChessBoard.PieceType.Black);
                //bool aeaten = brd.ChessBoard.CheckIsPiece(am.StartPos + 7, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(am.StartPos + 9, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(am.StartPos - 7, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(am.StartPos - 9, enemyPawn);
                //bool beaten = brd.ChessBoard.CheckIsPiece(bm.StartPos + 7, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(bm.StartPos + 9, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(bm.StartPos - 7, enemyPawn) ||
                //brd.ChessBoard.CheckIsPiece(bm.StartPos - 9, enemyPawn);
                //if (ap && bp)
                //    return 0;
                //if (ap)
                //    return -1;
                //if (bp)
                //    return 1;
                //if (aeaten && beaten)
                //    return 0;
                //if (aeaten)
                //    return 1;
                //if (beaten)
                //    return -1;
                ////if (brd.ChessBoard.IsCheck)
                //return 0;
            });

            if (!board.IsMoveTerminal(moveListPlayerId,
                                      moveList,
                                      minMaxInfo,
                                      isSearchHasBeenCanceled: minMaxInfo.HasTimedOut || IsSearchHasBeenCanceled,
                                      out int retVal, 
                                      false)) {
                retVal = maximizing ? int.MinValue : int.MaxValue;
                
                foreach (TMove move in moveList) {
                    if (minMaxInfo.HasTimedOut || IsSearchHasBeenCanceled)
                    {
                        // Fix queen bug
                        break;
                    }
                    if (board.DoMoveNoLog(move)) {
                        boardExtraInfo = board.ComputeBoardExtraInfo();
                        value          = minMaxInfo.TransTable?.ProbeEntry(moveListPlayerId, board.ZobristKey, boardExtraInfo, minMaxInfo.Depth - 1) ?? int.MaxValue;
                        if (value == int.MaxValue) {
                            //minMaxInfo.
                            childMoveList = board.GetMoves(lastMovePlayerId, out/*AttackPosInfo _*/minMaxInfo.attackPosInfo);
                            bool wasCheck = false;
                            // wasCheck = board.IsCheck();
                            if (!wasCheck)
                                minMaxInfo.Depth--;
                            value = AlphaBeta(lastMovePlayerId,
                                              board,
                                              childMoveList,
                                              alpha,
                                              beta,
                                              ref minMaxInfo,
                                              bestMovePos: out TMove? _,
                                              ptsPerMove:  null,
                                              evaluatedMoveCount: out int _,
                                              out bool isChildIsFullyEvaluated,
                                              !maximizing);
                            if (!wasCheck)
                                minMaxInfo.Depth++;
                            if (board.IsWinningPts(value) && isChildIsFullyEvaluated) { // Can only be if this is coming directly from Terminal
                                isChildIsFullyEvaluated = false;
                            }
                            if (isChildIsFullyEvaluated) {
                                minMaxInfo.TransTable?.RecordEntry(moveListPlayerId, board.ZobristKey, boardExtraInfo, value, minMaxInfo.Depth);
                            }
                            isFullyEvaluated &= isChildIsFullyEvaluated;
                        }
                        board.UndoMoveNoLog(move);
                    } else {
                        value = 0; // draw
                        board.UndoMoveNoLog(move);
                    }
                    if (ptsPerMove != null) {
                        ptsPerMove[evaluatedMoveCount] = value;
                    }
                    
                    if (maximizing) {
                        if (value > retVal) {
                            retVal      = value;
                            bestMovePos = move;
                            if (retVal >= beta) {
                                evaluatedMoveCount++;
                                if (evaluatedMoveCount < moveList.Count) {
                                    isFullyEvaluated = false;
                                }
                                break;
                            }
                            alpha = Math.Max(alpha, retVal);
                        }
                    } else {
                        if (value < retVal) {
                            retVal      = value;
                            bestMovePos = move;
                            if (value <= alpha) {
                                evaluatedMoveCount++;
                                if (evaluatedMoveCount < moveList.Count) {
                                    isFullyEvaluated = false;
                                }
                                break;
                            }
                            beta = Math.Min(beta, retVal);
                        }
                    }
                    evaluatedMoveCount++;
                }
            }
            //else if (minMaxInfo.Depth == 0)
            //{
            //    int old = retVal;
            //    do
            //    {
            //        if (maximizing)
            //        {
            //            if (retVal >= beta)
            //            {
            //                retVal = beta;
            //                break;
            //            }
            //            if (retVal > alpha)
            //                alpha = retVal;
            //        }
            //        else
            //        {
            //            if (retVal <= alpha)
            //            {
            //                retVal = alpha;
            //                break;
            //            }
            //            if (retVal < beta)
            //                beta = retVal;
            //        }
            //        retVal = int.MaxValue;
            //        foreach (TMove move in moveList)
            //        {
            //            if (move is Move mv && mv.OriginalPiece == ChessBoard.PieceType.None)
            //                continue;
            //            if (board.DoMoveNoLog(move))
            //            {
            //                boardExtraInfo = board.ComputeBoardExtraInfo();
            //                childMoveList = board.GetMoves(lastMovePlayerId, out minMaxInfo.attackPosInfo);
            //                value = AlphaBeta(lastMovePlayerId,
            //                                    board,
            //                                    childMoveList,
            //                                    alpha,
            //                                    beta,
            //                                    ref minMaxInfo,
            //                                    bestMovePos: out TMove? _,
            //                                    ptsPerMove: null,
            //                                    evaluatedMoveCount: out int _,
            //                                    out bool isChildIsFullyEvaluated,
            //                                    !maximizing);
            //                board.UndoMoveNoLog(move);
            //            }
            //            else
            //            {
            //                value = 0; // draw
            //                board.UndoMoveNoLog(move);
            //            }
            //            if (maximizing)
            //            {
            //                if (value >= beta)
            //                {
            //                    retVal = beta;
            //                    break;
            //                }
            //                if (value > alpha)
            //                    alpha = value;
            //            }
            //            else
            //            {
            //                if (value <= alpha)
            //                {
            //                    retVal = alpha;
            //                    break;
            //                }
            //                if (value < beta)
            //                    beta = retVal;
            //            }
            //            evaluatedMoveCount++;
            //        }
            //        if (retVal != int.MaxValue)
            //            break;
            //        if (maximizing)
            //            retVal = Math.Max(alpha, old);
            //        else
            //            retVal = Math.Min(beta, old);
            //    } while (false);
            //    if (maximizing)
            //        retVal = Math.Max(retVal, old);
            //    else
            //        retVal = Math.Min(retVal, old);
            //}

            return retVal;
        }

        /// <summary>
        /// Find the best move for a player using alpha-beta for a given depth
        /// </summary>
        /// <param name="board">               Chess board</param>
        /// <param name="searchEngineSetting"> Search mode</param>
        /// <param name="transTable">          Transposition table or null if not using one</param>
        /// <param name="playerId">            Color doing the move</param>
        /// <param name="moveList">            List of move to try</param>
        /// <param name="player1PosInfo">      Information about pieces attacks for the white</param>
        /// <param name="player2PosInfo">      Information about pieces attacks for the black</param>
        /// <param name="maxDepth">            Maximum depth</param>
        /// <param name="totalMoveCount">      Total list of moves</param>
        /// <param name="alpha">               Alpha bound</param>
        /// <param name="beta">                Beta bound</param>
        /// <param name="timeOut">             Time limit (DateTime.MaxValue for no time limit)</param>
        /// <param name="maximizing">          true for maximizing, false for minimizing</param>
        /// <param name="ptsPerMove">          Points for each move in the MoveList</param>
        /// <param name="evaluatedMoveCount">  Number of moves evaluated</param>
        /// <param name="bestMove">            Index of the best move</param>
        /// <param name="hasTimedOut">         Return true if time out</param>
        /// <param name="bestMovePts">         Return the best move point</param>
        /// <param name="permCount">           Total permutation evaluated</param>
        /// <returns>
        /// true if a best move has been found
        /// </returns>
        private bool FindBestMoveUsingAlphaBetaAtDepth(TBoard              board,
                                                       SearchEngineSetting searchEngineSetting,
                                                       TransTable?         transTable,
                                                       int                 playerId,
                                                       List<TMove>         moveList,
                                                       AttackPosInfo       player1PosInfo,
                                                       AttackPosInfo       player2PosInfo,
                                                       int                 maxDepth,
                                                       int                 totalMoveCount,
                                                       int                 alpha,
                                                       int                 beta,
                                                       DateTime            timeOut,
                                                       bool                maximizing,
                                                       int[]?              ptsPerMove,
                                                       out int             evaluatedMoveCount,
#if MOVEISCLASS
                                                       out TMove?          bestMove,
#else
                                                       out TMove           bestMove,
#endif
                                                       out bool            hasTimedOut,
                                                       out int             bestMovePts,
                                                       out int             permCount) {
            bool       retVal = false;
            MinMaxInfo minMaxInfo;
            int        player1MoveCount;
            int        player2MoveCount;

            if (playerId == PlayerId1) {
                player1MoveCount = totalMoveCount;
                player2MoveCount = 0;
            } else {
                player1MoveCount = 0;
                player2MoveCount = totalMoveCount;
            }
            minMaxInfo = new MinMaxInfo(searchEngineSetting, transTable) {
                PermCount            = 0,
                Depth                = maxDepth,
                MaxDepth             = maxDepth,
                TimeOut              = timeOut,
                HasTimedOut          = false,
                Player1AttackPosInfo = player1PosInfo,
                Player2AttackPosInfo = player2PosInfo,
                Player1MoveCount     = player1MoveCount,
                Player2MoveCount     = player2MoveCount,
                CapturesDepth = 10
            };
            bestMovePts = AlphaBeta(playerId,
                                    board,
                                    moveList,
                                    alpha,
                                    beta,
                                    ref minMaxInfo,
                                    out TMove? bestMovePos,
                                    ptsPerMove,
                                    out evaluatedMoveCount,
                                    isFullyEvaluated: out bool _,
                                    maximizing);
            if (bestMovePos !=  null) {
                bestMove = (TMove)bestMovePos;
                retVal   = true;
                LogSearchTrace(maxDepth, playerId, bestMove, bestMovePts);
            } else {
                bestMove = board.CreateEmptyMove();
            }
            permCount   = minMaxInfo.PermCount;
            hasTimedOut = minMaxInfo.HasTimedOut;
            return retVal;
        }

        /// <summary>
        /// Find the best move for a player using alpha-beta pruning. 
        /// One or more thread can execute this method at the same time.
        /// Handle the search method:
        ///     Fix depth,
        ///     Iterative fix depth 
        ///     Iterative depth with time limit
        /// </summary>
        /// <param name="board">               Chess board</param>
        /// <param name="searchEngineSetting"> Search mode</param>
        /// <param name="transTable">          Translation table if any</param>
        /// <param name="playerId">            Color doing the move</param>
        /// <param name="moveList">            List of move to try</param>
        /// <param name="player1PosInfo">      Information about pieces attacks for the white</param>
        /// <param name="player2PosInfo">      Information about pieces attacks for the black</param>
        /// <param name="totalMoveCount">      Total number of moves</param>
        /// <param name="alpha">               Alpha bound</param>
        /// <param name="beta">                Beta bound</param>
        /// <param name="maximizing">          true for maximizing, false for minimizing</param>
        /// <returns>
        /// Points
        /// </returns>
        private MinMaxResult<TMove> FindBestMoveUsingAlphaBetaAsync(TBoard              board,
                                                                    SearchEngineSetting searchEngineSetting,
                                                                    TransTable?         transTable,
                                                                    int                 playerId,
                                                                    List<TMove>         moveList,
                                                                    AttackPosInfo       player1PosInfo,
                                                                    AttackPosInfo       player2PosInfo,
                                                                    int                 totalMoveCount,
                                                                    int                 alpha,
                                                                    int                 beta,
                                                                    bool                maximizing) {
            MinMaxResult<TMove> retVal;
#if IterativeActivated
            DateTime            timeOut;
            int                 maxDepth;
            int                 depthLimit;
            int[]               ptsPerMove;
            bool                hasTimedOut;
#endif
            ThreadPriority      threadPriority;
            bool                bestMoveFound;
            bool                isIterativeDepthFirst;

            threadPriority                = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            isIterativeDepthFirst         = searchEngineSetting.SearchOption.HasFlag(SearchOption.UseIterativeDepthSearch);
            retVal = new MinMaxResult<TMove> {
                BestMoveFound = false,
                BestMove      = board.CreateEmptyMove()
            };
            try {
                retVal.PermCount = 0;
#if IterativeActivated
                if (searchEngineSetting.SearchDepth == 0 || isIterativeDepthFirst) {
                    // Iterative Depth (with limit time or fixed maximum iteration)
                    ptsPerMove    = new int[moveList.Count];
                    timeOut       = (isIterativeDepthFirst) ? DateTime.MaxValue : 
                                                              DateTime.Now + TimeSpan.FromSeconds(searchEngineSetting.TimeOutInSec);
                    depthLimit    = (isIterativeDepthFirst) ? searchEngineSetting.SearchDepth : 999;
                    maxDepth      = 1;
                    bestMoveFound = FindBestMoveUsingAlphaBetaAtDepth(board,
                                                                      searchEngineSetting,
                                                                      transTable,
                                                                      playerId,
                                                                      moveList,
                                                                      player1PosInfo,
                                                                      player2PosInfo,
                                                                      maxDepth,
                                                                      totalMoveCount,
                                                                      alpha,
                                                                      beta,
                                                                      timeOut: DateTime.MaxValue,
                                                                      maximizing,
                                                                      ptsPerMove,
                                                                      out int evaluatedMoveCount,
                                                                      out TMove bestMove,
                                                                      hasTimedOut: out bool _,
                                                                      out int pts,
                                                                      out int permCountAtLevel);
                    if (bestMoveFound) {
                        retVal.BestMoveFound = true;
                        retVal.BestMove      = bestMove;
                        retVal.Pts           = pts;
                        retVal.MaxDepth      = maxDepth;
                    }
                    retVal.PermCount += permCountAtLevel;
                    hasTimedOut       = false;
                    while (DateTime.Now < timeOut && !SearchCancelState && !hasTimedOut && maxDepth < depthLimit) {
                        moveList = SortMoveList(moveList, ptsPerMove, evaluatedMoveCount);
                        maxDepth++;
                        bestMoveFound = FindBestMoveUsingAlphaBetaAtDepth(board,
                                                                          searchEngineSetting,
                                                                          transTable,
                                                                          playerId,
                                                                          moveList,
                                                                          player1PosInfo,
                                                                          player2PosInfo,
                                                                          maxDepth,
                                                                          totalMoveCount,
                                                                          alpha,
                                                                          beta,
                                                                          timeOut,
                                                                          maximizing,
                                                                          ptsPerMove,
                                                                          out evaluatedMoveCount,
                                                                          out bestMove,
                                                                          out hasTimedOut,
                                                                          out pts,
                                                                          out permCountAtLevel);
                        if (bestMoveFound && !hasTimedOut) {
                            retVal.BestMoveFound = true;
                            retVal.BestMove      = bestMove;
                            retVal.Pts           = pts;
                            retVal.MaxDepth      = maxDepth;
                        }
                        retVal.PermCount += permCountAtLevel;
                    }
                } else {
#endif
                
                // Fixed Maximum Depth
                retVal.MaxDepth = searchEngineSetting.SearchDepth;
                    bestMoveFound   = FindBestMoveUsingAlphaBetaAtDepth(board,
                                                                        searchEngineSetting,
                                                                        transTable,
                                                                        playerId,
                                                                        moveList,
                                                                        player1PosInfo,
                                                                        player2PosInfo,
                                                                        retVal.MaxDepth,
                                                                        totalMoveCount,
                                                                        alpha,
                                                                        beta,
                                                                        (searchEngineSetting.TimeOutInSec == 0) ? DateTime.MaxValue :
                                                              DateTime.Now + TimeSpan.FromSeconds(searchEngineSetting.TimeOutInSec),
                                                                        maximizing,
                                                                        ptsPerMove: null,
                                                                        evaluatedMoveCount: out int _,
#if MOVEISCLASS
                                                                        out TMove? bestMove,
#else
                                                                        out TMove bestMove,
#endif
                                                                        hasTimedOut: out bool timeOut,
                                                                        out int pts,
                                                                        out int permCountAtLevel);
                    if (bestMoveFound) {
                        retVal.BestMoveFound = true;
                        retVal.BestMove      = bestMove;
                        retVal.Pts           = pts;
                        retVal.HasTimedOut   = timeOut;
                    }
                    retVal.PermCount += permCountAtLevel;
#if IterativeActivated
            }
#endif
            } finally {
                Thread.CurrentThread.Priority = threadPriority;
            }
            return (retVal);
        }

        /// <summary>
        /// Find the best move using Alpha Beta pruning.
        /// Handle the number of thread assignment for the search.
        /// </summary>
        /// <param name="board">               Board</param>
        /// <param name="searchEngineSetting"> Search mode</param>
        /// <param name="playerId">            Player doing the move</param>
        /// <param name="isMaximizing">        true if starting with maximizing</param>
        /// <param name="moveList">            Move list</param>
        /// <param name="attackPosInfo">       Attack/defense position info</param>
        /// <param name="bestMove">            Best move found</param>
        /// <param name="permCount">           Total permutation evaluated</param>
        /// <param name="cacheHit">            Number of moves found in the translation table cache</param>
        /// <param name="maxDepth">            Maximum depth used</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        protected override bool FindBestMove(TBoard              board,
                                             SearchEngineSetting searchEngineSetting,
                                             int                 playerId,
                                             bool                isMaximizing,
                                             List<TMove>         moveList,
                                             AttackPosInfo       attackPosInfo,
#if MOVEISCLASS
                                             out TMove?          bestMove,
#else
                                             ref TMove           bestMove,
#endif
                                             out int             permCount,
                                             out long            cacheHit,
                                             out int             maxDepth) { 
            bool                        retVal = false;
            TBoard[]                    boards;
            TransTable?                 transTable;
            Task<MinMaxResult<TMove>>[] tasks;
            List<TMove>[]               taskMoveList;
            MinMaxResult<TMove>         minMaxRes;
            AttackPosInfo               player1PosInfo;
            AttackPosInfo               player2PosInfo;
            int                         threadCount;
            int                         moveListPos;
            int                         moveListCount;
            int                         movePerTask;
            int                         overflowCount;
            int                         pts;
            bool                        maximizing;

#if MOVEISCLASS
            bestMove = null;
#endif
            if (playerId == PlayerId1) {
                player1PosInfo = attackPosInfo;
                player2PosInfo = AttackPosInfo.NullAttackPosInfo;
            } else {
                player1PosInfo = AttackPosInfo.NullAttackPosInfo;
                player2PosInfo = attackPosInfo;
            }
            maximizing    = isMaximizing;
            permCount     = 0;
            transTable    = board.GetTransTable(searchEngineSetting);
            transTable?.ResetCacheHit();
            moveListCount = moveList.Count;
            threadCount   = searchEngineSetting.ThreadingMode == ThreadingMode.OnePerProcessorForSearch ?
                            Math.Min(Environment.ProcessorCount, moveListCount) : 1;
            if (threadCount > 1) {
                boards        = new TBoard[threadCount];
                taskMoveList  = new List<TMove>[threadCount];
                tasks         = new Task<MinMaxResult<TMove>>[threadCount];
                moveListPos   = 0;
                movePerTask   = moveListCount / threadCount;
                overflowCount = moveListCount % threadCount;
                for (int i = 0; i < threadCount; i++) {
                    boards[i]       = (TBoard)board.Clone();
                    taskMoveList[i] = new List<TMove>(movePerTask);
                    for (int j = 0; j < movePerTask; j++) {
                        taskMoveList[i].Add(moveList[moveListPos++]);
                    }
                    if (overflowCount != 0) {
                        taskMoveList[i].Add(moveList[moveListPos++]);
                        overflowCount--;
                    }
                }
                for (int i = 0; i < threadCount; i++) {
                    tasks[i] = Task<MinMaxResult<TMove>>.Factory.StartNew((param) => {
                        int step = (int)param!;
                        return (FindBestMoveUsingAlphaBetaAsync(boards[step],
                                                                searchEngineSetting,
                                                                transTable,
                                                                playerId,
                                                                taskMoveList[step],
                                                                player1PosInfo,
                                                                player2PosInfo,
                                                                moveList.Count,
                                                                alpha: -10000000,
                                                                beta:   10000000,
                                                                maximizing));
                    }, i);
                }
                SetRunningTasks(tasks);
                pts      = maximizing ? int.MinValue : int.MaxValue;
                maxDepth = 999;
                for (int step = 0; step < threadCount; step++) {
                    minMaxRes = tasks[step].Result;
                    if (minMaxRes.BestMoveFound) {
                        permCount += minMaxRes.PermCount;
                        maxDepth   = Math.Min(maxDepth, minMaxRes.MaxDepth);
                        if (maximizing) {
                            if (minMaxRes.Pts > pts) {
                                pts      = minMaxRes.Pts;
                                bestMove = minMaxRes.BestMove;
                                retVal   = true;
                            }
                        } else {
                            if (minMaxRes.Pts < pts) {
                                pts      = minMaxRes.Pts;
                                bestMove = minMaxRes.BestMove;
                                retVal   = true;
                            }
                        }
                    }
                }
                if (maxDepth == 999) {
                    maxDepth = -1;
                }
                SetRunningTasks(tasks: null);
            } else {
                TBoard      chessBoardTmp;

                chessBoardTmp = (TBoard)board.Clone();
                minMaxRes     = FindBestMoveUsingAlphaBetaAsync(chessBoardTmp,
                                                                searchEngineSetting,
                                                                transTable,
                                                                playerId,
                                                                moveList,
                                                                player1PosInfo,
                                                                player2PosInfo,
                                                                moveList.Count,
                                                                alpha: -10000000,
                                                                beta:   10000000,
                                                                maximizing);
                permCount = minMaxRes.PermCount;
                maxDepth  = minMaxRes.MaxDepth;
                if (minMaxRes.BestMoveFound) {
                    bestMove = minMaxRes.BestMove;
                    retVal = true;
                }
            }
            if (!board.IsMoveValid(bestMove))
                Debugger.Break();
            cacheHit = transTable?.CacheHit ?? 0;
            return retVal;
        }
    } // Class SearchEngineAlphaBeta
} // Namespace
