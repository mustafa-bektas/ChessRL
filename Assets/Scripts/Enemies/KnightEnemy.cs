namespace Enemies
{
    public class KnightEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 4; atk = 3; def = 0;
        }

        public override void EnemyMove()
        {
            // If King’s ATK is significantly higher (example: 5 higher), do a timid approach
            // For demonstration: If (King.atk - this.atk >= 5), do we skip or pick a move that doesn't reduce distance?
            // We'll keep it simple for now:

            int[] rowDir = { 2, 1, -1, -2, -2, -1, 1, 2 };
            int[] colDir = { 1, 2, 2, 1, -1, -2, -2, -1 };

            int minDist = int.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + rowDir[i];
                int newCol = currentCol + colDir[i];

                if (BoardManager.IsValidPosition(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
                {
                    int dist = BoardManager.GetManhattanDistance(newRow, newCol, King.currentRow, King.currentCol);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestIndex = i;
                    }
                }
            }

            if (bestIndex != -1)
            {
                currentRow += rowDir[bestIndex];
                currentCol += colDir[bestIndex];
                StartCoroutine(SetPosition(currentRow, currentCol));
            }

            // Attack if adjacent
            AttackKingIfAdjacent(atk);
        }
    }
}