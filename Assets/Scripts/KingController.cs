using System.Collections;
using Enemies;
using Managers;
using UnityEngine;

public sealed class KingController : MonoBehaviour
{
    public int currentRow = 0;
    public int currentCol = 0;
    public int maxHP = 5;
    public int currentHP;
    public int atk = 2;
    public int def = 1;

    private BoardManager boardManager;

    void Start()
    {
        currentHP = maxHP;
        boardManager = FindAnyObjectByType<BoardManager>();
        StartCoroutine(SetPosition(currentRow, currentCol));
    }

    // Removed the Update() input checks entirely!

    public void TryMove(int rowDelta, int colDelta)
    {
        int newRow = currentRow + rowDelta;
        int newCol = currentCol + colDelta;

        // Check if new position is valid
        if (boardManager.IsValidPosition(newRow, newCol) &&
            !boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
        {
            currentRow = newRow;
            currentCol = newCol;
            StartCoroutine(SetPosition(newRow, newCol));
        }
    }

    private IEnumerator SetPosition(int r, int c)
    {
        GameObject cell = boardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
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

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(1, damage - this.def);
        currentHP -= damageTaken;
        Debug.Log($"King took {damageTaken} damage! Current HP: {currentHP}");
        
        if (currentHP <= 0)
        {
            Debug.Log("King is dead! Game Over.");
            // Disable King or trigger a UI
            gameObject.SetActive(false);
        }
    }

    public void Attack(ChessEnemy enemy)
    {
        if (enemy == null) return;

        int damage = Mathf.Max(1, this.atk);
        enemy.TakeDamage(damage);
    }
}