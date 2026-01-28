using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatManager : MonoBehaviour
{

    public bool playerIsInCombat;

    public GameObject[] enemyPool;

    public Vector2Int[] minmaxEnemiesToSpawn;

    public float[] chanceOfEachEnemySpawning;

    public EnviromentManager enviro;


    public List<FreeEntity> enemiesSpawned = new List<FreeEntity>();

    public UIManager uiMan;

    public float combatRangeForTurns = 7f;

    public float melleRange = 1.4f;

    public static float ChanceOfFireDamageToCauseFirePerDamage = .3f;
    public static float ChanceOfIceDamageToCauseFrozenPerDamage = .3f;
    public static float ChanceOfNatureDamageToCausePoisonPerDamage = .3f;
    public static float ChanceOfDarkDamageToCauseFearPerDamage = .3f;
    public static float ChanceOfElecctricalDamageToCauseStunPerDamage = .3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // Switch mode to combat mode if enemes within combat range
        if (!uiMan.combatManage.playerIsInCombat)
        {
            
            if (enemiesSpawned.Any(E => Vector3.Distance(enviro.player.transform.position, E.transform.position) < combatRangeForTurns))
            {
                // is false and switches to true
                uiMan.combatManage.playerIsInCombat = true;
            
                enviro.player.movement.ShowPossibleMoves();
            } 
            // is false and stays false, dont care 
        }
        else
        {

            if (!enemiesSpawned.Any(E => Vector3.Distance(enviro.player.transform.position, E.transform.position) < combatRangeForTurns))
            {
                // is true and switches to false
                uiMan.combatManage.playerIsInCombat = false;
            
                enviro.player.movement.ShowPossibleMoves();
            } 
            // is true and stays true, dont care
        }
       
            
      
       
        
        // trying to attack
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Raycast to find what they clicked on
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400))
            {
                // if it exists
                if (hit.collider.gameObject && hit.collider.gameObject.CompareTag("Enemy"))
                {
                   uiMan.ShowCombatActions(hit.collider.gameObject.transform.position, hit.collider.gameObject);
                }
            }
        }
    }

    public void PlayerUltimateAttack(GameObject enemy)
    {
        
       
        if (enviro.player.ultimate.ID == 18)
        {
            // Electricute
            
            enemy.GetComponent<FreeEntity>().TakeDamage(enviro.player.ultimateDamage, enviro.player.gameObject, DamageType.Electrical);
            enviro.player.playerHasUltimatedThisTurn = true;

            List<FreeEntity> enemiesInRange = enemiesSpawned.FindAll(x => x != enemy.GetComponent<FreeEntity>() && Vector3.Distance(enemy.transform.position, x.transform.position) < enviro.player.ultimateRange);

          
            // have to do this mess to prevent the list size frok changing
            while (enemiesInRange.Count != 0)
            {
               int IDl = enemiesInRange[0].uniqueID;
               
                enemiesInRange[0].TakeDamage(enviro.player.ultimateDamage/2, enviro.player.gameObject, DamageType.Electrical);

                if (enemiesInRange.Find(x => x.uniqueID == IDl))
                {
                    enemiesInRange.Remove(enemiesInRange.Find(x => x.uniqueID == IDl));
                }
            }
          
            
        }
        
        enviro.player.inUltimateCooldown = true;
        enviro.player.UcoolDownRemaining = enviro.player.ulimateCooldown;
    }
    
    public void AIDamagePlayer(int amount, GameObject source, DamageType type)
    {
      int damagetaken =  enviro.player.TakeDamage(amount, type);

        if (damagetaken > 0)
        {
            enviro.player.ChangeSanity(-1);
        }
        
    }
    public void PlayerBasicAttack(GameObject enemy)
    {
        DamageType T = DamageType.Normal;

        InventoryItem WP = enviro.player.selectedWeapon;

        if (WP.tags.Contains(InventoryTag.FireDamage))
        {
            T = DamageType.Fire;
        }  else if (WP.tags.Contains(InventoryTag.IceDamage))
        {
            T = DamageType.Ice;
        }
        else if (WP.tags.Contains(InventoryTag.NatureDamage))
        {
            T = DamageType.Nature;
        }
        else if (WP.tags.Contains(InventoryTag.ElectricalDamage))
        {
            T = DamageType.Electrical;
        }
        else if (WP.tags.Contains(InventoryTag.DarkMagicDamage))
        {
            T = DamageType.DarkMagic;
        }
        else if (WP.tags.Contains(InventoryTag.MagicDamage))
        {
            T = DamageType.Magic;
        }

        enemy.GetComponent<FreeEntity>().TakeDamage(enviro.player.playerDamage, enviro.player.gameObject, T);
        enviro.player.playerHasBasicAttackedThisTurn = true;
    }

    public void PlayerAbiltyAttack(GameObject enemy)
    {
        
        // Require this approach to use animations 
        
        
        if (enviro.player.selectedAbilty.ID == 17)
        {
            // Fire throw 
            enemy.GetComponent<FreeEntity>().TakeDamage(enviro.player.abilityDamage, enviro.player.gameObject, DamageType.Fire);
            enviro.player.firePunch.Play();
            enviro.player.playerHasAbilitiesThisTurn = true;
        } else if (enviro.player.selectedAbilty.ID == 16)
        {
            // Slam 
            enemy.GetComponent<FreeEntity>().TakeDamage(enviro.player.abilityDamage, enviro.player.gameObject, DamageType.Normal);
            enviro.player.playerHasAbilitiesThisTurn = true;
            enviro.player.movement.posToMoveTo = enemy.GetComponent<FreeEntity>().location;
            enviro.player.movement.actualPosToMoveTo = enemy.transform.position;
            enviro.player.movement.StartCoroutine(enviro.player.movement.SmoothlyMovePlayer());
            
        }

        enviro.player.inAbilityCoooldown = true;
        enviro.player.AcoolDownRemaining = enviro.player.abilitycooldown;
    }

    public void SpawnEnemies()
    {
        int i = 0;

        List<Tile> alreadySpawnedTiles = new List<Tile>();
        while (i < enemyPool.Length)
        {

            if (Random.value > chanceOfEachEnemySpawning[i])
            {
                // Spawn enemy

                int numToSpawn = Random.Range(minmaxEnemiesToSpawn[i].x, minmaxEnemiesToSpawn[i].y);

                Tile[] possibleSpawnTiles = enviro.theSand;
                if (possibleSpawnTiles.Length == 0)
                {
                    return;
                }

                while (numToSpawn > 0)
                {
                    int I = Random.Range(0, possibleSpawnTiles.Length);
                  
                    Tile randomSpawn = possibleSpawnTiles[I];

                    if (!alreadySpawnedTiles.Contains(randomSpawn))
                    {
                        // Spawn here

                        FreeEntity G1 = Instantiate(enemyPool[i], randomSpawn.pos + Vector3.up, Quaternion.identity).GetComponent<FreeEntity>();
                        enemiesSpawned.Add(G1);
                        
                        alreadySpawnedTiles.Add(randomSpawn);
                    }
                    numToSpawn--;
                }
               

            }
            i++;
        }

    }

    public void EnemyDestroyed()
    {
        enviro.GrowDock();
    }
}
