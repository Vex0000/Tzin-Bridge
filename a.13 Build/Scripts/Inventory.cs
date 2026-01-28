using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public List<InventoryItem> items;

    public List<InventoryItem> possibleItems;

    public int hotbarSlot = 0;

    public static int maxNumberOfUniqueItems = 9;

    public InventoryItemCollection[] hotbar = new InventoryItemCollection[maxNumberOfUniqueItems];

    public Transform playerItemSpawnPoint;

    public GameObject itemInHand;

    public Player player;

    public CraftingReciepe[] Reciepes;

    public List<InventoryItem> abilities;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool CraftItem(CraftingReciepe RP)
    {


        if (!CanCraft(RP))
        {
            return false;
        }

        int i = 0;
        while (i < RP.input.Count)
        {
            
            InventoryItemCollection inputItem = RP.input[i];

            RemoveItemFromInventory(inputItem.item, inputItem.count);
            /*
            int y = 0;

            while (y < inputItem.count)
            {
                if ( !items.Remove(items.Find(x => x.ID == inputItem.item.ID)))
                {
                    Debug.LogError("Illegal crafting! ");
                }
               
                y++;
            }
            */
            
            i++;
        }
        
        AddItemToInven(RP.output.item.DeepCopy(), RP.output.count);
        Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.ItemsCrafted, RP.output.count);

        return true;
    }

    public void RemoveItemFromInventory(InventoryItem genericItem, int count)
    {
        int y = 0;
        while (y < count)
        {
            
            if (genericItem.tags.Contains(InventoryTag.ActionEnableing) ||genericItem.tags.Contains(InventoryTag.PlaceableTile))
            {
                player.incidentalActions.Remove(player.incidentalActions.Find(x => x.ACTIONID == genericItem.relatedAction.ACTIONID));
            }
            
            if ( !items.Remove(items.Find(x => x.ID == genericItem.ID)))
            {
                Debug.LogError("Cannot remove what does not exist ");
            }
            UpdatedInventory(true);
            
            y++;
        }
    }

    public void RemoveItemFromInventory(int UID)
    {
        if ( !items.Remove(items.Find(x => x.uniqueID == UID)))
        {
            Debug.LogError("Cannot remove what does not exist ");
        }
    }

    public bool CanCraft(CraftingReciepe RP)
    {
        
        // Check if possible 
        bool canCraft = true;
        int i = 0;
        while (i < RP.input.Count)
        {
            InventoryItemCollection inputItem = RP.input[i];

            bool canUseitem = false;
            
            foreach (InventoryItemCollection C in hotbar)
            {
                if (C == null || C.item == null)
                {
                    continue;
                }
                
                if (C.item.ID == inputItem.item.ID)
                {
                    if (C.count >= inputItem.count)
                    {
                        canUseitem = true;
                        break;
                    }
                   
                    // Bc cannot craft ever after this point 
                    canUseitem = false;
                    break;

                }
            }

            if (canCraft)
            {
                if (!canUseitem)
                {
                    canCraft = false;
                }
            }
            i++;
        }

        
        return canCraft;
        
    }
    

    public void UpdatedInventory(bool forceUpdate)
    {
        // Recalc hotbar 

        List<int> counts = new List<int>();
        List<InventoryItem> Is = FindAllUnquieItems(out counts);

        Array.Clear(hotbar, 0, hotbar.Length);
        int i = 0;
        while (i < Is.Count)
        {
            InventoryItemCollection collectI = new InventoryItemCollection();
            collectI.item = Is[i];
            collectI.count = counts[i];
            hotbar[i] = collectI;
            
            i++;
        }
        
        Destroy(itemInHand);
        //TODO fix this to let things add an incidental action while in inventory 
        player.incidentalActions.Clear();
        if (hotbar[hotbarSlot] != null && hotbar[hotbarSlot].item!= null)
        {
            // Hold item
            itemInHand = Instantiate(hotbar[hotbarSlot].item.prefab, playerItemSpawnPoint, true);
            itemInHand.transform.position = playerItemSpawnPoint.transform.position;
            
            // If action enableing, add action
            
            if (hotbar[hotbarSlot].item.tags.Contains(InventoryTag.ActionEnableing) ||hotbar[hotbarSlot].item.tags.Contains(InventoryTag.PlaceableTile))
            {
                //player.incidentalActions.Remove(player.incidentalActions.Find(x => x.ACTIONID == hotbar[hotbarSlot].item.relatedAction.ACTIONID));
                player.incidentalActions.Add(hotbar[hotbarSlot].item.relatedAction);
            }
            
        }
      

        if (forceUpdate)
        {
            player.UI.UpdateHotBar();
        }
    }
    
    public void AddItemToInven(InventoryItem item, int count)
    {

    
        // if adding an ability, just add to abilities
        if (item.type == InventoryItemType.Ability)
        {
            abilities.Add(item.DeepCopy());
            UpdatedInventory(true);
            return;
        }
        
        List<int> UItemsCount;
        List<InventoryItem> uniqueItems = FindAllUnquieItems(out UItemsCount);

        
        if (uniqueItems.Count < maxNumberOfUniqueItems)
        {
            int i = 0;
            while (i < count)
            {
                items.Add(item);
                i++;
            }
        }
        else
        {
            Debug.LogError("Not engough space in backpack!");
        }

        UpdatedInventory(true);
    }

    public bool FindInventoryItemFromName(string Name, out InventoryItem foundItem)
    {
        foundItem = possibleItems.Find(x => x.Name == Name);
        return foundItem != null;

    }

    public List<InventoryItem> FindAllUnquieItems(out List<int> counts)
    {
        List<InventoryItem> uniqueItems = new List<InventoryItem>();
        List<int> UItemsCount = new List<int>();

        foreach (InventoryItem itemA in items)
        {
            // If it doesnt exists
            InventoryItem QItem = uniqueItems.Find(x => x.Name == itemA.Name);
            if (QItem == null)
            {
                uniqueItems.Add(itemA);
                UItemsCount.Add(1);
            }
            else
            {
                int I = uniqueItems.IndexOf(QItem);
                UItemsCount[I]++;
            }
        }

        counts = UItemsCount;

        while (counts.Contains(0))
        {
            int index = counts.IndexOf(0);
            uniqueItems.RemoveAt(index);
            counts.RemoveAt(index);
        }
        return uniqueItems;
        
    }
}
