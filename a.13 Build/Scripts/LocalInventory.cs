using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalInventory : MonoBehaviour
{

    public List<InventoryItem> containedItems = new List<InventoryItem>();
    public string inventoryName;

    public int sourceID;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        GameObject.FindObjectOfType<UIManager>().ShowLocalInventoryInteractScreen(this);
    }
}
