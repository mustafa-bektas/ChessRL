using UnityEngine;

public class PawnController : MonoBehaviour
{
    public int currentRow = 8;
    public int currentCol = 3;
    public int hp = 3;
    public int atk = 1; // for MVP
    public int forwardDirection = -1; // +1 means moves upward

    private BoardManager boardManager;
    private KingController king;

    void Start()
    {
        boardManager = FindAnyObjectByType<BoardManager>();
        king = FindAnyObjectByType<KingController>();
        // Position the pawn at a chosen row/col. For MVP, maybe row=1, col=3 or random
        SetPosition(currentRow, currentCol);
    }

    public void EnemyMove()
    {
        // Simple logic: move forward if valid
        int newRow = currentRow + forwardDirection;
        if (boardManager.IsValidPosition(newRow, currentCol))
        {
            currentRow = newRow;
            SetPosition(newRow, currentCol);
        }

        // Check if it's adjacent to the King and attack if so (MVP approach)
        if (Mathf.Abs(king.currentRow - currentRow) <= 1 &&
            Mathf.Abs(king.currentCol - currentCol) <= 1)
        {
            // Attack King
            king.TakeDamage(atk);
            Debug.Log("Pawn attacked the King!");
        }
    }

    private void SetPosition(int r, int c)
    {
        GameObject cell = boardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
        transform.position = cell.transform.position;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Debug.Log("Pawn is killed!");
            // Remove from TurnManager's list
            TurnManager tm = FindAnyObjectByType<TurnManager>();
            tm.enemies.Remove(this);
            Destroy(gameObject);
        }
    }

}