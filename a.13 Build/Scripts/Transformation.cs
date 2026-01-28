using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Transformation: ScriptableObject
{
   public string Name;


   public string Description;

   public bool unlocked;

    public Acheivement unlockableAchivement;

    public List<InventoryItem> abilities;
    public InventoryItem ultimateAbility;


}
