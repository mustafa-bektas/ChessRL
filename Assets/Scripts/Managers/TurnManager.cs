using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.UI;
using Upgrade;

namespace Managers
{
    public class TurnManager : MonoBehaviour
    {
        public KingController king;
        public List<ChessEnemy> enemies = new List<ChessEnemy>();

        private enum GamePhase { PlayerTurn, EnemyTurn }
        [SerializeField] private GamePhase _currentPhase = GamePhase.PlayerTurn;

        private enum PlayerSubPhase { Move, Action, SelectingAttackTarget }
        [SerializeField] private PlayerSubPhase _playerSubPhase = PlayerSubPhase.Move;

        private enum AttackMode { None, Normal, Power }
        private AttackMode _pendingAttackMode = AttackMode.None;
        
        private BoardManager _boardManager;

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
            attackButton.onClick.AddListener(OnAttackButtonClicked);
            powerAttackButton.onClick.AddListener(OnPowerAttackButtonClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
            _boardManager = FindAnyObjectByType<BoardManager>();

            RefreshUI();
        }
        
        private void Update()
        {
            switch (_currentPhase)
            {
                case GamePhase.PlayerTurn:
                    // We'll rely on click-based movement in KingController and UI button clicks
                    break;

                case GamePhase.EnemyTurn:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckWinCondition();
        }

        #region Player Turn Button Clicks

        /// <summary>
        /// The player clicked the "Attack" button.
        /// Enter a sub-state: SelectingAttackTarget (Normal).
        /// </summary>
        private void OnAttackButtonClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn || _playerSubPhase != PlayerSubPhase.Action)
                return;

            // Check if we have any adjacent enemies
            var adjacentEnemies = GetAdjacentEnemies();
            if (adjacentEnemies.Count == 0)
            {
                Debug.Log("No adjacent enemies to attack.");
                return;
            }

            // Enter SelectingAttackTarget state
            _pendingAttackMode = AttackMode.Normal;
            _playerSubPhase = PlayerSubPhase.SelectingAttackTarget;
            HighlightEnemies(adjacentEnemies);
            RefreshUI();
        }

        /// <summary>
        /// The player clicked the "Power Attack" button (only valid if we haven't moved).
        /// </summary>
        private void OnPowerAttackButtonClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn || _playerSubPhase != PlayerSubPhase.Action)
                return;

            if (_hasMovedThisTurn)
            {
                Debug.Log("Cannot Power Attack after moving!");
                return;
            }

            // Check if we have any adjacent enemies
            var adjacentEnemies = GetAdjacentEnemies();
            if (adjacentEnemies.Count == 0)
            {
                Debug.Log("No adjacent enemies to power attack.");
                return;
            }

            // Enter SelectingAttackTarget state with Power
            _pendingAttackMode = AttackMode.Power;
            _playerSubPhase = PlayerSubPhase.SelectingAttackTarget;
            HighlightEnemies(adjacentEnemies);
            RefreshUI();
        }

        /// <summary>
        /// The player clicked the "Skip" button. 
        /// Could be skipping Movement, skipping Action, or potentially skipping target selection.
        /// </summary>
        private void OnSkipClicked()
        {
            if (_currentPhase != GamePhase.PlayerTurn)
                return;

            switch (_playerSubPhase)
            {
                case PlayerSubPhase.Move:
                    // Skipping movement
                    _hasMovedThisTurn = false;
                    _playerSubPhase = PlayerSubPhase.Action;
                    Debug.Log("Player skipped movement.");
                    RefreshUI();
                    break;

                case PlayerSubPhase.Action:
                    // Skipping action => end turn
                    Debug.Log("Player skipped action.");
                    EndPlayerTurn();
                    break;

                case PlayerSubPhase.SelectingAttackTarget:
                    // If the player decides not to pick any target after all
                    // revert to Action sub-phase or end turn if desired
                    Debug.Log("Player cancelled attack target selection.");
                    _playerSubPhase = PlayerSubPhase.Action;
                    _pendingAttackMode = AttackMode.None;
                    ClearEnemyHighlights();
                    RefreshUI();
                    break;
            }
        }

        #endregion

        #region Enemy Clicks

        /// <summary>
        /// Called by an Enemy when it is clicked. (We'll add an OnMouseDown or something in ChessEnemy.)
        /// If we are selecting an attack target, we resolve the attack.
        /// </summary>
        public void OnEnemyClicked(ChessEnemy enemy)
        {
            // Only relevant if we're selecting an attack target
            if (_currentPhase != GamePhase.PlayerTurn || _playerSubPhase != PlayerSubPhase.SelectingAttackTarget)
                return;

            // Check if the clicked enemy is actually adjacent to the King
            bool isAdjacent = Mathf.Abs(enemy.currentRow - king.currentRow) <= 1
                              && Mathf.Abs(enemy.currentCol - king.currentCol) <= 1;
            if (!isAdjacent)
            {
                Debug.Log("Clicked an enemy who is not adjacent. Invalid target.");
                return;
            }

            // Perform the attack
            if (_pendingAttackMode == AttackMode.Normal)
            {
                king.Attack(enemy);
            }
            else if (_pendingAttackMode == AttackMode.Power)
            {
                int originalAtk = king.atk;
                king.atk += 2;
                king.Attack(enemy);
                king.atk = originalAtk;
            }

            // Cleanup
            _pendingAttackMode = AttackMode.None;
            ClearEnemyHighlights();

            // End turn
            EndPlayerTurn();
        }

        #endregion

        #region Turn Flow

        private void EndPlayerTurn()
        {
            _currentPhase = GamePhase.EnemyTurn;
            _playerSubPhase = PlayerSubPhase.Move;
            _hasMovedThisTurn = false;
            _pendingAttackMode = AttackMode.None;

            RefreshUI();
            HandleEnemyTurn();
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

        private void HandleEnemyTurn()
        {
            // Start a coroutine that processes enemies in sequence
            StartCoroutine(EnemyTurnSequence());
        }

        private IEnumerator EnemyTurnSequence()
        {
            Debug.Log("enemyturnsequence called");
            foreach (var enemy in enemies)
            {
                if (enemy)
                {
                    // Wait for this enemy's move+attack to complete
                    yield return StartCoroutine(enemy.EnemyMove());
                }
            }

            // After all enemies have moved in sequence, return to Player Turn
            _currentPhase = GamePhase.PlayerTurn;
            RefreshUI();
        }

        #endregion

        #region UI & Helpers

        private List<ChessEnemy> GetAdjacentEnemies()
        {
            var list = new List<ChessEnemy>();
            foreach (var e in enemies)
            {
                if (e == null) continue;
                if (Mathf.Abs(e.currentRow - king.currentRow) <= 1 &&
                    Mathf.Abs(e.currentCol - king.currentCol) <= 1)
                {
                    list.Add(e);
                }
            }
            return list;
        }

        private void HighlightEnemies(List<ChessEnemy> enemyList)
        {
            foreach (var e in enemyList)
            {
                e.Highlight(true); // We'll add a method in ChessEnemy or a separate script to visually highlight
            }
        }

        private void ClearEnemyHighlights()
        {
            // Clear highlight from all enemies
            foreach (var e in enemies)
            {
                if (e != null) e.Highlight(false);
            }
        }

        private void CheckWinCondition()
        {
            // If no enemies left, you win!
            if (enemies == null || enemies.Count == 0)
            {
                ShowUpgradeChoices();
            }

            // If King is at exit
            if (king.currentRow == 7 && king.currentCol == 7)
            {
                ShowUpgradeChoices();
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

            // The skip button is always available (for skipping move, action, or canceling target selection)
            skipButton.interactable = true;

            // We handle sub-phase
            switch (_playerSubPhase)
            {
                case PlayerSubPhase.Move:
                    phaseText.text = "MOVE";
                    // Move sub-phase => no Attack or PowerAttack
                    return;

                case PlayerSubPhase.Action:
                    phaseText.text = "ACTION";
                    
                    // Attack or Power Attack if there's an adjacent enemy
                    bool enemyAdjacent = (GetAdjacentEnemies().Count > 0);
                    if (enemyAdjacent)
                    {
                        attackButton.interactable = true;
                        powerAttackButton.interactable = !_hasMovedThisTurn;
                    }
                    else
                    {
                        Debug.Log("No adjacent enemy => skipping action phase automatically.");
                        EndPlayerTurn();
                        return; // do NOT enable any buttons
                    }
                    return;

                case PlayerSubPhase.SelectingAttackTarget:
                    phaseText.text = "SELECT TARGET";
                    // We are waiting for the player to click an adjacent enemy
                    // No further buttons are needed except skip (to cancel)
                    return;
            }
        }

        #endregion
        
        #region Floor Progression & Upgrades

        private bool _upgradeUIShown = false; // in case you only want to show once

        private void ShowUpgradeChoices()
        {
            if (_upgradeUIShown) return; // avoid double show
            _upgradeUIShown = true;

            UpgradeUI upgradeUI = FindAnyObjectByType<UpgradeUI>();
            if (upgradeUI != null)
            {
                upgradeUI.ShowUpgradePanel();
            }
        }

        /// <summary>
        /// Called by UpgradeUI after selecting an upgrade.
        /// We then go to the next floor.
        /// </summary>
        public void FloorClearedAndUpgraded()
        {
            NextFloor();
        }

        private void NextFloor()
        {
            _upgradeUIShown = false;

            // Increase BoardManager's floor index
            _boardManager.currentFloor++;
            // Clear the old enemies list
            enemies.Clear();

            // Generate the new floor
            _boardManager.GenerateFloor(_boardManager.currentFloor);
            Debug.Log($"Moved on to Floor {_boardManager.currentFloor}!");
        }

        #endregion
    }
}
