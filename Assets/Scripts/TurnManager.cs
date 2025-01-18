using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public KingController king;
    public List<PawnController> enemies = new List<PawnController>();

    private enum GamePhase { PlayerTurn, EnemyTurn }
    private GamePhase currentPhase = GamePhase.PlayerTurn;

    void Update()
    {
        if (currentPhase == GamePhase.PlayerTurn)
        {
            // Wait for the King to make a move, then manually switch
            // For MVP, let's detect any key press and switch to EnemyTurn
            if (Input.anyKeyDown)
            {
                // Once the King moves, we switch
                // (In a real setup, you'd detect completion of the King's move)
                currentPhase = GamePhase.EnemyTurn;
            }
        }
        else if (currentPhase == GamePhase.EnemyTurn)
        {
            // Enemies perform their moves here
            foreach (var enemy in enemies)
            {
                enemy.EnemyMove();
            }
            // After enemies move, go back to Player Turn
            currentPhase = GamePhase.PlayerTurn;
        }

        CheckWinCondition();
    }
    
    void CheckWinCondition()
    {
        // If no enemies left, you win!
        if (enemies == null || enemies.Count == 0)
        {
            Debug.Log("All enemies defeated! You Win!");
            // Show victory screen or reload
        }

        // Or if King is at exit
        if (king.currentRow == 7 && king.currentCol == 7)
        {
            Debug.Log("Reached the exit! You Win!");
        }
    }
}