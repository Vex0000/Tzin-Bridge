using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SearchService;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FreeEntity : MonoBehaviour
{

    public int health;

    public int maxHealth;

    public int currentArmour;

    public int maxActionPoints;

    public int actionPoints;

    public Vector3Int location;

    public List<GameAction> availableInnateActions;

    public List<GameAction> actionsToDoAtEndOfTurn = new List<GameAction>();

    public int uniqueID = 0;

    public List<StatusType> currentStatus = new List<StatusType> { StatusType.Nominal };
    public List<int> statusTimes = new List<int> { 0 };

    public List<GameObject> statusParticles = new List<GameObject>();
    public Transform particleSystemSpawn;

    public UIManager UI;

    public static int[] statusStartTimes = { -1, 3, 3, 2, 2, 0, 1 };


    public void OnEnable()
    {
        if (uniqueID == 0)
        {
            uniqueID = Random.Range(0, int.MaxValue);
            currentStatus  = new List<StatusType> { StatusType.Nominal };
        }
    }

    public void ChangeStatus(StatusType newStatus)
    {

        if (newStatus == StatusType.Nominal)
        {
            statusTimes = new List<int> { 0 };
            currentStatus =  new List<StatusType>{ StatusType.Nominal};

            foreach (GameObject G in statusParticles)
            {
                Destroy(G, .00000001f);
            }
                
            return;
        }

        // If contains nominal, then is all fine. Clear list and add status
        if (currentStatus.Contains(StatusType.Nominal)){
            currentStatus.Clear();
            currentStatus = new List<StatusType> { newStatus };
            statusTimes[0] = statusStartTimes[(int)newStatus];
            if (UI == null)
            {
                UI = GameObject.FindObjectOfType<UIManager>();
            }
            statusParticles.Add(Instantiate( UI.statusParticles[(int)newStatus], particleSystemSpawn));
        }
        else
        { // Then entity already has a status effect

            // Already present
            if (currentStatus.Contains(newStatus))
            {
                int j = currentStatus.IndexOf(newStatus);
                statusTimes[j] += statusStartTimes[(int)newStatus];
            }
            else
            {
                currentStatus.Add(newStatus);
                int j = currentStatus.Count();
                statusTimes.Add(statusStartTimes[(int)newStatus]);
                if (UI == null)
                {
                    UI = GameObject.FindObjectOfType<UIManager>();
                }
                statusParticles.Add(Instantiate(UI.statusParticles[(int)newStatus], particleSystemSpawn));
            }
          
        }

     
        if (newStatus== StatusType.Fearful)
        {

            // Move enemy away
            Vector3 enemPos = GameObject.FindObjectOfType<Player>().gameObject.transform.position;

            Vector3 OppDir = -(enemPos - transform.position).normalized;

            Vector3 dtilesf = OppDir * availableInnateActions.Find(x => x.ACTIONID == 6).actionRange;
            Vector3Int dtiles = new Vector3Int((int)dtilesf.x, (int)dtilesf.y, (int)dtilesf.z);

            Vector3Int newLoc = location + dtiles;

            if (Tile.DoesTileExist(newLoc))
            {
                MoveEntity(newLoc, null);
            }
            else
            {
                int t = 5;
                bool keepGoing = true;
                while (t != 0 && keepGoing)
                {

                    newLoc = new Vector3Int(newLoc.x + Random.Range(-1, 2), newLoc.y , newLoc.z + Random.Range(-1, 2));

                    if (Tile.DoesTileExist(newLoc))
                    {
                        keepGoing = false;
                    }
                    t--;
                }

                if (keepGoing)
                {
                    TakeDamage(2, gameObject, DamageType.Normal);
                }
                else
                {
                    MoveEntity(newLoc, null);
                }
            }
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
      
    }
 
    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int amount, GameObject damager, DamageType type)
    {
        
        if (currentStatus.Contains( StatusType.Frozen))
        {
            amount *= 2;
        }

        int remainingAmount = 0;
        
        if (currentArmour > 0)
        {
            // If no armour, apply full damage
            remainingAmount = amount;
        }
        else
        {
            // if any amount of non zero armour, apply armour logic 
            
            // halve the damage
            int halfAmount = amount / 2;
      
            // Clamp it to the armour amount
            if (halfAmount > currentArmour)
            {
                halfAmount = currentArmour;
            }

            // Calculate how much armour will be broken. Armour negates damage but has a 50% chance to break in process 
            int armourbroken = 0;
            int i = 0;
        
            while (i < halfAmount)
            {
                if (Random.value > .5f)
                {
                    armourbroken++;
                }
                i++;
            }

            currentArmour -= armourbroken;
            
            remainingAmount = amount - halfAmount;

        }

        health -= remainingAmount;
      
        if (health <= 0)
        {
            Die();
            return;
        }

        if (type == DamageType.Fire && Random.value<(CombatManager.ChanceOfFireDamageToCauseFirePerDamage * remainingAmount))
        {
            ChangeStatus(StatusType.OnFire);
        } else if (type == DamageType.Electrical && Random.value < (CombatManager.ChanceOfElecctricalDamageToCauseStunPerDamage * remainingAmount))
        {
            ChangeStatus(StatusType.Stunned);
        }
        else if (type == DamageType.Ice && Random.value < (CombatManager.ChanceOfIceDamageToCauseFrozenPerDamage * remainingAmount))
        {
            ChangeStatus(StatusType.Frozen);
        }
        else if (type == DamageType.Nature && Random.value < (CombatManager.ChanceOfNatureDamageToCausePoisonPerDamage * remainingAmount))
        {
            ChangeStatus(StatusType.Poisoned);
        }
        else if (type == DamageType.DarkMagic && Random.value < (CombatManager.ChanceOfDarkDamageToCauseFearPerDamage * remainingAmount))
        {
            ChangeStatus(StatusType.Fearful);
        }

    }

    public void TakeDamageNoArmour(int amount, GameObject damager)
    {

        if (currentStatus.Contains( StatusType.Frozen))
        {
            amount *= 2;
        }
        
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

        if (GameObject.FindObjectOfType<CombatManager>().enemiesSpawned.Contains(this))
        {
            GameObject.FindObjectOfType<CombatManager>().enemiesSpawned.Remove(this);
        }
        
        GameObject.FindObjectOfType<EnviromentManager>().GrowDock();
        
        Destroy(gameObject);
        
    }
    
    
    public virtual void ProcessTurn()
    {

       
        

        if (currentStatus.Contains( StatusType.OnFire))
        {
            TakeDamageNoArmour(1, gameObject);
        } 
        if (currentStatus.Contains( StatusType.Poisoned))
        {
            TakeDamageNoArmour((int)((float)maxHealth*.1f), gameObject);
        }
        
        
        if (!currentStatus.Contains( StatusType.Nominal))
        {
            int i = 0;

            while (i< statusTimes.Count())
            {

                statusTimes[i]--;

                if (statusTimes[i] <= 0)
                {
                    currentStatus.RemoveAt(i);
                    statusTimes.RemoveAt(i);
                    Destroy(statusParticles[i]);
                    statusParticles.RemoveAt(i);
                    if (currentStatus.Count == 0)
                    {
                        currentStatus.Add(StatusType.Nominal);
                        statusTimes.Add(0);
                        break;
                    }
                    continue;
                }

                i++;
            }
         
        }
        
    }
    public void MoveEntity(Vector3Int newLocation, Tile TileToAttack)
    {

        if (!currentStatus.Contains(StatusType.Frozen) && !currentStatus.Contains( StatusType.Stunned) && !currentStatus.Contains( StatusType.Trapped))
        {
            StartCoroutine(MoveMe(newLocation, TileToAttack));
        }
      
    }

    public IEnumerator MoveMe(Vector3Int newLocation, Tile TileToAttack)
    {
        GameAction movement = availableInnateActions.Find(x=> x.type == ActionType.Movement);

        float maxDist = movement.actionRange;

        NavMeshAgent ag  =   gameObject.GetComponent<NavMeshAgent>();
        Tile tileToMoveTo = Tile.tilesInWorld.Find(x => x.tilePos == (newLocation));
        // Set dest
        ag.destination = tileToMoveTo.pos + Vector3.up;

          while (ag.pathPending)
          {
              yield return new WaitForSeconds(Time.deltaTime);
          }

          NavMeshHit hit;
          if (ag.SamplePathPosition(ag.areaMask, maxDist, out hit))
          {
              // Close engogugh to reach player, dont care
              
          }
          else
          {
              // Update position to max length
              ag.destination = hit.position;
              // Need to find new tile pos
              Collider[] cols= Physics.OverlapSphere(hit.position, 1);

              float mindist = 0;
              Tile T = null;
              foreach (Collider col in cols)
              {
                  if (col.gameObject && col.gameObject.GetComponent<InGameTile>())
                  {
                      float d = Vector3.Distance(hit.position, col.gameObject.transform.position);
                      if (d < mindist)
                      {
                          mindist = d;
                      }

                      T = col.gameObject.GetComponent<InGameTile>().tile;
                  }
              }

              if (T != null)
              {
                  newLocation = T.tilePos + Vector3Int.up;
              }
              
          }
          
          while (ag.pathPending)
          {
              yield return new WaitForSeconds(Time.deltaTime);
          }

          while (ag.remainingDistance > ag.stoppingDistance)
          {
              yield return new WaitForSeconds(Time.deltaTime);
          }

          location = newLocation;

        // Now attack tile

        if (TileToAttack != null)
        {

            if (Random.value < .3333f)
            {
                TileToAttack.DestroyTile();
            }
        }
    }

    public enum StatusType
    {
        Nominal,
        OnFire,
        Frozen,
        Stunned,
        Poisoned,
        Fearful,
        Trapped,
        Pet
        
    }
    
}
