using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshDataSettings : ScriptableObject
{
    public float scale = 1f;
    public static Vector3 meshScale = new Vector3(15f, 1.5f, 15f);

    public float meshHeightMultiplier;

    public Material terrainMaterial;
    public Material grassMaterial;
}
