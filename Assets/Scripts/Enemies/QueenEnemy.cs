namespace Enemies
{
    public class QueenEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 4; atk = 4; def = 1; // example stats
        }

        public override void EnemyMove()
        {
            // moves diagonally or orthogonally closer to King’s row/col
            // combines Bishop and Rook movement
            
            // Bishop movement
            int rowDir = 0;
            int colDir = 0;
            
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
                StartCoroutine(SetPosition(currentRow, currentCol));
            }
            
            AttackKingIfPossible();

        }
    }
}