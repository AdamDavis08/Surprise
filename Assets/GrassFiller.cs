using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassFiller : MonoBehaviour
{
    public Terrain terrain;
    public Transform player;

    public int grassDensity = 10;
    public int patchDetail = 1;

    public float minHeight = 0f;

    private void OnValidate()
    {
        int islandSize = terrain.terrainData.heightmapResolution;
        terrain = GetComponent<Terrain>();
        terrain.terrainData.SetDetailResolution(grassDensity, patchDetail);

        int[,] newMap = new int[grassDensity, grassDensity];
        for(int i = 0; i < grassDensity; i++)
        {
            for(int j = 0; j < grassDensity; j++)
            {
                float height = terrain.terrainData.GetInterpolatedHeight((float)j / grassDensity, (float)i / grassDensity);

                if (height < minHeight)
                {
                    newMap[i, j] = 0;
                    continue;
                }

                newMap[i, j] = 1;
            }
        }

        terrain.terrainData.SetDetailLayer(0, 0, 0, newMap);
    }

    private void Update()
    {
        Debug.Log(terrain.SampleHeight(player.position));
    }
}
