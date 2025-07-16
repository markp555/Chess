using Microsoft.VisualBasic;

namespace GenericSearchEngine {
    /// <summary>
    /// Position information. Positive value for player 1, negative value for player 2.
    /// All these informations are computed before the last move to improve performance.
    /// </summary>
    /// <remarks>
    /// Class Ctor
    /// </remarks>
    /// <param name="piecesAttacked">   Number of pieces attacking this position</param>
    /// <param name="piecesDefending">  Number of pieces defending this position</param>
    public struct AttackPosInfo(int piecesAttacked, int piecesDefending, int[] piecesMobility) {

        /// <summary>
        /// Number of pieces being attacked by player's pieces
        /// </summary>
        public int PiecesAttacked = piecesAttacked;

        /// <summary>
        /// Number of pieces defending player's pieces
        /// </summary>
        public int PiecesDefending = piecesDefending;

        /// <summary>
        /// Number of avaiable moves for each kind of figure
        /// </summary>
        public int[] PiecesMobility = piecesMobility;

        public bool WhiteCheck { get; set; } = false;
        public bool BlackCheck { get; set; } = false;

        public int WhiteKingShield { get; set; } = 0;
        public int BlackKingShield { get; set; } = 0;

        /// <summary>
        /// null attack position
        /// </summary>
        public static AttackPosInfo NullAttackPosInfo { get; } = new AttackPosInfo(0, 0, new int[16]);
    }
}
