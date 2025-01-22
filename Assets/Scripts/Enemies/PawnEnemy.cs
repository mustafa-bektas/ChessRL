using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class PawnEnemy : ChessEnemy
    {
        public int forwardDirection = -1; // +1 means moves upward, -1 means downward

        protected override void Start()
        {
            base.Start();
            // Pawn base stats
            hp = 3;
            atk = 2;
            def = 0;
        }

        public override IEnumerator EnemyMove()
        {
            // 1) Try moving straight forward if valid and not occupied
            int forwardRow = currentRow + forwardDirection;
            if (BoardManager.IsValidPosition(forwardRow, currentCol) &&
                !BoardManager.IsPositionOccupiedByKing(forwardRow, currentCol) &&
                !BoardManager.IsPositionOccupiedByAnyEnemy(forwardRow, currentCol))
            {
                currentRow = forwardRow;
                Debug.Log("moving pawn to " + currentRow + ", " + currentCol);
                yield return StartCoroutine(SetPosition(currentRow, currentCol));
            }
            else
            {
                // 2) If blocked, check if King is diagonally forward for a capture
                //    We can check both diagonals: leftCol = currentCol-1, rightCol = currentCol+1
                int leftCol = currentCol - 1;
                int rightCol = currentCol + 1;
                
                // Check if King is on forward-left diagonal
                if (BoardManager.IsValidPosition(forwardRow, leftCol) &&
                    BoardManager.IsPositionOccupiedByKing(forwardRow, leftCol))
                {
                    // Attack the King
                    King.TakeDamage(atk);
                }
                // Check if King is on forward-right diagonal
                else if (BoardManager.IsValidPosition(forwardRow, rightCol) &&
                         BoardManager.IsPositionOccupiedByKing(forwardRow, rightCol))
                {
                    King.TakeDamage(atk);
                }

                yield return new WaitForSeconds(0.05f);

                // If not capturing the King, the Pawn does nothing else (stays put)
            }
        }
    }
}
