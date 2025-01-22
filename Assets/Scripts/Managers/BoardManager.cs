using System;
using System.Collections.Generic;
using Enemies;
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
        
        public int currentFloor = 1;  // Track which floor we're on
        
        [Header("Enemy Spawning")]
        public List<EnemySpawnData> enemySpawnConfigs;

        // Also let us define how many enemies we spawn each floor
        [SerializeField] private int baseEnemyCount = 2;   // e.g. starts at 2
        [SerializeField] private int enemyCountPerFloor = 2; // how many extra enemies per floor

        void Awake()
        {
            _king = FindAnyObjectByType<KingController>();
            _turnManager = FindAnyObjectByType<TurnManager>();
            
            GenerateFloor(currentFloor);
        }

        private void Start()
        {
            
        }
        
        #region Floor Generation

        /// <summary>
        /// Clears the old floor and builds a new one (grid, walls, item).
        /// Also resets the King position if needed.
        /// </summary>
        public void GenerateFloor(int floorIndex)
        {
            Debug.Log($"Generating Floor {floorIndex}...");

            // 1) Clear old floor
            ClearBoard();

            // 2) Build new 8x8 grid
            _grid = new GameObject[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector2 position = new Vector2(c * cellSize, r * cellSize);
                    GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                    cell.name = $"Cell_{r}_{c}";
                    _grid[r, c] = cell;

                    // Assign row/col to the CellController for click logic
                    CellController cc = cell.GetComponent<CellController>();
                    if (cc != null)
                    {
                        cc.row = r;
                        cc.col = c;
                    }
                }
            }

            // 3) Generate random walls
            int wallCount = Random.Range(minWalls, maxWalls+1 + floorIndex); 
            // Example: more walls as floorIndex grows
            for (int i = 0; i < wallCount; i++)
            {
                int r = Random.Range(0, rows);
                int c = Random.Range(0, cols);
                
                // ensure we don't place on the start or corners or block
                if ((r == 0 && c == 0) || (r == rows - 1 && c == cols - 1))
                {
                    i--;
                    continue;
                }
                // place wall
                if (!_grid[r, c].name.Contains("Wall")) 
                {
                    GameObject wall = Instantiate(wallPrefab, _grid[r, c].transform.position, Quaternion.identity, transform);
                    wall.name = $"Wall_{r}_{c}";
                    // Replace that cell reference with the wall so IsValidPosition sees it as a wall
                    _grid[r, c] = wall;
                }
                else
                {
                    i--;
                }
            }

            // 4) (Optional) Spawn some enemies. 
            SpawnEnemies(floorIndex);
            // or we do it here. We'll skip for brevity.

            // 5) Spawn random item
            SpawnRandomItem();

            // 6) Reposition King to the start cell (0,0) or whichever
            _king.currentRow = 0;
            _king.currentCol = 0;
            _king.transform.position = _grid[0, 0].transform.position;

            Debug.Log($"Floor {floorIndex} generated!");
        }

        /// <summary>
        /// Destroys the old grid, walls, etc.
        /// </summary>
        public void ClearBoard()
        {
            if (_grid != null)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (_grid[r, c] != null) 
                        {
                            Destroy(_grid[r, c]);
                        }
                    }
                }
            }
            // Also destroy any leftover objects under BoardManager (walls, items, etc.)
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region Item Spawning

        private void SpawnRandomItem()
        {
            if (possibleItems == null || possibleItems.Count == 0) return;

            ItemScriptableObject randomType = GetRandomItemType();

            // find a random free cell
            List<Vector2Int> freeCells = new List<Vector2Int>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (IsValidPosition(r, c) && !IsWall(r, c) 
                                              && !IsPositionOccupiedByAnyEnemy(r, c) 
                                              && !(r == _king.currentRow && c == _king.currentCol))
                    {
                        freeCells.Add(new Vector2Int(r, c));
                    }
                }
            }

            if (freeCells.Count == 0)
            {
                Debug.LogWarning("No free cell to spawn item!");
                return;
            }

            Vector2Int chosen = freeCells[Random.Range(0, freeCells.Count)];
            GameObject cellObj = _grid[chosen.x, chosen.y];
            Vector3 spawnPos = cellObj.transform.position;
            ItemPickup pickup = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity, transform);

            pickup.itemData = randomType;
            SpriteRenderer sr = pickup.GetComponent<SpriteRenderer>();
            if (sr != null && randomType.icon != null)
            {
                sr.sprite = randomType.icon;
            }

            Debug.Log($"Spawned {randomType.itemName} at ({chosen.x},{chosen.y})");
        }

        private ItemScriptableObject GetRandomItemType()
        {
            int index = Random.Range(0, possibleItems.Count);
            return possibleItems[index];
        }

        private bool IsWall(int r, int c)
        {
            return _grid[r, c].name.Contains("Wall");
        }

        #endregion

        #region Helper Methods

        // Utility method to check if a position is valid & not a wall
        public bool IsValidPosition(int r, int c)
        {
            return (r >= 0 && r < rows && c >= 0 && c < cols 
                    && _grid[r, c] != null 
                    && !_grid[r, c].name.Contains("Wall"));
        }

        // Checks if an enemy is here
        public bool IsPositionOccupiedByAnyEnemy(int r, int c)
        {
            // We'll check the TurnManager's enemy list
            if (_turnManager != null)
            {
                foreach (var enemy in _turnManager.enemies)
                {
                    if (enemy != null && enemy.currentRow == r && enemy.currentCol == c)
                        return true;
                }
            }
            return false;
        }

        public int GetManhattanDistance(int newRow, int newCol, int kingCurrentRow, int kingCurrentCol)
        {
            return Math.Abs(newRow - kingCurrentRow) + Math.Abs(newCol - kingCurrentCol);
        }
        
        public bool IsPositionOccupiedByKing(int r, int c)
        {
            return r == _king.currentRow && c == _king.currentCol;
        }

        #endregion
        
        private void SpawnEnemies(int floorIndex)
        {
            // 1) Calculate how many enemies to spawn
            int enemyCount = baseEnemyCount + (floorIndex - 1) * enemyCountPerFloor;
            // for example, if base=2 and perFloor=2, then:
            // Floor1: 2 enemies
            // Floor2: 4 enemies
            // Floor3: 6 enemies, etc.

            // 2) Build a list of valid enemy types for this floor
            //    e.g. skip ones whose minFloor > floorIndex
            List<EnemySpawnData> validEnemies = new List<EnemySpawnData>();
            float totalWeight = 0f;
            foreach (var config in enemySpawnConfigs)
            {
                if (floorIndex >= config.minFloor)
                {
                    validEnemies.Add(config);
                    totalWeight += config.spawnWeight;
                }
            }
            if (validEnemies.Count == 0)
            {
                Debug.LogWarning($"No valid enemy prefabs for floor {floorIndex}! Check your config?");
                return;
            }

            // 3) If using a cost-based approach, we can define a budget = floorIndex * X
            //    For now, we'll do a simple "spawn enemyCount in random free cells" approach.

            for (int i = 0; i < enemyCount; i++)
            {
                // pick a random enemy type from validEnemies, weighted by spawnWeight
                ChessEnemy chosenPrefab = GetRandomEnemyPrefab(validEnemies, totalWeight);

                // find a random free cell
                Vector2Int cellPos = GetRandomFreeCell();
                if (cellPos.x < 0)
                {
                    Debug.LogWarning("No free cell for enemy!");
                    break;
                }

                // spawn the enemy
                GameObject cellObj = _grid[cellPos.x, cellPos.y];
                Vector3 spawnPos = cellObj.transform.position;
                ChessEnemy enemy = Instantiate(chosenPrefab, spawnPos, Quaternion.identity, transform);
                enemy.currentRow = cellPos.x;
                enemy.currentCol = cellPos.y;

                // add to TurnManager.enemies so the game can track them
                _turnManager.enemies.Add(enemy);
                Debug.Log($"Spawned {enemy.name} at ({cellPos.x},{cellPos.y})");
            }
        }

        private ChessEnemy GetRandomEnemyPrefab(List<EnemySpawnData> validEnemies, float totalWeight)
        {
            float rand = Random.value * totalWeight;
            float cumulative = 0f;
            foreach (var config in validEnemies)
            {
                cumulative += config.spawnWeight;
                if (rand <= cumulative)
                {
                    return config.enemyPrefab;
                }
            }
            // fallback
            return validEnemies[^1].enemyPrefab;
        }

        /// <summary>
        /// Picks a random valid cell with no wall, no enemy, no King, etc.
        /// Returns (-1,-1) if none available.
        /// </summary>
        private Vector2Int GetRandomFreeCell()
        {
            List<Vector2Int> freeCells = new List<Vector2Int>();
            for (int r = 4; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // is valid position, not occupied by enemy, not a wall, etc.
                    if (IsValidPosition(r, c) && !IsPositionOccupiedByAnyEnemy(r, c) 
                        && !(r == _king.currentRow && c == _king.currentCol))
                    {
                        freeCells.Add(new Vector2Int(r,c));
                    }
                }
            }
            if (freeCells.Count == 0)
                return new Vector2Int(-1, -1);

            return freeCells[Random.Range(0, freeCells.Count)];
        }

    }
}