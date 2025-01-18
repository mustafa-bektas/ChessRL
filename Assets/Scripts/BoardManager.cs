using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject cellPrefab; 
    public int rows = 8;
    public int cols = 8;
    public float cellSize = 1f;   // Distance between cell centers

    private GameObject[,] grid;   // Store references to each cell in a 2D array

    void Awake()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        grid = new GameObject[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 position = new Vector2(c * cellSize, r * cellSize);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                cell.name = $"Cell_{r}_{c}";
                grid[r, c] = cell;
                cell.transform.parent = this.transform; // Keep hierarchy organized
            }
        }
    }

    // Utility method to check valid board positions
    public bool IsValidPosition(int r, int c)
    {
        return (r >= 0 && r < rows && c >= 0 && c < cols);
    }
}