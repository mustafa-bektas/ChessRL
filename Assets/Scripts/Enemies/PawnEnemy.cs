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
                SetPosition(newRow, currentCol);
            }

            // Check if it's adjacent to the King and attack if so (MVP approach)
            if (Mathf.Abs(King.currentRow - currentRow) <= 1 &&
                Mathf.Abs(King.currentCol - currentCol) <= 1)
            {
                // Attack King
                King.TakeDamage(atk);
                Debug.Log("Pawn attacked the King!");
            }
        }
    }
}