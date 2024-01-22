using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderPoints : MonoBehaviour
{
    public float offset = 1f;
    public float dst = 10f;
    public float lod = 10f;
    public Mesh mesh;
    public Mesh meshLOD;
    public Material grassMaterial;
    private List<List<ObjectData>> batches = new List<List<ObjectData>>();

    public ComputeShader cullGrassShader;

    private ObjectChunk chunk;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private uint[] argsLOD = new uint[5] { 0, 0, 0, 0, 0 };

    private void Start()
    {
        List<ObjectData> currentBatch = new List<ObjectData>();
        Vector3[] vertices = null;//MeshGenerator.AddVerticesToMesh(transform.GetChild(0), 10);
        int batchIndex = 0;
        Debug.Log(vertices.Length);

        for(int i = 0; i < vertices.Length; i++)
        {
            Vector2 uv = new Vector2(vertices[i].x, vertices[i].z);
            AddObject(currentBatch, new Vector3(vertices[i].x * 15f, vertices[i].y * 1.5f, vertices[i].z * 15f), uv);
            batchIndex++;

            if(batchIndex >= 1000)
            {
                batches.Add(currentBatch);
                currentBatch = new List<ObjectData>();
                batchIndex = 0;
            }
        }

        List<ObjectData> data = new List<ObjectData>();
        chunk = new ObjectChunk(transform.position, batches, new Material(grassMaterial));

        foreach (var batch in chunk.batches)
        {
            data.AddRange(batch);
        }

        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)data.Count;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        argsLOD[0] = (uint)meshLOD.GetIndexCount(0);
        argsLOD[1] = (uint)data.Count;
        argsLOD[2] = (uint)meshLOD.GetIndexStart(0);
        argsLOD[3] = (uint)meshLOD.GetBaseVertex(0);

        chunk.argsBuffer.SetData(args);
        chunk.argsBufferLOD.SetData(argsLOD);
        chunk.positionsBuffer.SetData(data);
        chunk.material.SetBuffer("positionBuffer", chunk.culledPositionsBuffer);
    }

    private void AddObject(List<ObjectData> currentBatch, Vector3 position, Vector2 uv)
    {
        Vector3 randPos = new Vector3(position.x + Random.Range(-offset, offset), position.y, position.z + Random.Range(-offset, offset));
        Quaternion randRot = new Quaternion(0f, Random.Range(0f, 180f), 0f, 1f);
        currentBatch.Add(new ObjectData(randPos, Quaternion.identity, Vector3.one, uv));
    }

    private void CullGrass(bool noLOD)
    {
        if (noLOD) chunk.argsBuffer.SetData(args);
        else chunk.argsBufferLOD.SetData(argsLOD);

        cullGrassShader.SetVector("_ChunkPos", chunk.position);
        cullGrassShader.SetVector("_CameraPos", Camera.main.transform.position);
        cullGrassShader.SetFloat("_DistanceToGrassThreshold", dst);
        cullGrassShader.SetBuffer(0, "_ObjectDataBuffer", chunk.positionsBuffer);
        cullGrassShader.SetBuffer(0, "_CulledObjectDataBuffer", chunk.culledPositionsBuffer);

        cullGrassShader.Dispatch(0, chunk.positionsBuffer.count / 128, 1, 1);
    }

    private void Update()
    {
        bool noLOD = MapGenerator.FindSqrDistanceBetweenTwoPoints(chunk.position, Camera.main.transform.position) <= lod;

        CullGrass(noLOD);
        if (noLOD)
            Graphics.DrawMeshInstancedIndirect(mesh, 0, chunk.material, chunk.bounds, chunk.argsBuffer);
        else
            Graphics.DrawMeshInstancedIndirect(mesh, 0, chunk.material, chunk.bounds, chunk.argsBufferLOD);
    }
}
