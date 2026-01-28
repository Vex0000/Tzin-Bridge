using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class Tile : ScriptableObject
{
    
    // The object insatniate in the world
    public GameObject sourceObject;
    
    // The world space position of the tile object
    public Vector3 pos;

    //World toation
    public Quaternion rotation;

    // Unused as of now, will be a list of all the tiles taken up by this item
    public Vector3[] relativeTilesTakenUp;

    // Its position of the lower left corner
    public Vector3Int tilePos;
    
    // List of neighbors
    public List<Neighbor> neighbors = new List<Neighbor>();
    
    // This tiles type
    public Tile.TileType type = TileType.Nothing;
    
    // Weather it is visible to the player
    public bool visible;
    
    // A postive layer means depth below surface, surface is 0, and a negative depth is above the surface
    public int layer;
    
    // Actual object if spawned
    public GameObject spawnedObject;

    // The tile tags 
    public TileTag[] tags;

    // Items to drop when destroyed
    public InventoryItem[] itemsToDrop;

    public Vector2Int[] numberOfItemsToDrop;

    public int uniqueID = 0;

    public TileTemp temp = TileTemp.Normal;
    
    
    
    
    public static Tile[] MasterTileList;
    public static EnviromentManager enviro;

    
    public void OnEnable()
    {
        if (uniqueID == 0)
        {
            uniqueID = Random.Range(0, int.MaxValue);
        }
    }
    
    public static void CompileListOfAllTiles()
    {
        MasterTileList = Resources.LoadAll<Tile>("Tiles/");
        
        numberOfEachTile = new float[MasterTileList.Length];
    }


    
    
    public static List<Tile> tilesInWorld = new List<Tile>();
    

    // Change number of tile system 
    public static float waterChance = .7f;
    public static int maxNumWaterTiles = 2000;

    public static float grassChance = .5f;
    public static int maxNumGrassTiles = 5000;

    public static float ironChance = .3f;
    public static int maxNumIronTiles = 1000;

    public static float sandChance = .55f;
    public static int maxNumSandTiles = 2500;

    public static float treeChance = .06f;
    public static int maxNumTrees = 1000;


    public static float[] numberOfEachTile;

    // Basically the max number of tiles each tile can spawn
    public static int maxMasterNumTrys = 6;

    // The max number of trys it does to spawn one tile
    public static int maxMinNumTrys = 9;

    
    public static bool firstGen;

 

    public void OnCreate()
    {
     
        
    }

    public void DestroyTile()
    {
        // Destroys tile and removes all references to it

        int i = 0;
        while (i < neighbors.Count)
        {
            Neighbor n = neighbors[i].theTile.neighbors.Find(x => x.theTile == this);
            neighbors[i].theTile.neighbors.Remove(n);
            i++;
        }
        
        tilesInWorld.Remove(this);
        i = 0;
        while (i < neighbors.Count)
        {
            neighbors[i].theTile.GenerateNeighborList(true);
            if (neighbors[i].theTile.spawnedObject == null )
            {
                GameObject G= neighbors[i].theTile.SpawnTheTile();
                G.transform.SetParent(spawnedObject.transform.parent);
            }
            i++;
        }
        
        Destroy(spawnedObject);
        Destroy(this);

    }


    public void GenerateNeighborList(bool justRefresh)
    {
        
        // generates a list of tiles that directly touch this one
       
        if (!justRefresh)
        { 
            neighbors.Clear();
            int i = 0;

            while (i < tilesInWorld.Count)
            {
                Vector3Int diff = tilesInWorld[i].tilePos - tilePos;
                if (Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1 && Mathf.Abs(diff.z) <= 1 )
                {
                    Neighbor N = new Neighbor
                    {
                        normalizedPos = diff,
                        theTile = tilesInWorld[i]
                    };
                    neighbors.Add(N);
                }
                
                i++;
            }
        }
        else
        {
            neighbors.Clear();
            // same thing for now
            int i = 0;

            while (i < tilesInWorld.Count)
            {
                Vector3Int diff = tilesInWorld[i].tilePos - tilePos;
                if (Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1 && Mathf.Abs(diff.z) <= 1)
                {
                    Neighbor N = new Neighbor();
                    N.normalizedPos = diff;
                    N.theTile = tilesInWorld[i];
                    neighbors.Add(N);
                }
                
                i++;
            }
        }
    }
    
    public Neighbor[] FindNeighbors(Vector3Int[] relativePos)
    {

        // Returns neighbors from neighrbor list in specified positions
        List<Neighbor> Ns = new List<Neighbor>();
        
        foreach (Neighbor N in neighbors)
        {
            foreach (Vector3Int VI in relativePos)
            {
                if (N.normalizedPos ==VI)
                {
                    Ns.Add(N);
                }
            }
       
        }
        
        return Ns.ToArray();
    }

    public static bool DoesTileExist(Vector3Int pos)
    {
        return (tilesInWorld.Find(x => x.tilePos == (pos)) != null);
    }
    
    public GameObject SpawnTheTile()
    {
        //Spawns the tile in the world
        
        visible = true;
        spawnedObject = Instantiate(sourceObject, pos, rotation);
        spawnedObject.GetComponent<InGameTile>().tile = this;
        return spawnedObject;
    }

    public List<Tile> CreateTile(EnviromentManager envirom, bool pastFirstSpawn)
    {
        // Now that the tile reference has been created, take care of the other stuff, like any secondary tiles around or below, and setting some basic info
        
        List<Tile> secondaryTilesSpawned = new List<Tile>();

        // Take car of pos
        tilePos = new Vector3Int(Mathf.RoundToInt( pos.x), layer, Mathf.RoundToInt( pos.z));

        enviro = envirom;
        
        numberOfEachTile[(int)type]++;

        if (!pastFirstSpawn)
        {
            
            // Spawn tiles below and beside

            if (layer == 0)
            {
                // Ground layer

                visible = true;
                // Spawn the tile in scene
                SpawnTheTile();

                // Will spawn tiles above me later
                if (type == TileType.Grass)
                {
                    // Spawn maybe a rock above, maybe a tree, maybe nothing

                    //Tree 
                    if (numberOfEachTile[(int)TileType.Tree] < maxNumTrees && Random.value < treeChance)
                    {
                        secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.up, TileType.Tree, layer + 1, enviro, out List<Tile> notNeeded, pastFirstSpawn, Quaternion.identity));
                    }

                    // Will spawn dirt below for 3D, maybe an iron

                    // Iron below 
                    if (numberOfEachTile[(int)TileType.Iron] < maxNumIronTiles && Random.value < ironChance)
                    {
                        secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.down, TileType.Iron, layer - 1, enviro, out List<Tile> notNeeded, pastFirstSpawn, Quaternion.identity));
                    }
                    else
                    {
                        // Dirt below 
                        secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.down, TileType.Dirt, layer - 1, enviro, out List<Tile> notNeeded, pastFirstSpawn, Quaternion.identity));
                    }

                }
                else if (type == TileType.Sand)
                {
                    // Spawn sand below
                    if (!DoesTileExist(tilePos + Vector3Int.down))
                    {
                        secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.down, TileType.Sand, layer - 1, enviro, out List<Tile> notNeeded1, pastFirstSpawn, Quaternion.identity));
                    }
                  

                    int STrys = maxMasterNumTrys;
                    MoreSandSpawnAttempt:

                    // Now horz. spawn more sand around
                    if (numberOfEachTile[(int)(TileType.Sand)] < maxNumSandTiles && Random.value < sandChance)
                    {
                        // Create in a random location beside
                        int Trys = maxMinNumTrys;

                        SandSpawnAttempt:
                        Vector3Int Rpos = tilePos + new Vector3Int(Random.Range(-1, 2), 0, Random.Range(-1, 2));

                        if (!DoesTileExist(Rpos))
                        {
                            secondaryTilesSpawned.Add(MasterCreateTile(Rpos, TileType.Sand, layer, enviro, out List<Tile> notNeeded, pastFirstSpawn, Quaternion.identity));
                        }
                        else
                        {
                            Trys--;
                            if (Trys > 0)
                            {
                                goto SandSpawnAttempt;
                            }
                        }

                        STrys--;
                        if (STrys > 0)
                        {
                            goto MoreSandSpawnAttempt;
                        }
                    }
                }
                else if (type == TileType.Water)
                {
                    // Spawn water below
                    secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.down, TileType.Water, layer - 1, enviro, out List<Tile> notNeeded1, pastFirstSpawn, Quaternion.identity));
                }

            }
            else if (layer > 0)
            {
                // Above ground
                visible = true;
                SpawnTheTile();

            }
            else if (layer < 0)
            {
                // Below ground, spawn a rock below me

                // Ignore because not visible
                if (firstGen)
                {
                    visible = false;
                }

                if (layer > -2)
                {

                    if (visible)
                    {
                        SpawnTheTile();
                    }


                    if (type == TileType.Iron)
                    {
                        // Spawn maybe iron beside

                    }

                    //Spawn rock below me

                    secondaryTilesSpawned.Add(MasterCreateTile(pos + Vector3.down, TileType.Rock, layer - 1, enviro, out List<Tile> notNeeded, pastFirstSpawn, Quaternion.identity));

                }
                else
                {
                    // Equal to -2, dont spawn anything anymore

                    if (visible)
                    {
                        SpawnTheTile();
                    }
                }
            }
        }
        else
        {
            visible = true;
            SpawnTheTile();
            GenerateNeighborList(false);
        }

        return secondaryTilesSpawned;
    }

    public List<Tile> SpawnSandTilesAround()
    {
        // Will spawn sand tiles around in at least 1 radius, and then more  randomly less and less

        List<Tile> tilesThatWereSpawned = new List<Tile>();
        if (layer == 0)
        {
            
            // Why does this mess it up
         //   GenerateNeighborList(true);

            Vector3Int[] possibleSandSpawnPos = 
            {
                new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 1), new Vector3Int(1, 0, -1), new Vector3Int(1, 0, 1), new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
            };
            
            
            List<Neighbor> Ns = FindNeighbors(possibleSandSpawnPos).ToList();

            int maxSand = 8 - Ns.Count;
            
            if (maxSand>0)
            {
                // Can spawn at least one sand
                
                foreach (Vector3Int VI in possibleSandSpawnPos)
                {
                    // Sets isempty if you cannot find that tile in the neightbor list, ergo it doesnt exist and so a sand can be placed

                    bool isEmpty = (Ns.Find(x => x.normalizedPos == VI)).normalizedPos == Vector3Int.zero;

                    if (isEmpty && !DoesTileExist(tilePos+VI))
                    {
                        // A secondary check to make sure sand hasnt already been placed
                        // Use tile pos so it looks nicer
                        // Can do a falloff from the nearest grass tile to look more natural
                        List<Tile> secondaryTiles;
                        tilesThatWereSpawned.Add(MasterCreateTile(tilePos + VI,TileType.Sand, layer, enviro, out secondaryTiles,false, Quaternion.identity));
                        foreach (Tile T in secondaryTiles)
                        {
                            tilesThatWereSpawned.Add(T);
                        }
                    }
                }
            }
        }

        return tilesThatWereSpawned;
    }

    public void SpawnWaterTilesAround()
    {
        // Will spawn water tiles around in at least 1 radius, and then more  randomly less and less

        if (layer == 0)
        {
            
            // Why does this mess it up
            //   GenerateNeighborList(true);

            Vector3Int[] possibleWaterSpawnPos = 
            {
                new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 1), new Vector3Int(1, 0, -1), new Vector3Int(1, 0, 1), new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
            };
            
            
            List<Neighbor> Ns = FindNeighbors(possibleWaterSpawnPos).ToList();

            int maxWater = 8 - Ns.Count;
            
            if (maxWater>0)
            {
                // Can spawn at least one water
                
                foreach (Vector3Int VI in possibleWaterSpawnPos)
                {
                    // Sets isempty if you cannot find that tile in the neightbor list, ergo it doesnt exist and so a water can be placed

                    bool isEmpty = (Ns.Find(x => x.normalizedPos == VI)).normalizedPos == Vector3Int.zero;

                    if (isEmpty)
                    {
                        // A secondary check to make sure water hasnt already been placed
                        if (!DoesTileExist(tilePos+VI))
                        {
                            // Use tile pos so it looks nicer
                            MasterCreateTile(tilePos + VI,TileType.Water, layer, enviro, out List<Tile> notNeeded, false, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }

    public static Tile MasterCreateTile(Vector3 posT, TileType TType,int TLayer, EnviromentManager envirom, out List<Tile> secondaryTilesSpawned, bool pastFirstSpawn, Quaternion rotation)
    {
        // master function for creating a new tile. 

        if (tilesInWorld.Find(x => (x.type == TType) && (x.layer == TLayer) && (Math.Abs(x.pos.x - posT.x) < .01f) && (Math.Abs(x.pos.z - posT.z) < .01f)))
        {
            //Debug.Log("Depulicate foudn at:"+ posT.ToString());
            secondaryTilesSpawned = null;
            return null;
        }
        
        Tile T = Instantiate<Tile>(MasterTileList.Where<Tile>(x => (x.type == TType)).ToList()[0]);
        T.rotation = rotation;
        T.pos = posT;
        T.layer = TLayer;
        secondaryTilesSpawned =  T.CreateTile(envirom, pastFirstSpawn);
        tilesInWorld.Add(T);
        return T;

    }
    
    public void JustDugMe()
    {
        //TODO make sure you can only mine with minable item 
        if (!(tags.Contains(TileTag.CanDig) || tags.Contains(TileTag.CanMine)))
        {
            return;
        }

        InventoryItem item;
        int indexofDroppedItem = Random.Range(0, itemsToDrop.Length);

        if (enviro.player.inventory.FindInventoryItemFromName(itemsToDrop[indexofDroppedItem].Name, out item))
        {
            enviro.player.inventory.AddItemToInven(item, Random.Range(numberOfItemsToDrop[ indexofDroppedItem].x,numberOfItemsToDrop[indexofDroppedItem].y) );
        }

        DestroyTile();

    }

    public void RemoveTile()
    {
        DestroyTile();
    }

    public Tile()
    {
        OnCreate();
    }

    public enum TileType
    {
        Nothing, 
        Grass,
        Dirt,
        Sand,
        Water,
        Iron,
        Rock,
        Tree,
        FlatWoodTile,
        WoodPillar,
        WoodFence,
        Bed,
        Ladder,
        Campfire,
        Furnace,
        Dock,
        CraftingBench
    }

    public enum TileTag
    {
        
        Nothing, 
        Walkable, 
        CanDig,
        CanMine,
        CanNotPlace
    }
    
    [System.Serializable]
    public struct Neighbor
    {
        public Vector3Int normalizedPos;
        public Tile theTile;

    }


    public enum TileTemp
    {
        FreezingCold,
        Cold,
        Normal,
        Warm,
        Hot


    }

}


