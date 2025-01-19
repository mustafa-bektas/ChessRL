using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "PassiveCharm", menuName = "Items/Passive Charm")]
    public class PassiveCharm : ItemScriptableObject
    {
        public int attackBoost = 1;

        private void OnEnable()
        {
            // We can set isPassive = true, so we never add it to inventory
            isPassive = true;
        }

        public override void UseItem(KingController king)
        {
            // +1 permanent ATK
            king.atk += attackBoost;
            Debug.Log($"PassiveCharm used! Attack is now {king.atk}.");
        }
    }
}