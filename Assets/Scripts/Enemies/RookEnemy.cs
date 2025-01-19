using UnityEngine;

namespace Enemies
{
    public class RookEnemy : ChessEnemy
    {
        protected override void Start()
        {
            base.Start();
            hp = 5; atk = 3; def = 1;
        }

        public override void EnemyMove()
        {
            // 1) Determine which direction to move (horizontal or vertical) 
            //    We choose the direction of greatest distance from the King
            int rowDiff = King.currentRow - currentRow; 
            int colDiff = King.currentCol - currentCol;

            int moveRowDir = 0;
            int moveColDir = 0;

            if (Mathf.Abs(rowDiff) >= Mathf.Abs(colDiff))
            {
                // Move vertically toward the King
                moveRowDir = (rowDiff > 0) ? 1 : -1;
            }
            else
            {
                // Move horizontally toward the King
                moveColDir = (colDiff > 0) ? 1 : -1;
            }

            // 2) Attempt to move up to 3 squares in that direction
            int stepsToMove = 3;
            while (stepsToMove > 0)
            {
                int newRow = currentRow + moveRowDir;
                int newCol = currentCol + moveColDir;

                // If next cell is valid & not occupied, move into it
                if (BoardManager.IsValidPosition(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol) &&
                    !BoardManager.IsPositionOccupiedByKing(newRow, newCol))
                {
                    currentRow = newRow;
                    currentCol = newCol;
                    // We move 1 cell, but keep going until stepsToMove is 0 or blocked
                }
                else
                {
                    // We hit a wall or the King or an enemy => stop moving
                    break;
                }
                stepsToMove--;
            }

            // 3) Animate final position
            StartCoroutine(SetPosition(currentRow, currentCol));

            // 4) If we ended up adjacent to the King, do a melee attack
            AttackKingIfAdjacent(this.atk);
        }
    }
}
