namespace ChessEngine.UCIStockfishOpponent
{
    /// <summary>
    /// A public static class that validates Chess notation related inputs.
    /// </summary>
    /// Author: Intuitive Gaming Solutions
    public static class InputValidation
    {
        #region Column Rank Input Validation
        /// <summary>Returns true if the specified character represents a valid column (a through h), otherwise false.</summary>
        /// <param name="pCharacter"></param>
        /// <returns>true if the specified character represents a valid column (a through h), otherwise false.</returns>
        public static bool IsCharacterValidColumn(char pCharacter)
        {
            return pCharacter >= 'a' && pCharacter <= 'h';
        }

        /// <summary>Returns true if the specified character represents a valid rank (1 through 8), otherwise false.</summary>
        /// <param name="pCharacter"></param>
        /// <returns>true if the specified character represents a valid rank (1 through 8), otherwise false.</returns>
        public static bool IsCharacterValidRank(char pCharacter)
        {
            // Only digits are valid.
            if (!char.IsDigit(pCharacter))
                return false;

            int value = pCharacter - '0';
            return value >= 1 && value <= 8;
        }
        #endregion
    }
}
