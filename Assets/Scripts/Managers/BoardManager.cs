using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class BoardManager : MonoBehaviour
    {
        public GameObject cellPrefab;
        public GameObject wallPrefab;
        public int rows = 8;
        public int cols = 8;
        public float cellSize = 1f;   // Distance between cell centers
        [SerializeField] private int minWalls = 5;
        [SerializeField] private int maxWalls = 10;

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
            
            int wallCount = Random.Range(minWalls, maxWalls+1);
            for (int i = 0; i < wallCount; i++)
            {
                int r = Random.Range(0, rows);
                int c = Random.Range(0, cols);
                
                // Ensure we don’t place on the starting cell or exit cell, etc.
                if (r == 0 && c == 0 || r == rows - 1 && c == cols - 1)
                {
                    i--;
                    continue;
                }
                {
                    // Instantiate a wall prefab at that cell
                    GameObject wall = Instantiate(wallPrefab, _grid[r, c].transform.position, Quaternion.identity);
                    wall.transform.parent = this.transform;
                    wall.name = $"Wall_{r}_{c}";
                    _grid[r, c] = wall;
                }
            }
        }

        // Utility method to check valid board positions
        public bool IsValidPosition(int r, int c)
        {
            return (r >= 0 && r < rows && c >= 0 && c < cols && !_grid[r, c].name.Contains("Wall"));
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