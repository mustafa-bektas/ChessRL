using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class RookEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 5; 
            atk = 3; 
            def = 1;
        }

        public override IEnumerator EnemyMove()
        {
            // Determine how far away the Rook is vertically vs horizontally
            int rowDiff = King.currentRow - currentRow;
            int colDiff = King.currentCol - currentCol;

            // Decide which axis is "dominant" based on absolute difference
            bool moveVertically = (Mathf.Abs(rowDiff) >= Mathf.Abs(colDiff));

            // We'll try up to 3 squares in that direction.
            // If we can't move at least 1 square, we'll try the other axis.

            bool moved = false;
            if (moveVertically)
            {
                moved = TryRookMovement(rowDiff, 0);
                // If we couldn't move in that axis, try horizontal
                if (!moved)
                {
                    moved = TryRookMovement(0, colDiff);
                }
            }
            else
            {
                // First try horizontal
                moved = TryRookMovement(0, colDiff);
                // If blocked, try vertical
                if (!moved)
                {
                    moved = TryRookMovement(rowDiff, 0);
                }
            }

            // Animate final position (whether we moved or not)
            yield return StartCoroutine(SetPosition(currentRow, currentCol));

            // If we ended up adjacent to the King, do a melee attack
            AttackKingIfAdjacent(atk);
            
            yield return new WaitForSeconds(0.05f);
        }

        /// <summary>
        /// Attempts to move the Rook up to 3 squares in the sign of rowDelta or colDelta.
        /// If we successfully move at least 1 square, returns true; otherwise false.
        /// </summary>
        private bool TryRookMovement(int rowDelta, int colDelta)
        {
            // Determine the sign of movement in each axis (row or col).
            int rowDir = (rowDelta > 0) ? 1 : (rowDelta < 0) ? -1 : 0;
            int colDir = (colDelta > 0) ? 1 : (colDelta < 0) ? -1 : 0;

            int stepsToMove = 3;
            bool movedAtLeastOneSquare = false;

            while (stepsToMove > 0)
            {
                int newRow = currentRow + rowDir;
                int newCol = currentCol + colDir;

                // If next cell is valid & not occupied, move into it
                if (BoardManager.IsValidPosition(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByKing(newRow, newCol))
                {
                    currentRow = newRow;
                    currentCol = newCol;
                    movedAtLeastOneSquare = true;
                }
                else
                {
                    // Blocked, stop trying
                    break;
                }
                stepsToMove--;
            }

            return movedAtLeastOneSquare;
        }
    }
}
