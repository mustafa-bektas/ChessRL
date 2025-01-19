using System;
using Items;
using UnityEngine;
using Managers; // if we need references to TurnManager or BoardManager

public class ItemPickup : MonoBehaviour
{
    public ItemScriptableObject itemData; // Assign in Inspector or at runtime
    private KingController _king;
    
    private void Start()
    {
        _king = FindAnyObjectByType<KingController>();
    }
    // If using tile-based movement, you might detect item pickup in BoardManager or KingController
    // But here's a typical approach if you have colliders:

    private void OnMouseDown()
    {
        Debug.Log("Item Clicked");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Item collided with " + other.name);
        // If the King steps on this cell
        if (_king != null)
        {
            // Trigger pickup
            _king.PickupItem(itemData);
            // Destroy the pickup object from the board
            Destroy(gameObject);
        }
    }
}