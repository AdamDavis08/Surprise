using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public float chunkRate = 0.5f;
    public float chunkRenderDistance = 10f;
    
    private Terrain[] terrains;
    
    private Transform player;
    
    private void Start()
    {
        player = ThirdPersonMovement.instance.transform;
        
        InvokeRepeating("CheckChunks", 0.0f, chunkRate);
    }

    public void SetTerrainsArray(Terrain[] value)
    {
        this.terrains = value;
    }
    
    private void CheckChunks()
    {
        foreach (Terrain terrain in terrains)
        {
            Vector3 chunkPos = terrain.transform.position;
            
            terrain.gameObject.SetActive(MapGenerator.FindSqrDistanceBetweenTwoPoints(chunkPos, player.position) < chunkRenderDistance * chunkRenderDistance);
        }
    }
}
