using System;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("UI References")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button powerAttackButton;
        [SerializeField] private Button skipButton;
        
        [SerializeField] private TMPro.TextMeshProUGUI phaseText;

        // Tracks whether we've moved in the current turn (affects Power Attack logic)
        private bool _hasMovedThisTurn = false;

        private void Start()
        {
            // Hook up the button events
            attackButton.onClick.AddListener(OnAttackClicked);
            powerAttackButton.onClick.AddListener(OnPowerAttackClicked);
            skipButton.onClick.AddListener(OnSkipClicked);

            RefreshUI();
        }
        
        private void Update()
        {
            switch (_currentPhase)
            {
                case GamePhase.PlayerTurn:
                    // We'll rely on click-based King movement and UI button clicks
                    break;

                case GamePhase.EnemyTurn:
                    HandleEnemyTurn();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckWinCondition();
        }

        #region Player Turn Actions (Button Clicks)

        /// <summary>
        /// Player clicked the "Attack" button.
        /// </summary>
        private void OnAttackClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn || _playerSubPhase != PlayerSubPhase.Action)
                return;

            ChessEnemy adjacentEnemy = FindAdjacentEnemy();
            if (adjacentEnemy != null)
            {
                king.Attack(adjacentEnemy);
                EndPlayerTurn();
            }
            else
            {
                Debug.Log("No adjacent enemy to attack!");
            }
        }

        /// <summary>
        /// Player clicked the "Power Attack" button (only valid if we haven't moved).
        /// </summary>
        private void OnPowerAttackClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn || _playerSubPhase != PlayerSubPhase.Action)
                return;

            if (_hasMovedThisTurn)
            {
                Debug.Log("Cannot Power Attack after moving!");
                return;
            }

            ChessEnemy adjacentEnemy = FindAdjacentEnemy();
            if (adjacentEnemy != null)
            {
                int originalAtk = king.atk;
                king.atk += 2; // temporary buff
                king.Attack(adjacentEnemy);
                king.atk = originalAtk;

                EndPlayerTurn();
            }
            else
            {
                Debug.Log("No adjacent enemy for Power Attack!");
            }
        }

        /// <summary>
        /// Player clicked the "Skip" button. 
        /// Could be skipping Movement in Move sub-phase OR skipping Action in Action sub-phase.
        /// </summary>
        private void OnSkipClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn)
                return;

            if (_playerSubPhase == PlayerSubPhase.Move)
            {
                // Skipping movement
                _hasMovedThisTurn = false;
                _playerSubPhase = PlayerSubPhase.Action;
                Debug.Log("Player skipped movement.");
                RefreshUI();
            }
            else if (_playerSubPhase == PlayerSubPhase.Action)
            {
                // Skipping action => end turn
                Debug.Log("Player skipped action.");
                EndPlayerTurn();
            }
        }

        #endregion

        #region Turn Flow

        private void EndPlayerTurn()
        {
            _currentPhase = GamePhase.EnemyTurn;
            _playerSubPhase = PlayerSubPhase.Move;
            _hasMovedThisTurn = false;
            RefreshUI();
        }

        /// <summary>
        /// Called by KingController after a successful click-move.
        /// This sets the sub-phase to Action and flags we've used our move.
        /// </summary>
        public void PlayerMoved()
        {
            _hasMovedThisTurn = true;
            _playerSubPhase = PlayerSubPhase.Action;
            RefreshUI();
        }

        /// <summary>
        /// Tells the King whether it's allowed to move right now (click-based).
        /// True if it's PlayerTurn AND the sub-phase is Move.
        /// </summary>
        public bool CanPlayerMove()
        {
            return _currentPhase == GamePhase.PlayerTurn && _playerSubPhase == PlayerSubPhase.Move;
        }

        #endregion

        #region Enemy Turn

        private void HandleEnemyTurn()
        {
            // Enemies perform their moves
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.EnemyMove();
                }
            }

            // After enemies move, go back to Player Turn
            _currentPhase = GamePhase.PlayerTurn;
            RefreshUI();
        }

        #endregion

        #region UI & Helpers

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

        private void CheckWinCondition()
        {
            // If no enemies left, you win!
            if (enemies == null || enemies.Count == 0)
            {
                Debug.Log("All enemies defeated! You Win!");
            }

            // If King is at exit
            if (king.currentRow == 7 && king.currentCol == 7)
            {
                Debug.Log("Reached the exit! You Win!");
            }
        }
        
        private void RefreshUI()
        {
            // Default to all disabled
            attackButton.interactable = false;
            powerAttackButton.interactable = false;
            skipButton.interactable = false;

            // If not player turn, we do nothing more
            if (_currentPhase != GamePhase.PlayerTurn) return;

            // We are in Player Turn, so skip button is always available (for skipping move/action).
            skipButton.interactable = true;

            // Check sub-phase
            if (_playerSubPhase == PlayerSubPhase.Move)
            {
                // Move sub-phase => no Attack or PowerAttack
                phaseText.text = "MOVE";
                return;
            }
            else if (_playerSubPhase == PlayerSubPhase.Action)
            {
                phaseText.text = "ACTION";
                // 1) If no adjacent enemy, automatically skip action
                bool enemyAdjacent = (FindAdjacentEnemy() != null);
                if (!enemyAdjacent)
                {
                    Debug.Log("No adjacent enemy => skipping action phase automatically.");
                    EndPlayerTurn();
                    return; // do NOT enable any buttons
                }

                // 2) If an enemy is adjacent, we can Attack or possibly PowerAttack
                attackButton.interactable = true;
                // Power Attack only if we haven't moved
                powerAttackButton.interactable = !_hasMovedThisTurn;
            }
        }

        #endregion
    }
}
