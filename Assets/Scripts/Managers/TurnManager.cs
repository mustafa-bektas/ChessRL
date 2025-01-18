using System;
using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Managers
{
    public class TurnManager : MonoBehaviour
    {
        public KingController king;
        public List<ChessEnemy> enemies = new List<ChessEnemy>();

        private enum GamePhase { PlayerTurn, EnemyTurn }
        [SerializeField] private GamePhase _currentPhase = GamePhase.PlayerTurn;

        private enum PlayerSubPhase { Move, Action }
        [SerializeField] private PlayerSubPhase _playerSubPhase = PlayerSubPhase.Move;

        private bool _hasMovedThisTurn = false;  // Track if the King actually moved

        private void Update()
        {
            switch (_currentPhase)
            {
                case GamePhase.PlayerTurn:
                    HandlePlayerTurn();
                    break;

                case GamePhase.EnemyTurn:
                    HandleEnemyTurn();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckWinCondition();
        }

        #region Player Turn

        private void HandlePlayerTurn()
        {
            switch (_playerSubPhase)
            {
                case PlayerSubPhase.Move:
                    HandleMoveSubPhase();
                    break;
                case PlayerSubPhase.Action:
                    HandleActionSubPhase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleMoveSubPhase()
        {
            // -- Movement Inputs --
            if (Input.GetKeyDown(KeyCode.UpArrow))    PlayerTryMove(1, 0);
            if (Input.GetKeyDown(KeyCode.DownArrow))  PlayerTryMove(-1, 0);
            if (Input.GetKeyDown(KeyCode.LeftArrow))  PlayerTryMove(0, -1);
            if (Input.GetKeyDown(KeyCode.RightArrow)) PlayerTryMove(0, 1);

            if (Input.GetKeyDown(KeyCode.W)) PlayerTryMove(1, 1);
            if (Input.GetKeyDown(KeyCode.S)) PlayerTryMove(-1, 1);
            if (Input.GetKeyDown(KeyCode.A)) PlayerTryMove(-1, -1);
            if (Input.GetKeyDown(KeyCode.D)) PlayerTryMove(1, -1);

            // Skip Movement:
            if (Input.GetKeyDown(KeyCode.M)) // 'M' for "skip Move"
            {
                // We did not move at all
                _hasMovedThisTurn = false;
                // Proceed to action sub-phase
                _playerSubPhase = PlayerSubPhase.Action;
            }
        }

        private void HandleActionSubPhase()
        {
            // Attack example: space bar to attack an adjacent enemy
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Attempt to attack an adjacent enemy if any
                ChessEnemy adjacentEnemy = FindAdjacentEnemy();
                if (adjacentEnemy != null)
                {
                    king.Attack(adjacentEnemy);
                    EndPlayerTurn();
                    return;
                }
            }

            // Wait/Pass turn
            if (Input.GetKeyDown(KeyCode.X))
            {
                // No action taken, just end turn
                EndPlayerTurn();
                return;
            }

            // Power Attack (only if we did NOT move this turn)
            // We'll say press 'P' for Power Attack
            if (!_hasMovedThisTurn && Input.GetKeyDown(KeyCode.P))
            {
                PowerAttack();
                EndPlayerTurn();
                return;
            }
        }

        private void EndPlayerTurn()
        {
            _currentPhase = GamePhase.EnemyTurn;
            // Reset the sub-phase for the next time we come back to PlayerTurn
            _playerSubPhase = PlayerSubPhase.Move;
            _hasMovedThisTurn = false;
        }

        private void PlayerTryMove(int rowDelta, int colDelta)
        {
            int oldRow = king.currentRow;
            int oldCol = king.currentCol;

            king.TryMove(rowDelta, colDelta);

            // If we actually moved
            if (king.currentRow != oldRow || king.currentCol != oldCol)
            {
                _hasMovedThisTurn = true;
                // Once the move is done, we switch to the action sub-phase
                _playerSubPhase = PlayerSubPhase.Action;
            }
        }

        private ChessEnemy FindAdjacentEnemy()
        {
            foreach (var enemy in enemies)
            {
                bool isAdjacent = Mathf.Abs(enemy.currentRow - king.currentRow) <= 1 
                                  && Mathf.Abs(enemy.currentCol - king.currentCol) <= 1;
                if (isAdjacent)
                    return enemy;
            }
            return null;
        }

        // Example Power Attack logic
        private void PowerAttack()
        {
            // Attack an adjacent enemy with a +2 to your ATK for this hit ONLY
            ChessEnemy adjacentEnemy = FindAdjacentEnemy();
            if (adjacentEnemy != null)
            {
                // Temporarily boost King’s ATK
                int originalAtk = king.atk;
                king.atk += 2;

                king.Attack(adjacentEnemy);

                // Restore original ATK
                king.atk = originalAtk;
            }
            else
            {
                Debug.Log("No adjacent enemy for Power Attack!");
            }
        }

        #endregion


        #region Enemy Turn

        private void HandleEnemyTurn()
        {
            // Enemies perform their moves
            foreach (var enemy in enemies)
            {
                if (enemy != null) // it might get destroyed mid-turn
                {
                    enemy.EnemyMove();
                }
            }

            // After enemies move, go back to Player Turn
            _currentPhase = GamePhase.PlayerTurn;
        }

        #endregion

        private void CheckWinCondition()
        {
            // If no enemies left, you win!
            if (enemies == null || enemies.Count == 0)
            {
                Debug.Log("All enemies defeated! You Win!");
                // Show victory screen or reload
            }

            // If King is at exit
            if (king.currentRow == 7 && king.currentCol == 7)
            {
                Debug.Log("Reached the exit! You Win!");
            }
        }
    }
}
