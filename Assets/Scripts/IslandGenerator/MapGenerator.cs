using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int seed;
    public HeightDataSettings heightData;
    public MeshDataSettings meshData;
    public Material groundMaterial;

    public AnimationCurve heightCurve;

    public GameObject plane;

    public Terrain terrain;
    private TerrainData terrainData;

    public static Transform player;
    public const float distanceToColliderThreshold = 400f;

    public bool autoUpdate = true;

    public static float FindSqrDistanceBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
    }

    private void Start()
    {
        player = ThirdPersonMovement.instance.transform;
        terrainData = terrain.terrainData;

        plane.SetActive(false);
    }

    private void OnValidate()
    {
        terrainData = terrain.terrainData;

        GenerateTerrain();
    }

    private void SpawnObjects()
    {
        int islandSize = terrainData.heightmapResolution;
        float[,] objectMap = Noise.GenerateNoiseMap(islandSize, seed + 10, meshData.scale - 10, heightData.octaves, heightData.persistance, heightData.lacunarity, Vector2.zero);
        float[,] forestBiomeMap = Noise.GenerateNoiseMap(islandSize, seed + 15, meshData.scale - 10, heightData.octaves, heightData.persistance, heightData.lacunarity, Vector2.zero);
        System.Random prng = new System.Random(seed.GetHashCode());

        terrainData.treeInstances = new TreeInstance[0];
        for (int x = 0; x < islandSize; x+=3)
        {
            for(int y = 0; y < islandSize; y+=3)
            {
                Vector2 position = new Vector3((float)(x + prng.Next(0, 20) / 10f) / islandSize, (float)(y + prng.Next(0, 20) / 10f) / islandSize);
                if (terrainData.GetInterpolatedHeight(position.x, position.y) < 30f) continue;

                if (objectMap[x, y] < 0.5f)
                {
                    int typeOfTree = (forestBiomeMap[x, y] >= 0.25f) ? 0 : (forestBiomeMap[x, y] >= 0.125f) ? 1 : 2;

                    TreeInstance treeInstance = new TreeInstance();
                    treeInstance.prototypeIndex = typeOfTree;
                    treeInstance.position = new Vector3(position.x, 0, position.y);
                    treeInstance.rotation = Random.Range(0, 360f) * Mathf.Deg2Rad;
                    treeInstance.widthScale = 1f;
                    treeInstance.heightScale = 1f;
                    treeInstance.color = Color.white;
                    treeInstance.lightmapColor = Color.white;
                    terrain.AddTreeInstance(treeInstance);
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            terrain.Flush();
        };
#endif
    }

    private void GenerateTerrain()
    {
        int islandSize = terrainData.heightmapResolution;

        float[,] islandMap = Noise.GenerateNoiseMap(islandSize, seed, meshData.scale, heightData.octaves, heightData.persistance, heightData.lacunarity, Vector2.zero);
        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(islandSize);

        for (int x = 0; x < islandSize; x++)
        {
            for (int y = 0; y < islandSize; y++)
            {
                islandMap[x, y] = Mathf.Clamp01(islandMap[x, y] - falloffMap[x, y]) / 7.5f;
                //islandMap[x, y] = heightCurve.Evaluate(islandMap[x, y]);
            }
        }

        terrainData.SetHeights(0, 0, islandMap);
    }

    private void DrawMesh(MeshData meshData)
    {
        plane.GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh();
    }
}