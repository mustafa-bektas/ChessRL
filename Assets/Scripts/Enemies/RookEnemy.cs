using UnityEngine;

namespace Enemies
{
    public class RookEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 5; atk = 3; def = 1; // example stats
        }

        public override void EnemyMove()
        {
            // moves horizontal OR vertical to king’s row/col
            // (choose the bigger difference)
        
            int rowDiff = Mathf.Abs(King.currentRow - currentRow);
            int colDiff = Mathf.Abs(King.currentCol - currentCol);
        
            int rowDir = 0;
            int colDir = 0;
        
            if (rowDiff > colDiff) rowDir = (King.currentRow > currentRow) ? 1 : -1;
            else colDir = (King.currentCol > currentCol) ? 1 : -1;
        
            // Attempt move 1 square in that direction
            int newRow = currentRow + rowDir;
            int newCol = currentCol + colDir;
        
            if (BoardManager.IsValidPosition(newRow, newCol) && !BoardManager.IsPositionOccupiedByKing(newRow, newCol))
            {
                currentRow = newRow;
                currentCol = newCol;
                SetPosition(currentRow, currentCol);
            }
        }
    }
}
