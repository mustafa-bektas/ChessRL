using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "MovementOrb", menuName = "Items/Movement Orb")]
    public class MovementOrb : ItemScriptableObject
    {
        // For demonstration, we might define a "random piece" system or pass a known piece type
        // e.g. Rook, Bishop, Knight, Queen
        public override void UseItem(KingController king)
        {
            // Example: store a 1-time “random piece” movement in the King
            king.GiveRandomMovement();
            Debug.Log("Used Movement Orb - next move is random chess piece movement.");
        }
    }
}