using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineMeshes : MonoBehaviour
{

    public void Combine()
    {
       /*
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject.GetComponent<InGameTile>())
            {
                if (meshFilters[i].gameObject.GetComponent<InGameTile>().tile.type == Tile.TileType.Water)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                    meshFilters[i].gameObject.SetActive(false);
                }
            }
            i++;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.CombineMeshes(combine, true, true);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        */
    }


}
