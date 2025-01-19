using UnityEngine;

namespace Enemies
{
    public class BishopEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 4; atk = 2; def = 0;
        }

        public override void EnemyMove()
        {
            // 1) Check the "magic sniper" special attack 
            // if the King is exactly 2 diagonal squares away
            // i.e., rowDelta == 2 and colDelta == 2
            int rowDelta = Mathf.Abs(King.currentRow - currentRow);
            int colDelta = Mathf.Abs(King.currentCol - currentCol);

            if (rowDelta == 2 && colDelta == 2)
            {
                // Check line of sight
                if (HasLineOfSight(currentRow, currentCol, King.currentRow, King.currentCol))
                {
                    // "Spell attack" at range
                    Debug.Log("Bishop casts a diagonal spell on the King!");
                    King.TakeDamage(atk); 
                    return; // Bishop used its action to cast spell, no movement
                }
            }

            // 2) If no special attack, move diagonally closer up to 2 squares
            // We'll pick the direction and attempt up to 2 steps
            int rowDir = (King.currentRow > currentRow) ? 1 : -1;
            int colDir = (King.currentCol > currentCol) ? 1 : -1;

            // Attempt 2 steps
            for (int i = 0; i < 2; i++)
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
                    // blocked => stop
                    break;
                }
            }

            // Animate final position
            StartCoroutine(SetPosition(currentRow, currentCol));

            // Could do a final adjacency check for melee
            AttackKingIfAdjacent(atk);
        }
    }
}
