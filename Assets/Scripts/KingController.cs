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

    private BoardManager _boardManager;
    private TurnManager _turnManager;
    private bool _isSelected = false; // is the king currently selected by the player?

    void Start()
    {
        currentHP = maxHP;
        _boardManager = FindAnyObjectByType<BoardManager>();
        _turnManager = FindAnyObjectByType<TurnManager>();
        StartCoroutine(SetPosition(currentRow, currentCol));
    }

    // Removed the Update() input checks entirely!

    public void TryMove(int rowDelta, int colDelta)
    {
        int newRow = currentRow + rowDelta;
        int newCol = currentCol + colDelta;

        // Check if new position is valid
        if (_boardManager.IsValidPosition(newRow, newCol) &&
            !_boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
        {
            currentRow = newRow;
            currentCol = newCol;
            StartCoroutine(SetPosition(newRow, newCol));
        }
    }

    private IEnumerator SetPosition(int r, int c)
    {
        GameObject cell = _boardManager.gameObject.transform.Find($"Cell_{r}_{c}").gameObject;
        Vector3 targetPosition = cell.transform.position;
        float duration = 0.3f; // Duration of the movement in seconds
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
    
    // Called by the King’s collider when clicked
    private void OnMouseDown()
    {
        Debug.Log("King clicked!");
        // Check if it's the King's turn and sub-phase is Move, etc.
        if (!_turnManager.CanPlayerMove()) // We'll define a helper in TurnManager that returns true if it's PlayerTurn + Move sub-phase
        {
            Debug.Log("Not player's turn to move!");
            return;
        }

        // Toggle selection
        if (!_isSelected)
        {
            SelectKing();
        }
        else
        {
            DeselectKing();
        }
    }
    
    private void SelectKing()
    {
        _isSelected = true;
        HighlightPossibleMoves();
    }

    private void DeselectKing()
    {
        _isSelected = false;
        ClearHighlightedCells();
    }
    
    private void HighlightPossibleMoves()
    {
        // King can move 1 square in any of the 8 directions if valid
        // We'll do rowDelta in [-1..1], colDelta in [-1..1], skipping [0,0]
        for (int rd = -1; rd <= 1; rd++)
        {
            for (int cd = -1; cd <= 1; cd++)
            {
                if (rd == 0 && cd == 0) continue; // skip 0,0

                int newRow = currentRow + rd;
                int newCol = currentCol + cd;

                if (_boardManager.IsValidPosition(newRow, newCol) &&
                    !_boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
                {
                    // highlight that cell
                    var cell = _boardManager.transform.Find($"Cell_{newRow}_{newCol}");
                    if (cell != null)
                    {
                        CellController cc = cell.GetComponent<CellController>();
                        if (cc != null)
                        {
                            cc.Highlight();
                        }
                    }
                }
            }
        }
    }
    
    private void ClearHighlightedCells()
    {
        // We can just loop over all cells and call Unhighlight.
        // Or, for efficiency, store references. For simplicity:
        foreach (Transform child in _boardManager.transform)
        {
            CellController cc = child.GetComponent<CellController>();
            if (cc != null)
            {
                cc.Unhighlight();
            }
        }
    }
    
    /// <summary>
    /// Called by CellController.OnMouseDown() if that cell was highlighted.
    /// That means the player wants to move the King here.
    /// </summary>
    public void OnCellClicked(int targetRow, int targetCol)
    {
        // Move the King
        currentRow = targetRow;
        currentCol = targetCol;
        StartCoroutine(SetPosition(currentRow, currentCol));

        // Deselect the King so we don't accidentally move it again
        DeselectKing();

        // Notify TurnManager that we've used our move
        _turnManager.PlayerMoved();
    }
}