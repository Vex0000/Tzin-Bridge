using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[CreateAssetMenu]
public class GameAction : ScriptableObject
{
    // The type of action
    public ActionType type;

    // The cost of the action
    public int actionPointHourCost;
    
    // The range of this aciton, like attack range, movement range, or interact range 
    public float actionRange;

    // The entity that called it 
    public FreeEntity actionCaller;

    // List of entitys that can recive the action
    public List<FreeEntity> actionRecievers;

    // Pos from, to
    public Vector3Int from;
    public Vector3Int to;

    public ActionTags[] tags;

    public string[] requiredTags;

    public bool isPrimaryAction;

    public int ACTIONID;
    public bool isBlockingAction;
    
    public string extraData;
    public Vector3 extraDataV;
    public Object extraDataO;
    public Object extraDataO1;

    public static Player player;
    public static GameAction prefomringBlockingAction;
    public static Vector3 blockPlaceRoation = (Quaternion.identity.eulerAngles);
    
    //TODO impliment primary actions and that is what cancels 
    // TODO impliment priorty system 
    public static void SetUp()
    {

        player = GameObject.FindObjectOfType<Player>();
    }

    public static void PossiblyAttemptGameAction(GameAction action)
    {
        if (action.ACTIONID == 0)
        {
            
        } else if (action.ACTIONID == 1)
        {
            
            // player movement 
            
           
            // For movement. If done on an enemy and holding a mele weapon, attack also
            if (player.movement.CanProcessInput&& Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject() && GameAction.prefomringBlockingAction==null)
            {
                
                // Raycast to find which tile they clicked on
                RaycastHit hit;
              //  Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction*400, Color.red, 10f);
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
                {
                    // if it exists
                    if (hit.collider.gameObject)
                    {
                        // If you hit a tile
                        if ( hit.collider.GetComponent<InGameTile>())
                        {
                            Tile T = hit.collider.gameObject.GetComponent<InGameTile>().tile;
                        
                            // If its walkable 
                            if (T.tags.Contains(Tile.TileTag.Walkable) && Vector3Int.Distance(player.movement.playerPos + Vector3Int.down, T.tilePos) <= (player.combat.playerIsInCombat?player.movement.speed:200))
                            {
                                // If it has nothing above it, can move there
                                if (T.FindNeighbors(new[] {Vector3Int.up }).Length==0)
                                {
                                    
                                    // Now check for ladder
                                    
                                    if ((T.pos.y-player.transform.position.y>.7f))
                                    {
                                        Debug.Log("Too high, check for ladder!");

                                        Collider[] cols = Physics.OverlapBox(T.pos + (Vector3.down * 2), new Vector3(1, 3, 1));
                                        bool canContinue = false;
                                        foreach (Collider col in cols)
                                        {
                                         
                                            if (col.gameObject.GetComponent<InGameTile>() && col.gameObject.GetComponent<InGameTile>().tile.type == Tile.TileType.Ladder)
                                            {
                                                // You can go up
                                                canContinue = true;
                                            }
                                        }

                                        if (!canContinue)
                                        {
                                            player.movement.CancelPrimaryActions();
                                            return;
                                        }
                                       
                                    }
                                    
                                    player.movement.CancelPrimaryActions();
                                  
                                    player.movement.willGoTo.transform.position = T.pos;
                                    
                                    
                                    if (!player.combat.playerIsInCombat)
                                    {
                                        player.movement.posToMoveTo = T.tilePos + Vector3Int.up;
                                        player.movement.actualPosToMoveTo = T.pos;
                                        player.movement.DoMovementOutsideOfCombat();
                                    }
                                    else
                                    {
                                        GameAction movementToDo = Instantiate(action);
                                        
                                        movementToDo.from = player.movement.playerPos;
                                        movementToDo.to = T.tilePos + Vector3Int.up;
                                        movementToDo.extraDataV = T.pos;
                                        player.actionsToProcessAtEndOfTurn.Add(movementToDo);
 
                                    }
                                }
                            }
                        }
                        else
                        {   // If you hit the marker, then cancel movement
                            if (hit.collider.gameObject.CompareTag(player.movement.movementMarkerWGTag))
                            {
                               Debug.Log("Canceling movement!");
                               player.movement.CancelPrimaryActions();

                            }
                        }
                    }
                }
            }
        } else if (action.ACTIONID == 2)
        {
            // CHOP tree
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
                {
                    if (hit.collider.gameObject)
                    {
                        // Hit a tile
                        if (hit.collider.GetComponent<InGameTile>())
                        {
                            Tile T = hit.collider.gameObject.GetComponent<InGameTile>().tile;
                    
                            // Right next to tree
                            if (T.type == Tile.TileType.Tree && Vector3Int.Distance(player.movement.playerPos + Vector3Int.down, T.tilePos) <= action.actionRange)
                            {
                            
                                player.movement.CancelPrimaryActions();
                                // set up vars
                                player.movement.willChop.transform.position = T.pos;
                                //set up action
                                GameAction chop = Instantiate(action);
                                chop.extraDataO = T;
                            
                                player.actionsToProcessAtEndOfTurn.Add(chop);
                            }
                        }
                        else
                        {
                            // To cancel
                            if (hit.collider.gameObject.CompareTag(player.movement.actionMarkerTag))
                            {
                                Debug.Log("Canceling action!");
                                player.movement.CancelPrimaryActions();

                            }
                        }
                    }
        
                }
            }
        } else if (action.ACTIONID == 3 )
        {
           

            // Dig ground
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
            
           
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
            {
                if (hit.collider.gameObject)
                {
                   
                    // Hit a tile
                    if (hit.collider.GetComponent<InGameTile>())
                    {
                       
                        Tile T = hit.collider.gameObject.GetComponent<InGameTile>().tile;
                        
                        // Mining
                        if ((T.tags.Contains(Tile.TileTag.CanDig) || T.tags.Contains(Tile.TileTag.CanMine)) && Vector3Int.Distance(player.movement.playerPos + Vector3Int.down, T.tilePos) <= action.actionRange && (player.movement.playerPos+Vector3Int.down)!=T.tilePos)
                        {
                            if (T.FindNeighbors(new[] {Vector3Int.up }).Length==0)
                            {
                                player.movement.CancelPrimaryActions();
                              
                                
                                GameAction mine = Instantiate(action);
                                mine.extraDataO = T;
                                
                                player.movement.willMine.transform.position = T.pos;
                                
                                player.actionsToProcessAtEndOfTurn.Add(mine);
                            }
                        }
                    }
                    else
                    {
                        // To cancel
                        if (hit.collider.gameObject.CompareTag(player.movement.actionMarkerTag))
                        {
                            Debug.Log("Canceling action!");
                            player.movement.CancelPrimaryActions();

                        }
                    }
                }
            }
            }
        } else if (action.ACTIONID == 4)
        {
            // Place generic block from players hotbar 
            
            // Rotation
            if (Input.GetKeyDown(KeyCode.Q))
            {
                blockPlaceRoation = (Quaternion.Euler(blockPlaceRoation)*Quaternion.Euler(new Vector3(0, 90, 0))).eulerAngles;
            } else if (Input.GetKeyDown(KeyCode.E))
            {
                blockPlaceRoation = (Quaternion.Euler(blockPlaceRoation)*Quaternion.Euler(new Vector3(0, -90, 0))).eulerAngles;
            }

            // placing
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
                {
                    if (hit.collider.gameObject)
                    {
                        // Hit a tile
                        if (hit.collider.GetComponent<InGameTile>())
                        {
                            Tile T = hit.collider.gameObject.GetComponent<InGameTile>().tile;
                            
                         //Placing not on your tile and within range 
                            if (Vector3Int.Distance(player.movement.playerPos + Vector3Int.down, T.tilePos) <= action.actionRange && (player.movement.playerPos+Vector3Int.down)!=T.tilePos && !T.tags.Contains(Tile.TileTag.CanNotPlace))
                            {
                                //TODo fix issue where you can place where a neigher block is 
                                if (!Tile.DoesTileExist(T.tilePos+ Vector3Int.up) && T.FindNeighbors(new[] {Vector3Int.up }).Length==0)
                                {
                                    
                                    player.movement.CancelPrimaryActions();
                                    
                                    InventoryItem I = player.inventory.hotbar[player.inventory.hotbarSlot].item;

                                    GameAction place = Instantiate(action);

                                    place.extraDataV = T.pos + Vector3.up;
                                    place.extraDataO = I.relatedTile;
                                    place.extraDataO1 = I;
                                    
                                    player.actionsToProcessAtEndOfTurn.Remove(player.actionsToProcessAtEndOfTurn.Find(x => x.ACTIONID == action.ACTIONID));
                                    player.actionsToProcessAtEndOfTurn.Add(place);

                                    player.movement.tilePlaceMarker.transform.position = place.extraDataV;
                                }
                            }
                        }
                        else
                        {
                            // To cancel
                            if (hit.collider.gameObject.CompareTag(player.movement.actionMarkerTag))
                            {
                                Debug.Log("Canceling action!");
                                player.movement.CancelPrimaryActions();
                              
                            }
                        }
                    }
                }
            }
        } else if (action.ACTIONID == 5)
        {
            // Sleeping
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
                {
                    if (hit.collider.gameObject)
                    {
                        // Hit a tile
                        if (hit.collider.GetComponent<InGameTile>())
                        {
                            Tile T = hit.collider.gameObject.GetComponent<InGameTile>().tile;
                            //Already know its within range 

                            if (T.type == Tile.TileType.Bed)
                            {
                                // Sleep in the bed
                                player.movement.CancelPrimaryActions();
                                player.actionsToProcessAtEndOfTurn.Add(Instantiate(action));
                                player.movement.markerSleep.transform.position = player.transform.position + Vector3.up * player.transform.lossyScale.y*1.1f;
                            }
                        }
                        else
                        {
                            // To cancel
                            if (hit.collider.gameObject.CompareTag(player.movement.actionMarkerTag))
                            {
                                Debug.Log("Canceling action!");
                                player.movement.CancelPrimaryActions();
                              
                            }
                        }
                    }
                }
            }
        } else if (action.ACTIONID == 6)
        {
            // Player eating 

            // if you right click, eat the food and destroy the inventory item
            if (Input.GetMouseButtonDown(1))
            {
                if (player.inventory.hotbar[player.inventory.hotbarSlot].item.type== InventoryItemType.Food)
                {
                    player.ChangeHunger(player.inventory.hotbar[player.inventory.hotbarSlot].item.extraDataInt);
                    // Player just ate somehing raw
                    if (player.inventory.hotbar[player.inventory.hotbarSlot].item.extraData.Contains("RAW"))
                    {
                        player.ChangeStatus(FreeEntity.StatusType.Poisoned);
                    }

                    player.inventory.RemoveItemFromInventory(player.inventory.hotbar[player.inventory.hotbarSlot].item,1);
                  
                }
                else
                {
                    Debug.LogError("trying to eat somethign that isnt a food: " + player.inventory.hotbar[player.inventory.hotbarSlot].item.Name);
                }

            }


        }
        
    }
    
    public static void CompleteGameAction(GameAction action)
    {
        if (action.ACTIONID == 0)
        {
            
        } else if (action.ACTIONID == 1)
        {
            // Move
          
            player.movement.posToMoveTo = action.to;
            player.movement.actualPosToMoveTo = action.extraDataV;
            
            player.movement.StartCoroutine(player.movement.SmoothlyMovePlayer());
            player.movement.ShowPossibleMoves();
            
        } else if (action.ACTIONID == 2)
        {
            // CHOP

            if (player.currentTransfomation == null)
            {
                player.movement.treeToChop = (Tile)action.extraDataO;
                player.movement.enviro.ChopTree(player.movement.treeToChop);
                player.movement.treeToChop = null;
            }
        }
        else if (action.ACTIONID == 3)
        {
            // Dig ground

            if (player.currentTransfomation == null)
            {
                player.movement.tileToDig = (Tile)action.extraDataO;
                player.movement.tileToDig.JustDugMe();
                player.movement.tileToDig = null;

            }
            
        } else if (action.ACTIONID == 4)
        {
            //Place block from hotbar

            // Only if human
            if (player.currentTransfomation == null)
            {
                Tile T = Tile.MasterCreateTile(new Vector3(action.extraDataV.x, (action.extraDataV.y), action.extraDataV.z), ((Tile)action.extraDataO).type, 0, player.movement.enviro, out List<Tile> notNeeded, true, Quaternion.Euler(blockPlaceRoation));
                player.inventory.RemoveItemFromInventory((InventoryItem)action.extraDataO1, 1);
                player.inventory.UpdatedInventory(true);
                T.sourceObject.GetComponent<InGameTile>().playerPlaced = true;
            }

            
        } else if (action.ACTIONID == 5)
        {
            //Sleeping
            //Literally just wait

            if (action.extraDataV== Vector3.one)
            {
                //Shorthand for long sleep to remove transformation



                foreach(InventoryItem A in player.currentTransfomation.abilities)
                {
                    player.inventory.abilities.Remove(A);
                }

                player.currentTransfomation = null;
                player.ultimate = null;


                foreach(Transform T in player.transform.Find("PlayerBody").Find("Transformations"))
                {
                    T.gameObject.SetActive(false);
                }
                player.transform.Find("PlayerBody").Find("Transformations").GetChild(0).gameObject.SetActive(true);



                player.ChangeSanity(2);
                player.ChangeSleep(24);
                Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.HoursSlept, 24);
            }
            else
            {
                player.ChangeSanity(2);
                player.ChangeSleep(8);
                Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.HoursSlept, 8);
            }
            
          

        }
        else if (action.ACTIONID == 6)
        {
            //Seek target and move towards
            if (action.extraData == "Player")
            {
                // AI seeking player
                
                // Find player, get random direction from player, path towards that
                int tries = 18;

                Vector3Int location = player.movement.playerPos;
                Tile closests = null;
                if (Vector3Int.Distance( action.actionCaller.location, player.movement.playerPos)>10)
                {
                    List<Tile> stucturesPlacecd = Tile.tilesInWorld.FindAll(x => x.sourceObject.GetComponent<InGameTile>() != null && x.sourceObject.GetComponent<InGameTile>().playerPlaced);

                    if (stucturesPlacecd.Count == 0)
                    {
                        location = action.actionCaller.location + new Vector3Int((int)Random.Range(-10, 10), 0, (int)Random.Range(-10, 10));

                    }
                    else
                    {

                        // Find closest

                        float C = 9999999999999f;
                        foreach (Tile T in stucturesPlacecd)
                        {
                            if (Vector3.Distance(action.actionCaller.transform.position, T.sourceObject.transform.position) < C)
                            {
                                closests = T;
                                C = Vector3.Distance(action.actionCaller.transform.position, T.sourceObject.transform.position);
                            }
                        }
                        location = closests.tilePos;
                    }

                }
            

                bool cont = false;
                while (tries > 0 && !cont)
                {
                    Vector3Int direction = new Vector3Int(Random.Range(-1, 2), -1, Random.Range(-1, 2));

                    if (direction == Vector3Int.up)
                    {
                        tries--;
                        continue;
                    }
                    if (Tile.DoesTileExist(location + direction) && !Tile.DoesTileExist(location + direction + Vector3Int.up))
                    {
                        // Can move 
                        location += direction;
                        cont = true;
                    }
                    tries--;
                }

                action.actionCaller.MoveEntity(location,closests );
                
            }
           
        } else if (action.ACTIONID == 7)
        {
            // Attack player by AI with mellee

            if (Vector3.Distance(action.actionCaller.transform.position, player.transform.position) < action.actionRange)
            {
                // Is close enough to attack
                
                // Attack player through combat manager
                GameObject.FindObjectOfType<CombatManager>().AIDamagePlayer(int.Parse(action.extraData), action.actionCaller.gameObject, DamageType.Normal);
                
                
            }
            
        } else if (action.ACTIONID == 8)
        {
            if (Vector3.Distance(action.actionCaller.transform.position, player.transform.position) < action.actionRange)
            {
                // Is close enough to attack
                
                // Attack player through combat manager
                GameObject.FindObjectOfType<CombatManager>().AIDamagePlayer(int.Parse(action.extraData), action.actionCaller.gameObject, DamageType.Normal);
                
                
            }
        } else if (action.ACTIONID == 9)
        {
            if (Vector3.Distance(action.actionCaller.transform.position, player.transform.position) < action.actionRange)
            {
                // Is close enough to attack
                
                // Attack player through combat manager
                GameObject.FindObjectOfType<CombatManager>().AIDamagePlayer(int.Parse(action.extraData), action.actionCaller.gameObject, DamageType.Magic);
                
                
            }
        } 
    }
    
}


public enum ActionType
{
    None, 
    Movement, 
    TileEditing,
    Attack, 
    Consume, 
    Create,


}

public enum ActionTags
{
    
    None, 
    FreeOutsideCombat,
    PlayerAction,
    
}