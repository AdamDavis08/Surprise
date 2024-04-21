using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TerrainFiller : MonoBehaviour
{
    public Material terrainMaterial;
    
    public int grassDensity = 10;
    public int patchDetail = 1;

    public float minHeight = 0f;

    [SerializeField] private bool hasChunks = false;
    
    private void OnValidate()
    {
        if (hasChunks)
        {
            Terrain[] terrains = transform.GetComponentsInChildren<Terrain>();

            foreach (Terrain terrain in terrains)
            {
                terrain.materialTemplate = terrainMaterial;
                
                FillGrass(terrain);
            }
        }
        else
        {
            Terrain terrain = GetComponent<Terrain>();  
            
            FillGrass(terrain);
        }
    }

    private void Start()
    {
        Terrain[] terrains = transform.GetComponentsInChildren<Terrain>();
        try
        {
            GetComponent<ChunkManager>().SetTerrainsArray(terrains);
        }
        catch
        {
            Debug.LogError("NO CHUNK MANAGER ON OBJECT!");
        }
        
        Destroy(this);
    }
    
    private void FillTextures(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float height = terrainData.GetInterpolatedHeight((float)y / terrainData.alphamapWidth, (float)x / terrainData.alphamapWidth);
                Vector3 splat = new Vector3(0, 1, 0);
                
                if (height < minHeight) 
                    splat = Vector3.Lerp(splat, new Vector3(1,0,0), 1f); //TODO: Fix blending between textures
                else
                    splat = Vector3.Lerp(splat, new Vector3(0,1,0), 1f);
                
                splat.Normalize();
                splatmapData[x, y, 0] = splat.y;
                splatmapData[x, y, 1] = splat.x;
            }
        }
        
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
    
    private void FillGrass(Terrain terrain)
    {
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
}