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

        protected BoardManager BoardManager;
        protected KingController King;
        protected TurnManager TurnManager;

        protected virtual void Start()
        {
            BoardManager = FindAnyObjectByType<BoardManager>();
            King = FindAnyObjectByType<KingController>();
            TurnManager = FindAnyObjectByType<TurnManager>();
            StartCoroutine(SetPosition(currentRow, currentCol));
        }

        public virtual void TakeDamage(int damage)
        {
            Debug.Log($"{gameObject.name} took {damage - def} damage!");
            hp -= (damage - def);
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
            GameObject cell = BoardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
            Vector3 targetPosition = cell.transform.position;
            float duration = 0.5f; // Duration of the movement in seconds
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition; // Ensure the final position is set
        }

        protected virtual void AttackKingIfPossible()
        {
            bool isAdjacent = Mathf.Abs(King.currentRow - currentRow) <= 1 &&
                              Mathf.Abs(King.currentCol - currentCol) <= 1;
            if (isAdjacent)
            {
                int damageToKing = atk;
                King.TakeDamage(damageToKing);
            }
        }

        public abstract void EnemyMove(); // Must implement in derived classes
    }
}

