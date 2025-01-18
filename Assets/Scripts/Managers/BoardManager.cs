using System;
using UnityEngine;

namespace Managers
{
    public class BoardManager : MonoBehaviour
    {
        public GameObject cellPrefab; 
        public int rows = 8;
        public int cols = 8;
        public float cellSize = 1f;   // Distance between cell centers

        private GameObject[,] _grid;   // Store references to each cell in a 2D array
        private KingController _king;
        private TurnManager _turnManager;

        void Awake()
        {
            GenerateBoard();
        }

        private void Start()
        {
            _king = FindAnyObjectByType<KingController>();
            _turnManager = FindAnyObjectByType<TurnManager>();
        }

        void GenerateBoard()
        {
            _grid = new GameObject[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector2 position = new Vector2(c * cellSize, r * cellSize);
                    GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                    cell.name = $"Cell_{r}_{c}";
                    _grid[r, c] = cell;
                    cell.transform.parent = this.transform; // Keep hierarchy organized
                }
            }
        }

        // Utility method to check valid board positions
        public bool IsValidPosition(int r, int c)
        {
            return (r >= 0 && r < rows && c >= 0 && c < cols);
        }
        
        public bool IsPositionOccupiedByKing(int r, int c)
        {
            return r == _king.currentRow && c == _king.currentCol;
        }
        
        public bool IsPositionOccupiedByAnyEnemy(int r, int c)
        {
            foreach (var enemy in _turnManager.enemies)
            {
                if (r == enemy.currentRow && c == enemy.currentCol)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetManhattanDistance(int newRow, int newCol, int kingCurrentRow, int kingCurrentCol)
        {
            return Math.Abs(newRow - kingCurrentRow) + Math.Abs(newCol - kingCurrentCol);
        }
    }
}