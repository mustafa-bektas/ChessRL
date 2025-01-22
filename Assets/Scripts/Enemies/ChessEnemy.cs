using System.Collections;
using Managers;
using UnityEngine;

namespace Enemies
{
    public abstract class ChessEnemy : MonoBehaviour
    {
        public int currentRow;
        public int currentCol;
        public int hp;
        public int atk;
        public int def;
        
        private Color _originalColor;

        protected BoardManager BoardManager;
        protected KingController King;
        protected TurnManager TurnManager;

        protected virtual void Start()
        {
            BoardManager = FindAnyObjectByType<BoardManager>();
            King = FindAnyObjectByType<KingController>();
            TurnManager = FindAnyObjectByType<TurnManager>();
            _originalColor = GetComponent<SpriteRenderer>().color;
            StartCoroutine(SetPosition(currentRow, currentCol));
        }
        
        public void Highlight(bool enable)
        {
            Debug.Log("Highlighting enemy");
            GetComponent<SpriteRenderer>().color = enable ? Color.red : _originalColor;
        }
        
        private void OnMouseDown()
        {
            // If TurnManager is in "Select Target" mode, we pass this up
            TurnManager.OnEnemyClicked(this);
        }

        public virtual void TakeDamage(int damage)
        {
            int netDamage = Mathf.Max(1, damage - def);
            Debug.Log($"{gameObject.name} took {netDamage} damage!");
            hp -= netDamage;
            if (hp <= 0) Die();
        }

        protected virtual void Die()
        {
            // Remove from TurnManager's enemy list
            TurnManager.enemies.Remove(this);
            Destroy(gameObject);
        }

        protected virtual IEnumerator SetPosition(int r, int c)
        {
            // Smooth movement to the new cell
            GameObject cell = BoardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
            Vector3 targetPosition = cell.transform.position;
            float duration = 0.3f; // shorter or longer as desired
            float elapsedTime = 0f;

            Vector3 startPos = transform.position;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                transform.position = Vector3.Lerp(startPos, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition; // snap final
        }

        /// <summary>
        /// Base adjacency check for melee attacks (if needed).
        /// </summary>
        protected virtual void AttackKingIfAdjacent(int attack)
        {
            bool isAdjacent = Mathf.Abs(King.currentRow - currentRow) <= 1 &&
                              Mathf.Abs(King.currentCol - currentCol) <= 1;
            if (isAdjacent)
            {
                int damageToKing = attack;
                King.TakeDamage(damageToKing);
            }
        }

        /// <summary>
        /// Checks if there's a clear line from (startRow,startCol) to (targetRow,targetCol).
        /// Only used by Rook/Bishop/Queen for ranged or multi-square moves.
        /// </summary>
        protected bool HasLineOfSight(int startRow, int startCol, int targetRow, int targetCol)
        {
            // Determine direction
            int rowDir = (targetRow > startRow) ? 1 : (targetRow < startRow) ? -1 : 0;
            int colDir = (targetCol > startCol) ? 1 : (targetCol < startCol) ? -1 : 0;

            // Step from the next cell in that direction until we reach target or break
            int curRow = startRow + rowDir;
            int curCol = startCol + colDir;

            while (true)
            {
                // If we've arrived at the target cell
                if (curRow == targetRow && curCol == targetCol) return true;

                // If we hit a wall or an invalid position, no line of sight
                if (!BoardManager.IsValidPosition(curRow, curCol))
                    return false;

                curRow += rowDir;
                curCol += colDir;
            }
        }

        public abstract IEnumerator EnemyMove();
    }
}
