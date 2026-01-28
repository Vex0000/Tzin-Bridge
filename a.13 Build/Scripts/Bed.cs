using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bed: PlaceableEntity
{

    public Player player;

    public GameAction sleeping;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
      
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
            {
                if (hit.collider.gameObject && hit.collider.gameObject== gameObject)
                {
                    player.actionsThatThePlayerCanDoBecauseExternalStuff.Remove(player.actionsThatThePlayerCanDoBecauseExternalStuff.Find(x => x.ACTIONID == sleeping.ACTIONID));
                    //Add the action to the que so the other systems can take over 
                    if (Vector3.Distance(transform.position, player.transform.position) < sleeping.actionRange)
                    {
                        player.actionsThatThePlayerCanDoBecauseExternalStuff.Add(Instantiate(sleeping));
                    }
                }
            }
        }


    }
}
