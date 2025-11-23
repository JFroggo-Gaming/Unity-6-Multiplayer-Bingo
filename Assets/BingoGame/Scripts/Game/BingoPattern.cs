using UnityEngine;

namespace BingoGame.Network
{
    // Defines a Bingo pattern (5x5 grid)
    [CreateAssetMenu(fileName = "BingoPattern", menuName = "Bingo/Pattern")]
    public class BingoPattern : ScriptableObject
    {
        public string patternName = "Pattern";

        [Header("Pattern Grid (24 cells)")]
        [Tooltip("True = this cell must be marked to win")]
        public bool[] pattern = new bool[24];

        // Automatically fix pattern size when edited in Inspector
        private void OnValidate()
        {
            if (pattern == null || pattern.Length != 24)
            {
                bool[] newPattern = new bool[24];
                if (pattern != null)
                {
                    // Copy existing values
                    int copyCount = Mathf.Min(pattern.Length, 24);
                    for (int i = 0; i < copyCount; i++)
                    {
                        newPattern[i] = pattern[i];
                    }
                }
                pattern = newPattern;
                Debug.Log($"[BingoPattern] Fixed pattern size to 24 for '{patternName}'");
            }
        }

        /// <summary>
        /// Check if the marked cells match this pattern
        /// </summary>
        public bool CheckPattern(bool[] markedCells)
        {
            if (markedCells == null || markedCells.Length != 24)
            {
                Debug.LogError($"Invalid marked cells array! Expected 24, got {markedCells?.Length}");
                return false;
            }

            // Check each cell in the pattern
            for (int i = 0; i < 24; i++)
            {
                // If pattern requires this cell to be marked, check if it is
                if (pattern[i] && !markedCells[i])
                {
                    return false; // Pattern cell not marked
                }
            }

            return true; // All required cells are marked
        }

        /// <summary>
        /// Get pattern as 2D array for easier visualization (6x4 grid)
        /// </summary>
        public bool[,] GetPattern2D()
        {
            bool[,] grid = new bool[6, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    int index = y * 6 + x;
                    if (index < 24)
                    {
                        grid[x, y] = pattern[index];
                    }
                }
            }
            return grid;
        }

        /// <summary>
        /// Create common patterns for 24-cell grid (6x4)
        /// </summary>
        public static BingoPattern CreateHorizontalLine(int row)
        {
            BingoPattern pattern = CreateInstance<BingoPattern>();
            pattern.patternName = $"Horizontal Line {row + 1}";
            pattern.pattern = new bool[24];

            for (int x = 0; x < 6; x++)
            {
                int index = row * 6 + x;
                if (index < 24)
                {
                    pattern.pattern[index] = true;
                }
            }

            return pattern;
        }

        public static BingoPattern CreateVerticalLine(int col)
        {
            BingoPattern pattern = CreateInstance<BingoPattern>();
            pattern.patternName = $"Vertical Line {col + 1}";
            pattern.pattern = new bool[24];

            for (int y = 0; y < 4; y++)
            {
                int index = y * 6 + col;
                if (index < 24)
                {
                    pattern.pattern[index] = true;
                }
            }

            return pattern;
        }

        public static BingoPattern CreateFullCard()
        {
            BingoPattern pattern = CreateInstance<BingoPattern>();
            pattern.patternName = "Full Card";
            pattern.pattern = new bool[24];

            for (int i = 0; i < 24; i++)
            {
                pattern.pattern[i] = true;
            }

            return pattern;
        }

        public static BingoPattern CreateFourCorners()
        {
            BingoPattern pattern = CreateInstance<BingoPattern>();
            pattern.patternName = "Four Corners";
            pattern.pattern = new bool[24];

            pattern.pattern[0] = true;  // Top-left
            pattern.pattern[5] = true;  // Top-right
            pattern.pattern[18] = true; // Bottom-left
            pattern.pattern[23] = true; // Bottom-right

            return pattern;
        }
    }
}
