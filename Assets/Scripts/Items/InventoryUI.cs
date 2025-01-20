using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;
using TMPro; // Or wherever your ItemScriptableObject is

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemsParent;       // The parent transform for item buttons
    [SerializeField] private GameObject itemButtonPrefab; // The prefab for each item button

    /// <summary>
    /// Rebuilds the inventory UI from a list of ItemScriptableObjects.
    /// Call this whenever the inventory changes (pickup, use, drop, etc.).
    /// </summary>
    public void RefreshInventory(List<ItemScriptableObject> inventory)
    {
        // 1) Clear existing buttons
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // 2) Instantiate a new button for each item
        foreach (ItemScriptableObject itemData in inventory)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemsParent);
            // Access the button component
            Button btn = buttonObj.GetComponent<Button>();
            
            // If you have an icon, you could find an Image child and set its sprite to itemData.icon, etc.
            buttonObj.GetComponent<Image>().sprite = itemData.icon;

            // Capture the itemData in a local variable for the click event closure
            ItemScriptableObject capturedItem = itemData;
            btn.onClick.AddListener(() =>
            {
                // We find the King and use the item
                KingController king = FindAnyObjectByType<KingController>();
                if (king != null)
                {
                    king.UseItem(capturedItem);
                }
            });
        }
    }
}
