namespace Enemies
{
    public class BishopEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 3; atk = 2; def = 0; // example stats
        }

        public override void EnemyMove()
        {
            // moves diagonally closer to King’s row/col
            int rowDir = -1;
            int colDir = -1;
            
            if (King.currentRow > currentRow) rowDir = 1;
            else if (King.currentRow < currentRow) rowDir = -1;
            
            if (King.currentCol > currentCol) colDir = 1;
            else if (King.currentCol < currentCol) colDir = -1;
            
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