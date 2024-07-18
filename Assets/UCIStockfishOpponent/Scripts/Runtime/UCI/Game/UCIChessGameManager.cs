using UnityEngine;
using System.Collections.Generic;
using ChessEngine.Game;

namespace ChessEngine.UCIStockfishOpponent.UCI
{
    /// <summary>
    /// An implementation of ChessGameManager for games that use UCI engines for AI.
    /// </summary>
    /// Author: Intuitive Gaming Solutions
    public class UCIChessGameManager : ChessGameManager
    {
        [Header("Settings - UCI")]
        [Tooltip("A reference to the UCIProcess associated with this component.")]
        public UCIProcess process;

        [Header("Settings - AI")]
        [Tooltip("Enable AI to play for white team?")]
        public bool enableWhiteAI;
        [Tooltip("Enable AI to play for black team?")]
        public bool enableBlackAI;
        [Min(0)]
        [Tooltip("The exact number of milliseconds AI should think for, or 0 to let the UCI engine decide. (Default: 0 - let UCI engine decide, no exact time)")]
        public uint aiThinkTime = 0;
        [Min(1)]
        [Tooltip("The think depth for the AI. (Default: 10 | Minimum: 1)")]
        public uint aiThinkDepth = 10;

        // Unity callback(s).
        protected virtual void Awake()
        {
            // Ensure there is a valid 'UCIProcess' reference.
            if (process == null)
                process = GetComponent<UCIProcess>();
            if (process == null)
                Debug.LogWarning("UCIChessGameManager is missing a 'process' reference! Make sure to reference a UCIProcess or have one on the same GameObject as the UCIChessGameManager component.", gameObject);
        }

        protected override void OnEnable()
        {
            // Invoke the base class 'OnEnable' callback.
            base.OnEnable();

            // Subscribe to own event(s).
            PostGameReset.AddListener(OnPostGameReset);

            // Subscribe to UCI event(s).
            process.UCIOkReceived.AddListener(OnUCIOkReceived);
            process.UCIBestMoveReceived.AddListener(OnUCIBestMoveReceived);
        }

        protected override void OnDisable()
        {
            // Invoke the base class 'OnDisable' callback.
            base.OnDisable();

            // Unsubscribe from own event(s).
            PostGameReset.RemoveListener(OnPostGameReset);

            // Unsubscribe from UCI event(s).
            process.UCIOkReceived.RemoveListener(OnUCIOkReceived);
            process.UCIBestMoveReceived.RemoveListener(OnUCIBestMoveReceived);
        }

        // Public method(s).
        /// <summary>Simulates UCI move with current settings effectively performing the next move by getting a 'bestmove' command from the UCI.</summary>
        public void PerformUCIBestMove()
        {
            // Simulate turn on UCI.
            if (aiThinkTime == 0)
            {
                process.SendInput("go depth " + aiThinkDepth);
            }
            else { process.SendInput("go depth " + aiThinkDepth + " movetime " + aiThinkTime); }
        }

        /// <summary>Sets the 'enableWhiteAI' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pEnabled"></param>
        public void SetEnableWhiteAI(bool pEnabled) { enableWhiteAI = pEnabled; }
        /// <summary>Sets the 'enableBlackAI' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pEnabled"></param>
        public void SetEnableBlackAI(bool pEnabled) { enableBlackAI = pEnabled; }
        /// <summary>Sets the 'aiThinkTime' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pMilliseconds"></param>
        public void SetAIThinkTime(uint pMilliseconds) { aiThinkTime = pMilliseconds; }
        /// <summary>Sets the 'aiThinkDepth' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pDepth"></param>
        public void SetAIThinkDepth(uint pDepth) { aiThinkDepth = pDepth; }

        // Public override method(s).
        /// <summary>
        /// Override piece selection check method to disallow selections of other team and while game is not started.
        /// </summary>
        /// <param name="pPiece"></param>
        /// <param name="pTile"></param>
        /// <returns>true of the piece can be selected, otherwise false.</returns>
        public override bool CanSelectPiece(VisualChessPiece pPiece, VisualChessTableTile pTile)
        {
            // Cannot select piece til UCI is ok.
            if (!process.IsUCIOk)
                return false;

            // Cannot select piece on AI turn.
            if (ChessInstance.turn == ChessColor.White)
            {
                if (enableWhiteAI)
                    return false;
            }
            else if (enableBlackAI) { return false; }

            // Check if the piece may be selected.
            return base.CanSelectPiece(pPiece, pTile);
        }
        // Protected method(s).
        /// <summary>
        /// Moves using a 4 character string describing the move. (e.g: e2e4)
        /// </summary>
        /// <param name="pMove">The 4 character string describing the move. (e.g: e2e4)</param>
        protected void MoveByDescription(string pMove)
        {
            // Get move IDs.
            string moveFromID = pMove.Substring(0, 2);
            string moveToID = pMove.Substring(2, 2);

            // Get move tiles.
            ChessTableTile moveFromTile = ChessInstance.Table.GetTileByID(moveFromID);
            if (moveFromTile != null)
            {
                ChessTableTile moveToTile = ChessInstance.Table.GetTileByID(moveToID);
                if (moveToTile != null)
                {
                    // Ensure there is a chess piece on the 'from' tile.
                    ChessPiece piece = moveFromTile.GetPiece();
                    if (piece != null)
                    {
                        // If we have a valid, occupied tile selected handle movement of the piece on it.
                        List<ChessTableTile> validMoves = piece.GetValidMoves();
                        List<AttackInfo> validAttacks = piece.GetValidAttacks();
                        if (validMoves.Contains(moveToTile) || ChessTableTile.IsTileAttackable(validAttacks, moveToTile))
                        {
                            // Don't allow us to capture a king.
                            ChessPiece moveToPiece = moveToTile.GetPiece();
                            if (moveToPiece == null || !(moveToPiece is King))
                            {
                                // Move the 'piece' to the 'move to tile'.
                                MoveInfo moveInfo = piece.Move(moveToTile.TileIndex, moveToTile.GetPiece());

                                // Reset selection.
                                Deselect();

                                // End the turn.
                                ChessInstance.EndTurn(moveInfo);
                            }
                            else { Debug.LogWarning("UCIChessGameManager failed to carry out move '" + pMove + "'! The chess piece on 'from tile' is trying to capture a king.", gameObject); }
                        }
                        else { Debug.LogWarning("UCIChessGameManager failed to carry out move '" + pMove + "'! The move is not valid for the chess piece on 'from tile'.", gameObject); }
                    }
                    else { Debug.LogWarning("UCIChessGameManager failed to carry out move '" + pMove + "'! No chess piece on 'from tile'.", gameObject); }
                }
                else { Debug.LogWarning("UCIChessGameManager failed to carry out move '" + pMove + "'! Unable to find 'to tile'.", gameObject); }
            }
            else { Debug.LogWarning("UCIChessGameManager failed to carry out move '" + pMove + "'! Unable to find 'from tile'.", gameObject); }
        }

        /// <summary>Send the UCI command(s) for starting a new game to the engine.</summary>
        protected void UCI_Command_NewGame()
        {
            process.SendInput("ucinewgame");
            process.SendInput("position startpos");
        }

        // Protected override callback(s).
        /// <summary>
        /// Invoked when a turn is started.
        /// </summary>
        /// <param name="pTurn">The color whose turn was started.</param>
        protected override void OnTurnStarted(ChessColor pTurn)
        {
            // Invoke the base class' turn started event.
            base.OnTurnStarted(pTurn);

            // Only run AI if UCI is ok.
            if (process.IsUCIOk)
            {
                // If an AI turn started simulate the turn.
                if ((pTurn == ChessColor.White && enableWhiteAI) || (pTurn == ChessColor.Black && enableBlackAI))
                {
                    // Move using the UCI.
                    PerformUCIBestMove();
                }
            }
        }

        /// <summary>
        /// Invoked when a turn is ended.
        /// </summary>
        /// <param name="pLastTurn">The ChessColor whose turn ended.</param>
        /// <param name="pMove">The move the turn ended on.</param>
        protected override void OnTurnEnded(ChessColor pLastTurn, MoveInfo pMove)
        {
            // Invoke the base class' turn ended event.
            base.OnTurnEnded(pLastTurn, pMove);

            // Update UCI position with fen string.
            process.SendInput("position fen " + ChessInstance.GenerateFENString());
        }

        /// <summary>Invoked when a game over event occurs.</summary>
        /// <param name="pTurn"></param>
        /// <param name="pReason"></param>
        protected override void OnGameOver(ChessColor pTurn, GameOverReason pReason)
        {
            // Run default game over stuff.
            base.OnGameOver(pTurn, pReason);
        }

        // Protected virtual callback(s).
        /// <summary>Invoked whenever the game is reset.</summary>
        protected override void OnPostGameReset()
        {
            // Invoke the base class method.
            base.OnPostGameReset();

            // Start new game via UCI if UCI is ok.
            if (process.IsUCIOk)
            {
                // Invoke UCI new game command.
                UCI_Command_NewGame();
            }
        }

        /// <summary>Invoked whenever the 'uciok' command is received.</summary>
        protected virtual void OnUCIOkReceived()
        {
            // Start a new game on the UCI engine.
            UCI_Command_NewGame();

            // If an AI turn started simulate the turn.
            if ((ChessInstance.turn == ChessColor.White && enableWhiteAI) || (ChessInstance.turn == ChessColor.Black && enableBlackAI))
            {
                // Move using the UCI.
                PerformUCIBestMove();
            }
        }

        /// <summary>Invoked whenever a 'bestmove' command is received from the UCI process.</summary>
        /// <param name="pMove">A 4 character string representing the move. (e.g: e2e4)</param>
        protected virtual void OnUCIBestMoveReceived(string pMove)
        {
            // Perform the move.
            MoveByDescription(pMove);
        }
    }
}
