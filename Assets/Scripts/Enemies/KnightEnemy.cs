namespace Enemies
{
    public class KnightEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 2; atk = 3; def = 0; // example stats
        }

        public override void EnemyMove()
        {
            // jumps in an L shape just like chess
            // tries to close the distance to the King
            
            // 8 possible moves
            int[] rowDir = { 2, 1, -1, -2, -2, -1, 1, 2 };
            int[] colDir = { 1, 2, 2, 1, -1, -2, -2, -1 };
            
            // find the move that gets the Knight closest to the King
            
            int minDist = int.MaxValue;
            int minIndex = -1;
            
            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + rowDir[i];
                int newCol = currentCol + colDir[i];
                
                if (BoardManager.IsValidPosition(newRow, newCol) && !BoardManager.IsPositionOccupiedByKing(newRow, newCol))
                {
                    int dist = BoardManager.GetManhattanDistance(newRow, newCol, King.currentRow, King.currentCol);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIndex = i;
                    }
                }
            }
            
            if (minIndex != -1)
            {
                currentRow += rowDir[minIndex];
                currentCol += colDir[minIndex];
                StartCoroutine(SetPosition(currentRow, currentCol));
            }
            
            AttackKingIfPossible();

        }
    }
}