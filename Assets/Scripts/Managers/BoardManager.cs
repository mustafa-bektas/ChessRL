using System;
using System.Collections.Generic;
using Items;
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
        public List<ItemScriptableObject> possibleItems;
        public ItemPickup itemPickupPrefab;

        void Awake()
        {
            GenerateBoard();
        }

        private void Start()
        {
            _king = FindAnyObjectByType<KingController>();
            _turnManager = FindAnyObjectByType<TurnManager>();
            GenerateWalls();
            SpawnRandomItem();
        }

        private void SpawnRandomItem()
        {
            // 1) Pick a random item type
            ItemScriptableObject randomType = GetRandomItemType();

            // 3) Find a random free cell
            List<Vector2Int> freeCells = new List<Vector2Int>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (IsValidPosition(r,c) && !IsWall(r,c) && !IsPositionOccupiedByAnyEnemy(r,c) && !IsPositionOccupiedByKing(r,c))
                    {
                        freeCells.Add(new Vector2Int(r,c));
                    }
                }
            }

            if (freeCells.Count == 0)
            {
                Debug.LogWarning("No free cell to spawn item!");
                return;
            }

            Vector2Int chosen = freeCells[Random.Range(0, freeCells.Count)];

            // 4) Instantiate the item pickup prefab
            GameObject cellObj = _grid[chosen.x, chosen.y];
            Vector3 spawnPos = cellObj.transform.position;
            ItemPickup pickup = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity);

            pickup.itemData = randomType;
            pickup.GetComponent<SpriteRenderer>().sprite = randomType.icon;

            Debug.Log($"Spawned {randomType.itemName} at ({chosen.x},{chosen.y})");
        }
        
        bool IsWall(int r, int c)
        {
            return _grid[r, c].name.Contains("Wall");
        }
        
        private ItemScriptableObject GetRandomItemType()
        {
            // Weighted or not, up to you
            int index = Random.Range(0, possibleItems.Count);
            ItemScriptableObject chosenItem = possibleItems[index];
            return chosenItem;
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

                    // Assign row/col to the CellController
                    CellController cc = cell.GetComponent<CellController>();
                    if (cc != null)
                    {
                        cc.row = r;
                        cc.col = c;
                    }
                }
            }
        }

        void GenerateWalls()
        {
            int wallCount = Random.Range(minWalls, maxWalls+1);
            for (int i = 0; i < wallCount; i++)
            {
                int r = Random.Range(0, rows);
                int c = Random.Range(0, cols);
                
                // Ensure we don’t place on the starting cell or exit cell, etc.
                if (r == 0 && c == 0 || r == rows - 1 && c == cols - 1 || IsPositionOccupiedByAnyEnemy(r, c))
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