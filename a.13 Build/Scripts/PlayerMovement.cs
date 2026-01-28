using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{

    public Vector3Int playerPos;

    public Vector3Int posToMoveTo;

    public Vector3 actualPosToMoveTo;

    public float speed;

    public float actionRange;

    public Vector3 offset;
    

    public bool CanProcessInput = true;


    public List<GameObject> canGoMarkers = new List<GameObject>();

    public GameObject willGoTo;
    public GameObject willMine;
    public GameObject willChop;
    public GameObject tilePlaceMarker;
    public GameObject markerSleep;

    public EnviromentManager enviro;
    public CombatManager combatManager;
    
    public Tile treeToChop;
    public Tile tileToDig;

    public Player player;

    public float playerMovementInTilesPerSecond;

    public string movementMarkerWGTag;
    public string actionMarkerTag;
    public string combatMarkerTag;
    public string placeableEntityTag;

    public Transform canGoMarkersParent;

    // Start is called before the first frame update
    void Start()
    {
        var position = transform.position;
        posToMoveTo = new Vector3Int((int)position.x, 1, (int)position.z);
        actualPosToMoveTo = posToMoveTo;
        playerPos = posToMoveTo;
        transform.position = actualPosToMoveTo + offset;
        StartTurn();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    // TODO write cancel movement function
    
    public void ProcessInput()
    {
        if (!CanProcessInput)
        {
            return;
        }

   
        
        // Select chest
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
           // Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction*400, Color.red, 10f);
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
            {
                if (hit.collider.gameObject)
                {
                    // Hit a tile
                    if (!hit.collider.GetComponent<InGameTile>())
                    {
                        // Hit static entity
                        if (hit.collider.gameObject.CompareTag(placeableEntityTag))
                        {
                          //  Debug.Log("Hit a placeable entity");

                            // It was a chest
                            if (hit.collider.gameObject.GetComponent<Chest>() &&Vector3.Distance(playerPos + Vector3Int.down, hit.collider.gameObject.transform.position) <= actionRange )
                            {
                                hit.collider.gameObject.GetComponent<Chest>().items.Interact();
                            }
                        }
                    }
                }
            }
        }
    }

    public void CancelPrimaryActions()
    {
        
        CancelMovement();
        CancelChopTree();
        CancelDigGround();
        CancelPlaceBlock();
        CancelSleep();
    }

    public void CancelSleep()
    {
        player.actionsToProcessAtEndOfTurn.Remove(player.actionsToProcessAtEndOfTurn.Find(x => x.ACTIONID == 5));
        markerSleep.transform.position = Vector3.down * 999;
    }
    public void CancelPlaceBlock()
    {
        tilePlaceMarker.transform.position = Vector3.down * 99999;
    }

    public void CancelMovement()
    {
        posToMoveTo = playerPos;
        actualPosToMoveTo = posToMoveTo;
        willGoTo.transform.position = Vector3.down * 9999;

        player.actionsToProcessAtEndOfTurn.Remove(player.actionsToProcessAtEndOfTurn.Find(x => x.type == ActionType.Movement));
    }

    public void CancelChopTree()
    {
        
        treeToChop = null;
        willChop.transform.position = Vector3.down * 9999;
        player.actionsToProcessAtEndOfTurn.Remove(player.actionsToProcessAtEndOfTurn.Find(x => x.ACTIONID==2));
    }

    public void CancelDigGround()
    {
        tileToDig = null;
        willMine.transform.position = Vector3.down * 9999;
        player.actionsToProcessAtEndOfTurn.Remove(player.actionsToProcessAtEndOfTurn.Find(x => x.ACTIONID==3));
        
    }

    public IEnumerator SmoothlyMovePlayer()
    {

        CanProcessInput = false;
      
        float t = 0;
        float t1 = (posToMoveTo - playerPos).magnitude / playerMovementInTilesPerSecond * .5f;

        player.Anim.SetBool("IsWalking", true);
        while (t < t1)
        {  
            //Camera.main.gameObject.GetComponent<CameraFollow>().StartResetToTarget();
            transform.position = Vector3.Lerp(transform.position, actualPosToMoveTo + offset, t / t1);
            yield return new WaitForSeconds(Time.deltaTime);
            t += Time.deltaTime;
        }

        player.Anim.SetBool("IsWalking", false);

        transform.position = actualPosToMoveTo + offset;
     
        playerPos = posToMoveTo;

        Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.BlocksMoved, (int)(posToMoveTo - playerPos).magnitude);

        CanProcessInput = true;
        ShowPossibleMoves();
    }
    
    public void DoMovementOutsideOfCombat()
    {
        StartCoroutine(SmoothlyMovePlayer());
    }
    
    public void StartTurn()
    {

        if (tileToDig == null)
        {
            if (treeToChop == null)
            { 
                StartCoroutine(SmoothlyMovePlayer());
            }
            else
            {
                enviro.ChopTree(treeToChop);
                treeToChop = null;
            }
        }
        else
        {
            tileToDig.JustDugMe();
            tileToDig = null;
        }

        ShowPossibleMoves();


    }

    public void ShowPossibleMoves()
    {

        GameObject canGomarker = Resources.Load<GameObject>("Prefabs/TileCanGoMarker");
        // clear leftover markers
        foreach (GameObject G in canGoMarkers)
        {
            Destroy(G);
        }
        canGoMarkers.Clear();

        if (combatManager.playerIsInCombat)
        {
               
            int i = 0;
            while (i < Tile.tilesInWorld.Count)
            {
                if (Tile.tilesInWorld[i].tags.Contains(Tile.TileTag.Walkable) && (Tile.tilesInWorld[i].tilePos - (playerPos+Vector3Int.down)).magnitude<= (combatManager.playerIsInCombat?speed:200) )
                {
                    canGoMarkers.Add(Instantiate(canGomarker, Tile.tilesInWorld[i].pos, Quaternion.identity, canGoMarkersParent));
                }
            
                i++;
            }
        }
     

        willGoTo.transform.position = Vector3.down * 999999;

    }
    
}
