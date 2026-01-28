using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Furnace : PlaceableEntity
{
    public UIManager UI;
    // Start is called before the first frame update
    void Start()
    {
        UI = GameObject.FindObjectOfType < UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
           
        // Select chest
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            // Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction*400, Color.red, 10f);
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
            {
                if (hit.collider.gameObject && hit.collider.gameObject== this.gameObject)
                {
                  UI.OpenSmeltingPanel();
                }
            }
        }
    }
}
