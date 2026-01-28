using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[CreateAssetMenu]
public class InventoryItem : ScriptableObject
{
    // Item name
    public string Name;

    // Item description
    public string Description;

    // item ID
    public int ID;
    
    // Item unqiue id, for every instance
    public int uniqueID = 0;

    // Extra data for the item - like weapon damage, item range, by DAMAGE: 2; RANGE: 4; 
    public string extraData;

    // Works to hold an extra int, like armour value
    public int extraDataInt = 0;

    public int cooldown;

    // Its type
    public InventoryItemType type;

    
    // Its physical prefab. Any placeable entity or spawnable entity lives here
    public GameObject prefab;

    // The tags of the item
    public InventoryTag[] tags;

    public GameAction relatedAction;
    public Tile relatedTile;
    
    


    public void OnEnable()
    {
        if (uniqueID == 0)
        { 
            uniqueID = Random.Range(0, int.MaxValue);
            
        }
    }

    public InventoryItem DeepCopy()
    {
        InventoryItem I = ScriptableObject.CreateInstance<InventoryItem>();

        I.Name = Name;
        I.Description = Description;
        I.ID = ID;
        I.uniqueID= Random.Range(0, int.MaxValue);
        I.extraData = extraData;
        I.type = type;
        I.prefab = prefab;
        I.tags = tags;
        I.relatedAction = relatedAction;
        I.relatedTile = relatedTile;
        I.cooldown = cooldown;
        I.extraDataInt = extraDataInt;
    
        return I;


    }

}


public enum InventoryItemType
{
    
    None, 
    Resource,
    EquipableArmour, 
    Potion, 
    Building,
    FreeEntity,
    Object,
    Weapon, 
    Ability, 
    Ultimate,
    Food

}

public enum InventoryTag
{
    
    None, 
    PlaceableEntity,
    SpawnableEntity,
    ActionEnableing,
    PlaceableTile,
    BasicItem,
    RequiresSmelting,
    RangedWeapon,
    Helmet,
    Chestplate,
    Legs,
    FeetArmour,
    FireDamage,
    IceDamage,
    NatureDamage,
    MagicDamage,
    DarkMagicDamage,
    ElectricalDamage
    
}

class InventoryItemComparer : IEqualityComparer<InventoryItem>
{
    // Products are equal if their names and product numbers are equal.
    public bool Equals(InventoryItem x, InventoryItem y)
    {

        //Check whether the products' properties are equal.
        return x.ID == y.ID;
    }

    // If Equals() returns true for a pair of objects
    // then GetHashCode() must return the same value for these objects.
    public int GetHashCode(InventoryItem product)
    {
        //Check whether the object is null
        if (System.Object.ReferenceEquals(product, null)) return 0;

        //Get hash code for the Name field if it is not null.
        int hashProductName = product.Name == null ? 0 : product.Name.GetHashCode();

        //Get hash code for the Code field.
        int hashProductCode = product.ID.GetHashCode();

        //Calculate the hash code for the product.
        return hashProductName ^ hashProductCode;
    }
}

