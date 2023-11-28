using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int seed;
    public int numberOfChunksPerLine = 5;
    public HeightDataSettings heightData;
    public MeshDataSettings meshData;
    public TerrainObjectData terrainObjectData;
    public Material groundMaterial;

    public AnimationCurve heightCurve;
    private GrassSpawner grassSpawner;

    public GameObject plane;
    public static Transform player;
    public float renderDistance;
    public const float distanceToColliderThreshold = 400f;
    public float grassRenderDistance;
    public float chunkRenderingDistance;

    public bool autoUpdate = true;
    private bool disabledChunks = false;

    private List<GameObject> chunksList = new List<GameObject>();

    public static float FindSqrDistanceBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
    }

    private void OnValidate()
    {
        //SpawnObjects();
    }

    private void Start()
    {
        player = ThirdPersonMovement.instance.transform;
        grassSpawner = GetComponent<GrassSpawner>();

        plane.SetActive(false);
        //grassSpawner.enabled = true;
        
        FillChunkList();
    }

    private void SpawnObjects()
    {
        int chunkSize = heightData.islandSize / numberOfChunksPerLine + 2;
        for (int i = 0; i < numberOfChunksPerLine; i++)
        {
            for (int j = numberOfChunksPerLine - 1; j >= 0; j--)
            {
                Transform chunk = transform.GetChild(i * numberOfChunksPerLine + j);
                TerrainObjectsGenerator.GenerateTerrainObjectsMap(Noise.GenerateNoiseMap(chunkSize, seed + 10, meshData.scale - 10, heightData.octaves, heightData.persistance, heightData.lacunarity, new Vector2(i * chunkSize, j * chunkSize)), seed, chunkSize, terrainObjectData.terrainObjects[0], chunk);
            }
        }
    }

    private void FillChunkList()
    {
        foreach (Transform chunk in transform)
        {
            chunksList.Add(chunk.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        if (FindSqrDistanceBetweenTwoPoints(player.position, plane.transform.position) >= chunkRenderingDistance * chunkRenderingDistance)
        {
            plane.SetActive(true);
            //grassSpawner.enabled = false;
            
            if(!disabledChunks)
            {
                foreach (Transform child in transform) child.gameObject.SetActive(false);
                disabledChunks = true;
            }

            return;
        }
        else
        {
            if(FindSqrDistanceBetweenTwoPoints(player.position, plane.transform.position) < 1000 * 1000)
            {
                plane.SetActive(false);
                //grassSpawner.enabled = true;
            }

            disabledChunks = false;
        }

        for(int i = 0; i < chunksList.Count; i++)
        {
            float sqrDistance = FindSqrDistanceBetweenTwoPoints(player.position, chunksList[i].transform.position);
            if (sqrDistance < renderDistance * renderDistance)
            {
                chunksList[i].SetActive(true);

                if (sqrDistance < distanceToColliderThreshold * distanceToColliderThreshold)
                    chunksList[i].GetComponent<MeshCollider>().enabled = true;
                else
                    chunksList[i].GetComponent<MeshCollider>().enabled = false;
            }
            else
                chunksList[i].SetActive(false);
        }
    }

    private void DrawMesh(MeshData meshData)
    {
        plane.GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh();
    }

    private void GenerateChunks()
    {
        float[,] islandMap = Noise.GenerateNoiseMap(heightData.islandSize + 2, seed, meshData.scale, heightData.octaves, heightData.persistance, heightData.lacunarity, Vector2.zero);
        float[,] decimatedMap = Noise.GenerateNoiseMap(heightData.islandSize, seed, meshData.scale, heightData.octaves, heightData.persistance, heightData.lacunarity, Vector2.zero);
        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(heightData.islandSize);

        int chunkSize = heightData.islandSize / numberOfChunksPerLine + 2;
        int chunkCounter = 0;

        for(int x = 0; x < heightData.islandSize; x++)
        {
            for(int y = 0; y < heightData.islandSize; y++)
            {
                islandMap[x, y] = Mathf.Clamp01(islandMap[x, y] - falloffMap[x, y]);
                decimatedMap[x, y] = Mathf.Clamp01(decimatedMap[x, y] - falloffMap[x, y]);
            }
        }

        for (int i = 0; i < numberOfChunksPerLine; i++)
        {
            for(int j = numberOfChunksPerLine - 1; j >= 0; j--)
            {
                float[,] chunkMap = new float[chunkSize, chunkSize];
                Vector2 chunkPos = new Vector2(i, j);
                TerrainChunk chunk = new TerrainChunk(chunkPos, chunkSize, chunkCounter, meshData.terrainMaterial);

                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        chunkMap[x, y] = islandMap[x + i * (chunkSize - 3), y + (numberOfChunksPerLine - j - 1) * (chunkSize - 3)];
                    }
                }

                chunk.ApplyNoiseMap(chunkMap, transform, meshData.meshHeightMultiplier, heightCurve);
                chunkCounter++;
            }
        }

        transform.localScale = MeshDataSettings.meshScale;
        transform.position = new Vector3(plane.transform.position.x - 1350, 0, plane.transform.position.z - 1215);
        //plane.GetComponent<MeshFilter>().mesh = MeshGenerator.DecimateMesh(MeshGenerator.GenerateTerrainMesh(decimatedMap, meshData.meshHeightMultiplier, heightCurve, 0).CreateMesh());
    }

    public class TerrainChunk
    {
        public GameObject meshObject;
        Vector2 position;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        Material[] materials;

        public TerrainChunk(Vector2 coord, int chunkSize, int number, Material terrainMaterial)
        {
            string name = "Chunk " + number.ToString();
            position = coord * (chunkSize - 3);
            materials = new Material[1] { terrainMaterial };

            meshObject = new GameObject(name);
            meshObject.transform.localPosition = new Vector3(position.x, 0, position.y);
            meshObject.layer = 8;

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.sharedMaterials = materials;
        }

        public void ApplyNoiseMap(float[,] noiseMap, Transform parent, float meshHeightMultiplier, AnimationCurve heightCurve)
        {
            meshFilter.mesh = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, heightCurve, 0).CreateMesh();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshObject.transform.parent = parent;
        }
    }
}