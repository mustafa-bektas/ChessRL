using UnityEngine;

namespace Enemies
{
    public class PawnEnemy : ChessEnemy
    {
        public int forwardDirection = -1; // +1 means moves upward

        public override void EnemyMove()
        {
            // Simple logic: move forward if valid
            int newRow = currentRow + forwardDirection;
            if (BoardManager.IsValidPosition(newRow, currentCol))
            {
                currentRow = newRow;
                StartCoroutine(SetPosition(currentRow, currentCol));
            }

            AttackKingIfPossible();
        }
    }
}