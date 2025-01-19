using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "HealingElixir", menuName = "Items/Healing Elixir")]
    public class HealingElixir : ItemScriptableObject
    {
        [Header("Healing Elixir Settings")]
        public int healAmount = 3;

        public override void UseItem(KingController king)
        {
            // Increase King's HP
            king.currentHP = Mathf.Min(king.currentHP + healAmount, king.maxHP);
            Debug.Log($"Used Healing Elixir! HP is now {king.currentHP}.");
        }
    }
}