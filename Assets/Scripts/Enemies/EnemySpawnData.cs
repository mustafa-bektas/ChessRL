using UnityEngine;

namespace Enemies
{
    [System.Serializable]
    public struct EnemySpawnData
    {
        public ChessEnemy enemyPrefab;   // The prefab for this enemy type
        public int minFloor;            // The floor at which this enemy can start appearing
        [Range(0f, 1f)]
        public float spawnWeight;       // Probability weight among valid enemies
        public int difficultyCost;      // Optional, if using a "cost" system
    }
}