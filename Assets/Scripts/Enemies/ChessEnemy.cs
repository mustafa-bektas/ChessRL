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
            SetPosition(currentRow, currentCol);
        }

        public virtual void TakeDamage(int damage)
        {
            hp -= (damage - def);
            if (hp <= 0) Die();
        }

        protected virtual void Die()
        {
            // Remove from TurnManager's enemy list
            TurnManager.enemies.Remove(this);
            Destroy(gameObject);
        }
    
        protected virtual void SetPosition(int r, int c)
        {
            GameObject cell = BoardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
            transform.position = cell.transform.position;
        }

        public abstract void EnemyMove(); // Must implement in derived classes
    }
}

