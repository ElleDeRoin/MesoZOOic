using System.Collections.Generic;

namespace DinoDig
{
    public static class MatchResolver
    {
        private const int MinMatchSize = 3;

        public static List<GridPiece> FindAllMatches(GridManager grid)
        {
            var visited = new HashSet<(int col, int row)>();
            var result = new List<GridPiece>();

            for (int col = 0; col < grid.ColumnCount; col++)
            {
                int height = grid.GetColumnHeight(col);
                for (int row = 0; row < height; row++)
                {
                    if (visited.Contains((col, row))) continue;

                    GridPiece piece = grid.GetPieceAt(col, row);
                    if (piece == null || piece.IsEgg) continue;

                    var group = FloodFill(grid, col, row, piece.BlockType, visited);
                    if (group.Count >= MinMatchSize)
                        result.AddRange(group);
                }
            }

            return result;
        }

        private static List<GridPiece> FloodFill(GridManager grid, int startCol, int startRow, BlockType type, HashSet<(int, int)> visited)
        {
            var group = new List<GridPiece>();
            var stack = new Stack<(int col, int row)>();
            stack.Push((startCol, startRow));

            while (stack.Count > 0)
            {
                var (col, row) = stack.Pop();
                if (visited.Contains((col, row))) continue;

                GridPiece piece = grid.GetPieceAt(col, row);
                if (piece == null || piece.IsEgg || piece.BlockType != type) continue;

                visited.Add((col, row));
                group.Add(piece);

                stack.Push((col + 1, row));
                stack.Push((col - 1, row));
                stack.Push((col, row + 1));
                stack.Push((col, row - 1));
            }

            return group;
        }
    }
}