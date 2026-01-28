using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : PlaceableEntity
{
    
    public static float heatRange=3;
    public static float maxheatRange = 1;

    public Player player = null;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player!= null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < heatRange)
            {
                if (!player.inRangeOfHowManyFires.Contains(uniqueID))
                {
                    player.inRangeOfHowManyFires.Add(uniqueID);
                    if (Vector3.Distance(transform.position, player.transform.position) < maxheatRange)
                    {
                        player.inRangeOfHowManyFires.Add(uniqueID + 1);
                    }
                }
               
            }
        }  
        
    }
}
