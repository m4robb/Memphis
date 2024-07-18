using System;
using UnityEngine.Events;

namespace ChessEngine.UCIStockfishOpponent.UCI
{
    /// <summary>
    /// An event that holds a string, simply input or output to/from the UCI process.
    /// 
    /// Arg0: string - input or output to/from the UCI program.
    /// </summary>
    /// Author: Intuitive Gaming Solutions
    [Serializable]
    public class UCIUnityEvent : UnityEvent<string> { }
}
