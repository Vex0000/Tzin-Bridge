using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnviromentManager : MonoBehaviour
{
    // Generates enviroment and handles enviromental effects

    public Player player;
    
    public int islandSize;
    public int tileSize;
    public float grassRadius;

    public float y0;
    
    public int seed;

    public bool useRandomSeed;

    private Random.State ordinaryRand;

    public float noiseScale;
    public Vector2 offset;
    
    public List<Tile> tilesToSpawnSandAround = new List<Tile>();
    
    public Transform tileParent;


    public AnimationCurve lightIntensity;

    public AnimationCurve colourChange;
    public Gradient enviromentLight;

    public Light mainLight;
    
    public Vector2Int[] minMaxChestsSpawn;

    public GameObject[] chests;

    public Transform chestParent;

    public static bool isDay;

    public Object navmeshThingy;

    public Tile[] theSand;

    public CombatManager combatMan;

    public Tile newestBottomDockPiece;

    public GameObject loadingScreen;

    public float dayNightCyclePercentage;
    


    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(TurnOn());
        /*
        loadingScreen.SetActive(true);
        Tile.CompileListOfAllTiles();
        GenerateIsland();
        loadingScreen.SetActive(false);
        */
    }
    

    public IEnumerator TurnOn()
    {

        yield return new WaitForSeconds(.1f);
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(.1f);
        Tile.CompileListOfAllTiles();
        GenerateIsland();
        yield return new WaitForSeconds(.1f);
        loadingScreen.SetActive(false);
        Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.TimesOpenedGame, 1);

    }

    // Change this function to just use copies of the master tile list 

    // Update is called once per frame
    void Update()
    {
     
    }


  
    public void GrowDock()
    {
          
        // remove pieces that it expands into
        
        if (Tile.DoesTileExist(newestBottomDockPiece.tilePos +new Vector3Int(1,0,0) ))
        {
            Tile.tilesInWorld.Find(x => x.tilePos == newestBottomDockPiece.tilePos + new Vector3Int(1,0,0)).RemoveTile();
        }
        
        if (Tile.DoesTileExist(newestBottomDockPiece.tilePos + new Vector3Int(1,0,1)))
        {
            Tile.tilesInWorld.Find(x => x.tilePos == newestBottomDockPiece.tilePos + new Vector3Int(1,0,1)).RemoveTile();
        }
        if (Tile.DoesTileExist(newestBottomDockPiece.tilePos + new Vector3Int(2, 0, 1)))
        {
            Tile.tilesInWorld.Find(x => x.tilePos == newestBottomDockPiece.tilePos + new Vector3Int(2, 0, 1)).RemoveTile();
        }
        if (Tile.DoesTileExist(newestBottomDockPiece.tilePos + new Vector3Int(2, 0, 0)))
        {
            Tile.tilesInWorld.Find(x => x.tilePos == newestBottomDockPiece.tilePos + new Vector3Int(2, 0, 0)).RemoveTile();
        }


        // Add the new dock pieces

        List<Tile> dummyList;
        
        newestBottomDockPiece =   Tile.MasterCreateTile(newestBottomDockPiece.pos+ new Vector3(2.5f,0,0), Tile.TileType.Dock, 0, this, out dummyList, true, Quaternion.identity);
        
        //Tile.MasterCreateTile(newestBottomDockPiece.pos+ new Vector3(0,0,1), Tile.TileType.Dock, 0, this, out dummyList, true, Quaternion.identity);

    }


    public void HandleDayNightCycle()
    {
         dayNightCyclePercentage = ((((float)TurnManager.numberOfHoursPassed % (float)TurnManager.dayNightCyclePeriod) / (float)TurnManager.dayNightCyclePeriod) + TurnManager.cyclePhaseOffset)%1f;
        // 0 is midday, .5 is night
     

        float intensity = lightIntensity.Evaluate(dayNightCyclePercentage);

        Color enviroColor = enviromentLight.Evaluate(colourChange.Evaluate(dayNightCyclePercentage));

        mainLight.intensity = intensity;
        mainLight.color = enviroColor;
        

    }

    
    public void ChopTree(Tile Tree)
    {
        InventoryItem item;
        
        int indexofDroppedItem = Random.Range(0, Tree.itemsToDrop.Length);
       

        if (player.inventory.FindInventoryItemFromName(Tree.itemsToDrop[indexofDroppedItem].Name, out item))
        {
          
            player.inventory.AddItemToInven(item, Random.Range(Tree.numberOfItemsToDrop[ indexofDroppedItem].x,Tree.numberOfItemsToDrop[indexofDroppedItem].y) );
        }
        Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.TreesChopped, 1);

        Tree.DestroyTile();
    }
    
    
    public void GenerateIsland()
    {

        // Handle randoms
        
        if (useRandomSeed)
        {
            seed = Random.Range(-99999, 99999);
            useRandomSeed = false;
        }

        ordinaryRand = Random.state;
        Random.InitState(seed);

        // Create the top layer grass, of which sand will be spawned from 
        int x = -islandSize;
        int z = -islandSize;
        float a;
        int i = 0;
        Tile.firstGen = true;
        
        while (z < islandSize)
        {
            x = -islandSize;
            while (x < islandSize)
            {
                // Generates a circle of grass
                if ((x * x + z * z) > grassRadius * grassRadius)
                {
                    x++;
                    continue;
                }
                
                List<Tile> tilesCreated;
                
                a = Mathf.PerlinNoise((x+offset.x)*noiseScale, (z+offset.y)*noiseScale);
                
               Tile T = Tile.MasterCreateTile(new Vector3(x * tileSize, (y0 + a), z * tileSize), Tile.TileType.Grass, 0, this, out tilesCreated, false, Quaternion.identity);
               
                tilesToSpawnSandAround.Add(T);

                x++;
            }
            z++;
        }
        
        
        // Generate neighbors to give sand
        while (i < Tile.tilesInWorld.Count)
        {
            Tile.tilesInWorld[i].GenerateNeighborList(false);
            i++;
        }

        
        // Tell each to spawn sand
        i = 0;
        while (i < tilesToSpawnSandAround.Count)
        {
           tilesToSpawnSandAround[i].SpawnSandTilesAround();
           i++;
        }
        
        List<Tile> allSandTilesSpawned = Tile.tilesInWorld.FindAll(x4 => x4.type == Tile.TileType.Sand);

        theSand = allSandTilesSpawned.ToArray();
        SpawnChests(allSandTilesSpawned);
        
        
        
        // Now take care of dock spawn system

        Tile mostEasternSpawnTile = null;
        Vector3 tilePos = Vector3.zero;
     

        List<Tile> possibleLocations = allSandTilesSpawned.FindAll(x3 => ((x3.tilePos.y == 0) && (x3.tilePos.x > 0) && (x3.tilePos.z==0)));
    

        foreach (Tile P in possibleLocations)
        {
            
            if (P.tilePos.x > tilePos.x)
            {
                mostEasternSpawnTile = P;
                tilePos = P.tilePos;
            }
        }
      
        // Now have spawn location, move on to spawn dock 

        // First remove tiles to spawn dock tiles instead
        
        Vector3Int secondTilePos = mostEasternSpawnTile.tilePos + new Vector3Int(0, 0, 1);
        
        Vector3Int dockTile = mostEasternSpawnTile.tilePos;
        Vector3 secondTilePosf = tilePos + new Vector3(0, 0, 1) * tileSize;
        
        if (Tile.DoesTileExist(secondTilePos))
        {
            
            Tile STPF = Tile.tilesInWorld.Find(x => x.tilePos == secondTilePos);
            secondTilePosf = STPF.pos;
            
            STPF.RemoveTile();
        }
        
        Tile.tilesInWorld.Find(x=>x.tilePos == dockTile).RemoveTile();
        
        // Then replace them with new tiles

        List<Tile> dummyList;
        
        newestBottomDockPiece =   Tile.MasterCreateTile(tilePos, Tile.TileType.Dock, 0, this, out dummyList, false, Quaternion.identity);
        
        //Tile.MasterCreateTile(secondTilePosf, Tile.TileType.Dock, 0, this, out dummyList, false, Quaternion.identity);
        


        // Then surround all with water for long period
       
         x = -islandSize*2;
         z = -islandSize*2;
         i = 0;

        while (z < islandSize*2) 
        {
            x = -islandSize*2;
            while (x < islandSize*2)
            {

                if (Tile.DoesTileExist(new Vector3Int(Mathf.RoundToInt(x), 0, Mathf.RoundToInt(z))))
                {
                    x++;
                    continue;
                }
                
                     
                Tile T = Tile.MasterCreateTile(new Vector3(x*tileSize, (y0), z*tileSize), Tile.TileType.Water, 0, this, out List<Tile> notNeeded, false, Quaternion.identity);
                /*
                Tile T = ScriptableObject.CreateInstance<Tile>();
                
                T.pos = new Vector3(x*tileSize, (y0), z*tileSize);
                T.type = Tile.TileType.Water;
                T.layer = 0;

                T.CreateTile(this);
                */
                //Tile.tilesInWorld.Add(T);
                tilesToSpawnSandAround.Add(T);

                x++;
            }
            z++;
        }
        
        // Set tiles into tile object for easy heiracry manipulation adn assign tile temps
        foreach (Tile T in Tile.tilesInWorld)
        {
            if (T.spawnedObject)
            {
                T.spawnedObject.transform.SetParent(tileParent);
            }
            if (T.type== Tile.TileType.Water)
            {
                T.temp = Tile.TileTemp.Cold;
            }
        }
        
        // Combine meshes, not operable right now due to navigation
        tileParent.GetComponent<CombineMeshes>().Combine();
        
        // Make the navigation 
        tileParent.GetComponent<NavMeshSurface>().BuildNavMesh();
        
        // Spawn initial enemies
        combatMan.SpawnEnemies();

        // Restart randomness, set flags for later terrain modification, and start the player loop
        Tile.firstGen = false;
        Random.state = ordinaryRand;
        player.movement.StartTurn();
    }


    public void SpawnChests(List<Tile> sand)
    {


        int k = 0;

        while (k < chests.Length)
        {
            int i = 0;

            int c = Random.Range(minMaxChestsSpawn[k].x, minMaxChestsSpawn[k].y);

            while (i < c)
            {
                Tile T;
                int g = 5;
            
                while (g != 0)
                {
                    T = sand[Random.Range(0, sand.Count)];
            
                    if (T.spawnedObject!=null && T.FindNeighbors(new[] {Vector3Int.up }).Length==0)
                    {
                        GameObject newChest = Instantiate(chests[k],T.spawnedObject.transform.position, chests[k].transform.rotation, chestParent);
                        break;
                    }
                    else
                    {
                        g--;
                        T = sand[Random.Range(0, sand.Count)];
                    }
                }
            
                i++;
            }

            k++;

        }
     
    }
    
    
}
