using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gnome : FreeEntity
{
    
    
    public GameObject player;

    public int damage = 2;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<Player>().gameObject;
        currentArmour = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ProcessTurn()
    {
        
        base.ProcessTurn();
        // Go to player 
        
        actionsToDoAtEndOfTurn.Clear();
        GameAction seekPlayer = availableInnateActions.Find(x => x.ACTIONID == 6);

        GameAction actionToDo = Instantiate(seekPlayer);

        actionToDo.actionCaller = this;
        actionToDo.extraData = "Player";
        
        actionsToDoAtEndOfTurn.Add(actionToDo);
 
        // may want an animation to delay damage taken to end of movement
        
        // Attack player
        
        
        GameAction attackPlayer = availableInnateActions.Find(x => x.ACTIONID == 8); 
        if (Vector3.Distance(transform.position, player.transform.position) < attackPlayer.actionRange)
        {
            // Prevent re-moving to player
            actionsToDoAtEndOfTurn.Clear();
        }
        GameAction attackToMaybeDo = Instantiate(attackPlayer);
        attackToMaybeDo.actionCaller = this;
        attackToMaybeDo.extraData = damage.ToString();
        actionsToDoAtEndOfTurn.Add(attackToMaybeDo);
    }
}
