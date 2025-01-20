using System.Collections;
using System.Collections.Generic;
using Enemies;
using Items;
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
    private Collider2D _collider;

    public List<ItemScriptableObject> inventory = new List<ItemScriptableObject>();
    private InventoryUI _ui;
    
    private MovementMode _movementMode = MovementMode.Normal;
    // How many moves left in special mode (1 for a single use)
    private int _tempMovementUses = 0;
    
    public enum MovementMode
    {
        Normal,
        Rook,
        Knight,
        Bishop,
        Queen
    }

    void Start()
    {
        currentHP = maxHP;
        _boardManager = FindAnyObjectByType<BoardManager>();
        _turnManager = FindAnyObjectByType<TurnManager>();
        _ui = FindAnyObjectByType<InventoryUI>();
        _collider = GetComponent<Collider2D>();
        
        _movementMode = MovementMode.Normal;
        _tempMovementUses = 0;
        
        StartCoroutine(SetPosition(currentRow, currentCol));
    }

    public void PickupItem(ItemScriptableObject itemData)
    {
        // If it's passive, apply immediately
        if (itemData.isPassive)
        {
            itemData.UseItem(this);
            Debug.Log($"Picked up {itemData.name} - passive effect applied immediately!");
        }
        else
        {
            inventory.Add(itemData);
            Debug.Log($"Picked up {itemData.name} - added to inventory!");
            // Refresh Inventory UI
            _ui.RefreshInventory(inventory);
        }
    }
    
    /// <summary>
    /// Called when player clicks an item button in the UI, or if we apply a passive effect
    /// </summary>
    public void UseItem(ItemScriptableObject item)
    {
        // Actually apply item effect
        item.UseItem(this);

        // Remove from inventory after use
        inventory.Remove(item);
        _ui.RefreshInventory(inventory);
    }
    
    public void GiveRandomMovement()
    {
        // Pick from {Rook, Bishop, Knight, Queen}
        MovementMode[] pieceOptions = { MovementMode.Rook, MovementMode.Bishop, MovementMode.Knight, MovementMode.Queen };
        int idx = Random.Range(0, pieceOptions.Length);
        _movementMode = pieceOptions[idx];
        _tempMovementUses = 1;  // one-time use

        Debug.Log($"Movement Orb grants {_movementMode} movement (1-time)!");
    }
    
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
        // disable the King's collider while moving
        _collider.enabled = false;
        
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
        _collider.enabled = true; // re-enable the collider
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
        ClearHighlightedCells(); // optionally clear old highlights before re-highlighting

        switch (_movementMode)
        {
            case MovementMode.Normal:
                HighlightKingMoves();
                break;
            case MovementMode.Rook:
                HighlightRookMoves();
                break;
            case MovementMode.Knight:
                HighlightKnightMoves();
                break;
            case MovementMode.Bishop:
                HighlightBishopMoves();
                break;
            case MovementMode.Queen:
                HighlightQueenMoves();
                break;
        }
    }

    // Normal king: 1 square in any direction
    private void HighlightKingMoves()
    {
        for (int rd = -1; rd <= 1; rd++)
        {
            for (int cd = -1; cd <= 1; cd++)
            {
                if (rd == 0 && cd == 0) continue;

                int newRow = currentRow + rd;
                int newCol = currentCol + cd;
                if (_boardManager.IsValidPosition(newRow, newCol) &&
                    !_boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
                {
                    HighlightCell(newRow, newCol);
                }
            }
        }
    }

    // Rook: up to 3 squares horizontally/vertically
    private void HighlightRookMoves()
    {
        // Check the 4 directions: up, down, left, right
        // We'll allow up to 3 squares in each direction
        int maxSteps = 3;

        // Up
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow + i;
            int newCol = currentCol;
            if (!TryHighlightCell(newRow, newCol)) break; 
            // if blocked or invalid, stop highlighting further
        }
        // Down
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow - i;
            int newCol = currentCol;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
        // Right
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow;
            int newCol = currentCol + i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
        // Left
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow;
            int newCol = currentCol - i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
    }

    // Knight: L-shaped moves
    private void HighlightKnightMoves()
    {
        // 8 possible positions
        int[] rowDir = { 2, 2, -2, -2, 1, 1, -1, -1 };
        int[] colDir = { 1, -1, 1, -1, 2, -2, 2, -2 };

        for (int i = 0; i < 8; i++)
        {
            int newRow = currentRow + rowDir[i];
            int newCol = currentCol + colDir[i];
            if (_boardManager.IsValidPosition(newRow, newCol) &&
                !_boardManager.IsPositionOccupiedByAnyEnemy(newRow, newCol))
            {
                HighlightCell(newRow, newCol);
            }
        }
    }

    // Bishop: up to 2 squares diagonally
    private void HighlightBishopMoves()
    {
        int maxSteps = 2;

        // 4 diagonal directions: up-right, up-left, down-right, down-left
        // up-right
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow + i;
            int newCol = currentCol + i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
        // up-left
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow + i;
            int newCol = currentCol - i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
        // down-right
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow - i;
            int newCol = currentCol + i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
        // down-left
        for (int i = 1; i <= maxSteps; i++)
        {
            int newRow = currentRow - i;
            int newCol = currentCol - i;
            if (!TryHighlightCell(newRow, newCol)) break;
        }
    }

    // Queen = combination of Rook + Bishop
    private void HighlightQueenMoves()
    {
        // We can reuse the logic from Rook + Bishop with max steps
        // Let's do Rook's logic with up to 3 squares, and Bishop's logic with up to 2 or 3 squares 
        // but you might want to unify them if you want the Queen to move up to 3 squares diagonally as well. 
        // We'll assume the Queen can do up to 3 squares in cardinal or diagonal directions.
        int maxSteps = 3;
        
        // Rook-like directions
        // up
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow + i, currentCol)) break;
        }
        // down
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow - i, currentCol)) break;
        }
        // right
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow, currentCol + i)) break;
        }
        // left
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow, currentCol - i)) break;
        }

        // diagonal
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow + i, currentCol + i)) break; 
        }
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow + i, currentCol - i)) break;
        }
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow - i, currentCol + i)) break;
        }
        for (int i = 1; i <= maxSteps; i++)
        {
            if (!TryHighlightCell(currentRow - i, currentCol - i)) break;
        }
    }
    
    private bool TryHighlightCell(int r, int c)
    {
        if (!_boardManager.IsValidPosition(r, c) ||
            _boardManager.IsPositionOccupiedByAnyEnemy(r, c))
        {
            // If blocked or out of bounds, return false so we stop in that direction
            return false;
        }

        HighlightCell(r, c);
        return true;
    }

    private void HighlightCell(int r, int c)
    {
        var cell = _boardManager.transform.Find($"Cell_{r}_{c}");
        if (cell != null)
        {
            CellController cc = cell.GetComponent<CellController>();
            if (cc != null)
            {
                cc.Highlight();
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

        // Deselect so we don't move again
        DeselectKing();

        // If we're using a special movement, decrement uses
        if (_tempMovementUses > 0)
        {
            _tempMovementUses--;
            if (_tempMovementUses <= 0)
            {
                _movementMode = MovementMode.Normal;
                Debug.Log("Reverted to normal movement after one special move.");
            }
        }

        // Notify TurnManager we used our move
        _turnManager.PlayerMoved();
    }
}