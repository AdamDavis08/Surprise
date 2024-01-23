using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainObjectsGenerator
{
    public static void GenerateTerrainObjectsMap(float[,] objectMap, int seed, int mapSize, TerrainObject settings, Transform parent)
    {
        System.Random prng = new System.Random(seed.GetHashCode());
        System.Random object_prng = new System.Random(seed.GetHashCode());

        for (int x = 0; x < mapSize; x += settings.spaceIncrement)
        {
            for (int y = 0; y < mapSize; y += settings.spaceIncrement)
            {
                if (objectMap[x, y] < settings.minObjectHeight)
                {
                    Transform terrainObject = GameObject.Instantiate(settings.prefabs[object_prng.Next(0, settings.prefabs.Length)]).transform;
                    terrainObject.parent = parent;
                    terrainObject.localPosition = new Vector3(x + (float)(prng.Next(0, 20) / 10f), 50f, y + (float)(prng.Next(0, 20) / 10f));

                    Ray ray = new Ray(terrainObject.position, -Vector3.up);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        terrainObject.position = new Vector3(hitInfo.point.x, hitInfo.point.y - 1f, hitInfo.point.z);
                    }

#if UNITY_EDITOR
                    if (terrainObject.position.y < 4.5f)
                    {
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            GameObject.DestroyImmediate(terrainObject.gameObject);
                        };
                        continue;
                    }
#endif
                }
            }
        }
    }
}

[System.Serializable]
public class TerrainObject
{
    public string name;
    public GameObject[] prefabs;
    public int spaceIncrement = 2;
    public float minObjectHeight = 0.5f;
}
