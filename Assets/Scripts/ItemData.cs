public enum ItemType
{
    HealingElixir,
    MovementOrb,
    PassiveCharm
    // You can add more in the future...
}

/// <summary>
/// Represents a single item instance in the game.
/// If you want more data (names, descriptions, icons), you can expand this.
/// </summary>
[System.Serializable]
public class ItemData
{
    public ItemType itemType;

    // For now, we store some base parameters if needed
    public string itemName;
    public string description;

    public bool IsPassive()
    {
        // Passive Charm applies effect on pickup
        // Others require manual usage
        return itemType == ItemType.PassiveCharm;
    }
}