using System;
using UnityEngine;

namespace ChessEngine.UCIStockfishOpponent.UCI
{
    /// <summary>
    /// A class that holds the definition of a UCI option.
    /// </summary>
    /// Author: Intuitive Gaming Solutions
    [Serializable]
    public class UCIOption
    {
        [Tooltip("The name of the UCI option.")]
        public string name;
        [Tooltip("The type of the UCI option.")]
        public string type;
        [Tooltip("The default value for the UCI option (or null if it has none).")]
        public string defaultValue;
    }
}
