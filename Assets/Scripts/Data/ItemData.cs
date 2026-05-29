using UnityEngine;

[CreateAssetMenu(fileName = "NewBook", menuName = "PawnShop/Book Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemIcon;

    [Header("Sahaf Attributes")]
    public string edition = "First Edition";
    public BookRarity rarity = BookRarity.Common;
    public string conditionString = "Mint";

    [Header("Fantasy Attributes")]
    public string magicLevel = "None";
    public string age = "Modern";
    public string curse = "None";

    [Header("Economy")]
    public float basePrice = 100f;
    [Range(0f, 1f)] public float condition = 1f;

    [Header("Race")]
    public CustomerRace race;
}