using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceableEntity : MonoBehaviour
{

    public GameObject physicalModel;

    public Vector3 worldSpacePos;

    public Vector3Int pos;

    public string[] tags;

    public string extraData;

    public int ID;

    public int uniqueID = 0;
    
    

    public void OnEnable()
    {
        if (uniqueID == 0)
        {
            uniqueID = Random.Range(0, int.MaxValue);
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
}
