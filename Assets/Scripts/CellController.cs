using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // only if using UI Image for highlight, otherwise remove

public class CellController : MonoBehaviour
{
    public int row;
    public int col;

    private SpriteRenderer _renderer;
    private Color _originalColor;

    [SerializeField] private Color highlightColor = Color.yellow;

    // Whether this cell is currently highlighted as a valid move
    private bool _isHighlighted = false;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.color;
        }
    }

    /// <summary>
    /// Highlights this cell so the player knows it's a valid move.
    /// </summary>
    public void Highlight()
    {
        if (_renderer != null)
        {
            _renderer.color = highlightColor;
        }
        _isHighlighted = true;
    }

    /// <summary>
    /// Removes highlight from this cell.
    /// </summary>
    public void Unhighlight()
    {
        if (_renderer != null)
        {
            _renderer.color = _originalColor;
        }
        _isHighlighted = false;
    }

    /// <summary>
    /// Called when this cell is clicked. We'll pass the row/col to the KingController if it's expecting a move.
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log($"Cell clicked: {row}, {col}");
        // If this cell is highlighted, it might be a valid move. 
        // We let the King know about the click. 
        if (_isHighlighted)
        {
            KingController king = FindAnyObjectByType<KingController>();
            if (king != null)
            {
                king.OnCellClicked(row, col);
            }
        }
    }
}