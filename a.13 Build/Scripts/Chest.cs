using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{

    public LocalInventory items; 

    public List<InventoryItem> possibleItems;

    public Vector2Int minMaxAmount;
    
    // Start is called before the first frame update
    void Start()
    {
        items = gameObject.AddComponent<LocalInventory>();
        int i = 0;

        int numItems = Random.Range(minMaxAmount.x, minMaxAmount.y);

        while (i < numItems)
        {
            items.containedItems.Add( possibleItems[Random.Range(0, possibleItems.Count)].DeepCopy());
            i++;
        }

        gameObject.name = Random.Range(0, int.MaxValue).ToString();
        items.sourceID = int.Parse(gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
