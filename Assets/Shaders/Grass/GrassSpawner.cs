using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;

public struct ObjectChunk
{
    public Vector3 position;
    public Bounds bounds;
    public List<List<ObjectData>> batches;

    public ComputeBuffer positionsBuffer;
    public ComputeBuffer culledPositionsBuffer;
    public ComputeBuffer argsBuffer;
    public ComputeBuffer argsBufferLOD;
    public Material material;

    public ObjectChunk(Vector3 position, List<List<ObjectData>> batches, Material material)
    {
        this.position = position;
        this.batches = batches;
        this.bounds = new Bounds(position, new Vector3(MeshDataSettings.meshScale.x * 20f, MeshDataSettings.meshScale.y * 100f, MeshDataSettings.meshScale.z * 20f));

        int count = 0;
        foreach (var batch in batches)
        {
            count += batch.Count;
        }

        this.positionsBuffer = new ComputeBuffer(count, sizeof(float) * 3 * 2 + sizeof(float) * 4 + sizeof(float) * 2);
        this.culledPositionsBuffer = new ComputeBuffer(count, sizeof(float) * 3 * 2 + sizeof(float) * 4 + sizeof(float) * 2);
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBufferLOD = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        this.material = material;
    }
}

public struct ObjectData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector2 uv;

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(position, rotation, scale);
        }
    }

    public ObjectData(Vector3 position, Quaternion rotation, Vector3 scale, Vector2 uv)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.uv = uv;
    }
}

public class GrassSpawner : MonoBehaviour
{
    public Mesh mesh;
    public Mesh meshLOD;
    public Material grassMaterial;
    public ComputeShader cullGrassShader;
    public ComputeShader generateWindShader;
    public float offset;
    public int numberOfChunksPerLine = 10;
    public float distanceToGrassThreshold;
    public float distanceToLOD;
    public float distanceToChunkThreshold;
    public float minHeight = 4.5f;
    public Transform player;

    private List<List<ObjectData>> batches = new List<List<ObjectData>>();
    private List<ObjectChunk> grassChunks = new List<ObjectChunk>();

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private uint[] argsLOD = new uint[5] { 0, 0, 0, 0, 0 };

    [Header("Wind")]
    public float windSpeed = 1.0f;
    public float frequency = 1.0f;
    public float windStrength = 1.0f;

    private int numWindThreadGroups;
    private RenderTexture wind;

    private void Start()
    {
        int batchIndex = 0;
        List<ObjectData> currentBatch = new List<ObjectData>();

        wind = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        wind.enableRandomWrite = true;
        wind.Create();
        numWindThreadGroups = Mathf.CeilToInt(wind.height / 8.0f);

        foreach (Transform child in transform)
        {
            List<List<ObjectData>> chunkBatches = new List<List<ObjectData>>();
            Vector3[] vertices = MeshGenerator.AddVerticesToMesh(child, 40);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y * MeshDataSettings.meshScale.y < minHeight) continue;

                Vector2 uv = new Vector2(vertices[i].x, vertices[i].z);
                AddObject(currentBatch, new Vector3(vertices[i].x * MeshDataSettings.meshScale.x, vertices[i].y * MeshDataSettings.meshScale.y, vertices[i].z * MeshDataSettings.meshScale.z), uv);
                batchIndex++;
                if (batchIndex >= 1000)
                {
                    batches.Add(currentBatch);
                    chunkBatches.Add(currentBatch);
                    currentBatch = BuildNewBatch();
                    batchIndex = 0;
                }
            }

            if(chunkBatches.Count != 0)
            {
                List<ObjectData> data = new List<ObjectData>();
                ObjectChunk chunk = new ObjectChunk(child.position, chunkBatches, new Material(grassMaterial));
                grassChunks.Add(chunk);

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
                argsLOD[2] = (uint)meshLOD.GetBaseVertex(0);

                chunk.argsBuffer.SetData(args);
                chunk.argsBufferLOD.SetData(argsLOD);
                chunk.positionsBuffer.SetData(data);
                chunk.material.SetBuffer("positionBuffer", chunk.culledPositionsBuffer);
                chunk.material.SetTexture("_WindTex", wind);
            }
        }
    }

    private void Update()
    {
        RenderBatches();
    }

    private void GenerateWind()
    {
        generateWindShader.SetTexture(0, "_WindMap", wind);
        generateWindShader.SetFloat("_Time", Time.time * windSpeed);
        generateWindShader.SetFloat("_Frequency", frequency);
        generateWindShader.SetFloat("_Amplitude", windStrength);
        generateWindShader.Dispatch(0, numWindThreadGroups, numWindThreadGroups, 1);
    }

    private void RenderBatches()
    {
        GenerateWind();

        foreach (ObjectChunk chunk in grassChunks)
        {
            if (MapGenerator.FindSqrDistanceBetweenTwoPoints(chunk.position, player.position) > distanceToChunkThreshold) continue;

            bool noLOD = MapGenerator.FindSqrDistanceBetweenTwoPoints(chunk.position, player.position) <= distanceToLOD;

            CullGrass(noLOD, chunk);
            if(noLOD)
                Graphics.DrawMeshInstancedIndirect(mesh, 0, chunk.material, chunk.bounds, chunk.argsBuffer);
            else
                Graphics.DrawMeshInstancedIndirect(mesh, 0, chunk.material, chunk.bounds, chunk.argsBufferLOD);
        }
    }

    private void CullGrass(bool noLOD, ObjectChunk chunk)
    {
        cullGrassShader.SetBool("noLOD", noLOD);
        cullGrassShader.SetVector("_ChunkPos", chunk.position);
        cullGrassShader.SetVector("_CameraPos", Camera.main.transform.position);
        cullGrassShader.SetFloat("_DistanceToGrassLODThreshold", distanceToLOD);
        cullGrassShader.SetFloat("_DistanceToGrassThreshold", distanceToGrassThreshold);
        cullGrassShader.SetBuffer(0, "_ObjectDataBuffer", chunk.positionsBuffer);
        cullGrassShader.SetBuffer(0, "_CulledObjectDataBuffer", chunk.culledPositionsBuffer);

        cullGrassShader.Dispatch(0, chunk.positionsBuffer.count / 128, 1, 1);
    }

    private void AddObject(List<ObjectData> currentBatch, Vector3 position, Vector2 uv)
    {
        Vector3 randPos = new Vector3(position.x + Random.Range(-offset, offset), position.y, position.z + Random.Range(-offset, offset));
        Quaternion randRot = new Quaternion(0f, Random.Range(0f, 180f), 0f, 1f);
        currentBatch.Add(new ObjectData(randPos, randRot, Vector3.one, uv));
    }

    private void FreeBuffers()
    {
        foreach(ObjectChunk chunk in grassChunks)
        {
            chunk.argsBuffer.Release();
            chunk.argsBufferLOD.Release();
            chunk.positionsBuffer.Release();
            chunk.culledPositionsBuffer.Release();
        }
    }

    private List<ObjectData> BuildNewBatch()
    {
        return new List<ObjectData>();
    }
}