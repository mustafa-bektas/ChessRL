using Managers;
using UnityEngine;

public class KingController : MonoBehaviour
{
    public int currentRow = 0;
    public int currentCol = 0;
    public int maxHP = 5;
    public int currentHP;

    private BoardManager boardManager;

    void Start()
    {
        currentHP = maxHP;
        boardManager = FindAnyObjectByType<BoardManager>();

        // Position King at bottom-left (row=0, col=0) for simplicity
        // Adjust transform so it visually matches the board cell
        SetPosition(currentRow, currentCol);
    }

    void Update()
    {
        // We only move if it's the player's turn – 
        // but for the MVP, we can read input here or use a TurnManager approach
        if (Input.GetKeyDown(KeyCode.UpArrow))    TryMove(1, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow))  TryMove(-1, 0);
        if (Input.GetKeyDown(KeyCode.LeftArrow))  TryMove(0, -1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(0, 1);

        // Diagonals if you want them:
        if (Input.GetKeyDown(KeyCode.W)) TryMove(1, 1);   // up-right diagonal
        if (Input.GetKeyDown(KeyCode.A)) TryMove(-1, -1); // down-left diagonal
        // etc. for other diagonals...
        if (Input.GetKeyDown(KeyCode.S)) TryMove(-1, 1);  // down-right diagonal
        if (Input.GetKeyDown(KeyCode.D)) TryMove(1, -1);  // up-left diagonal
        
    }

    public void TryMove(int rowDelta, int colDelta)
    {
        int newRow = currentRow + rowDelta;
        int newCol = currentCol + colDelta;

        // Check if new position is valid
        if (boardManager.IsValidPosition(newRow, newCol) && !boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
        {
            currentRow = newRow;
            currentCol = newCol;
            SetPosition(newRow, newCol);
        }
    }

    private void SetPosition(int r, int c)
    {
        // Place King exactly at the cell's position
        GameObject cell = boardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
        transform.position = cell.transform.position;
    }

    // For taking damage
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            // For MVP, just log Game Over
            Debug.Log("King is dead! Game Over.");
            // You might disable the King or show a UI panel
        }
    }
}