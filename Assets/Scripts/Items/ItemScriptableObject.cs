using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "ItemScriptableObject", menuName = "Scriptable Objects/ItemScriptableObject")]
    public abstract class ItemScriptableObject : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        public string description;
        public Sprite icon; // optional, for UI

        /// <summary>
        /// If true, item is applied immediately when picked up and never goes into inventory.
        /// </summary>
        public bool isPassive;

        /// <summary>
        /// Called when the King uses this item. 
        /// If isPassive == true, you may also call UseItem immediately upon pickup.
        /// </summary>
        /// <param name="king">The KingController that uses the item.</param>
        public abstract void UseItem(KingController king);
    }
}
