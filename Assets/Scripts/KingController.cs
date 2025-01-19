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
    private new Collider2D _collider;

    public List<ItemScriptableObject> inventory = new List<ItemScriptableObject>();
    private InventoryUI _ui;

    void Start()
    {
        currentHP = maxHP;
        _boardManager = FindAnyObjectByType<BoardManager>();
        _turnManager = FindAnyObjectByType<TurnManager>();
        _ui = FindAnyObjectByType<InventoryUI>();
        _collider = GetComponent<Collider2D>();
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
        // Example logic:
        // 1) Randomly pick from {Rook, Bishop, Knight, Queen}
        // 2) Store it in a small variable that modifies King’s next move
        // 3) On your next move, revert to normal King movement

        // For demonstration:
        string[] pieceOptions = { "Rook", "Bishop", "Knight", "Queen" };
        int idx = Random.Range(0, pieceOptions.Length);
        string randomPiece = pieceOptions[idx];
        Debug.Log("Movement Orb grants " + randomPiece + " movement (1-time)!");

        // Implementation detail depends on how your King movement logic is set up. 
        // You might set a bool like `king.hasTempMovement = true; king.tempMovementType = randomPiece;`
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