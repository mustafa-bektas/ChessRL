using UnityEngine;

namespace Enemies
{
    public class QueenEnemy : ChessEnemy
    {
        // You could set a bool isFinalBoss = false if you want to differentiate
        [SerializeField] private bool isFinalBoss = false;

        protected override void Start()
        {
            base.Start();
            hp = isFinalBoss ? 12 : 8;
            atk = 4;
            def = 2;
        }

        public override void EnemyMove()
        {
            // 1) Check for possible ranged attack if King is within 2 squares linearly or diagonally
            //    e.g., rowDelta <= 2 && colDelta == 0 for vertical,
            //          colDelta <= 2 && rowDelta == 0 for horizontal,
            //          rowDelta == 2 && colDelta == 2 for diagonal.
            //    And then check HasLineOfSight.
            int rowDelta = Mathf.Abs(King.currentRow - currentRow);
            int colDelta = Mathf.Abs(King.currentCol - currentCol);

            bool canRangedAttack = false;

            // Horizontal/vertical within 2 squares
            if ((rowDelta == 0 && colDelta <= 2) || (colDelta == 0 && rowDelta <= 2))
            {
                if (HasLineOfSight(currentRow, currentCol, King.currentRow, King.currentCol))
                {
                    canRangedAttack = true;
                }
            }
            // Diagonal within 2 squares
            else if (rowDelta == colDelta && rowDelta <= 2)
            {
                if (HasLineOfSight(currentRow, currentCol, King.currentRow, King.currentCol))
                {
                    canRangedAttack = true;
                }
            }

            if (canRangedAttack)
            {
                Debug.Log($"Queen uses ranged attack on King!");
                King.TakeDamage(atk);
                return; // no movement if we used the ranged attack
            }

            // 2) If no ranged attack, we move up to 3 squares either diagonally or straight 
            //    whichever gets us closer to the King.
            // We'll pick direction based on row/col difference
            // Then move up to 3 steps, stopping if blocked.

            int rowDir = 0;
            int colDir = 0;

            // If diagonal is a good approach (we are not aligned horizontally or vertically)
            // We'll do a bishop-like approach. 
            // Otherwise, do rook-like approach.
            // You can refine logic if you want.
            if (Mathf.Abs(rowDelta) == Mathf.Abs(colDelta))
            {
                // Diagonal
                rowDir = (King.currentRow > currentRow) ? 1 : -1;
                colDir = (King.currentCol > currentCol) ? 1 : -1;
            }
            else if (Mathf.Abs(rowDelta) > Mathf.Abs(colDelta))
            {
                // Vertical
                rowDir = (King.currentRow > currentRow) ? 1 : -1;
            }
            else
            {
                // Horizontal
                colDir = (King.currentCol > currentCol) ? 1 : -1;
            }

            int steps = 3;
            while (steps > 0)
            {
                int newRow = currentRow + rowDir;
                int newCol = currentCol + colDir;

                if (BoardManager.IsValidPosition(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByKing(newRow, newCol))
                {
                    currentRow = newRow;
                    currentCol = newCol;
                }
                else
                {
                    // blocked
                    break;
                }
                steps--;
            }

            StartCoroutine(SetPosition(currentRow, currentCol));

            // 3) Final adjacency check for melee
            AttackKingIfAdjacent(atk);
        }
    }
}
