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
        this.bounds = new Bounds(position, new Vector3(-1000f, 1000f, 1000f));

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

    private ComputeBuffer voteBuffer, scanBuffer, groupSumArrayBuffer, scannedGroupSumBuffer;

    public bool grassGenerated = false;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private uint[] argsLOD = new uint[5] { 0, 0, 0, 0, 0 };

    [Header("Wind")]
    public float windSpeed = 1.0f;
    public float frequency = 1.0f;
    public float windStrength = 1.0f;

    private int numThreadGroups, numWindThreadGroups, numVoteThreadGroups, numGroupScanThreadGroups;
    private RenderTexture wind;

    private IEnumerator GenerateGrass()
    {
        yield return new WaitForEndOfFrame();

        int numInstancesPerChunk = 10000;
        numThreadGroups = Mathf.CeilToInt(numInstancesPerChunk / 128.0f);
        int batchIndex = 0;
        List<ObjectData> currentBatch = new List<ObjectData>();

        if(numThreadGroups > 128)
        {
            int powerOfTwo = 128;
            while (powerOfTwo < numThreadGroups)
                powerOfTwo *= 2;

            numThreadGroups = powerOfTwo;
        } 
        else
        {
            while (128 % numThreadGroups != 0)
                numThreadGroups++;
        }

        numVoteThreadGroups = Mathf.CeilToInt(numInstancesPerChunk / 128.0f);
        numGroupScanThreadGroups = Mathf.CeilToInt(numInstancesPerChunk / 1024.0f);
        voteBuffer = new ComputeBuffer(numInstancesPerChunk, 4);
        scanBuffer = new ComputeBuffer(numInstancesPerChunk, 4);
        groupSumArrayBuffer = new ComputeBuffer(numThreadGroups, 4);
        scannedGroupSumBuffer = new ComputeBuffer(numThreadGroups, 4);

        wind = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        wind.enableRandomWrite = true;
        wind.Create();
        numWindThreadGroups = Mathf.CeilToInt(wind.height / 8.0f);

        int chunkIndex = 0;
        foreach (Transform child in transform)
        {
            List<List<ObjectData>> chunkBatches = new List<List<ObjectData>>();
            Vector3[] vertices = MeshGenerator.AddVerticesToMesh(child, 10);

            for (int i = 0; i < vertices.Length; i++)
            {
                //if (vertices[i].y * MeshDataSettings.meshScale.y < minHeight) continue;

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

            if (chunkBatches.Count != 0)
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
                argsLOD[3] = (uint)meshLOD.GetBaseVertex(0);

                chunk.argsBuffer.SetData(args);
                chunk.argsBufferLOD.SetData(argsLOD);
                chunk.positionsBuffer.SetData(data);
                chunk.material.SetBuffer("positionBuffer", chunk.culledPositionsBuffer);
                chunk.material.SetTexture("_WindTex", wind);

                chunkIndex++;
            }
            yield return new WaitForEndOfFrame();
        }

        grassGenerated = true;
    }

    private void Start()
    {
        StartCoroutine(GenerateGrass());
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
        if (!grassGenerated) return;

        Matrix4x4 P = Camera.main.projectionMatrix;
        Matrix4x4 V = Camera.main.transform.worldToLocalMatrix;
        Matrix4x4 VP = P * V;
        //GenerateWind();

        foreach (ObjectChunk chunk in grassChunks)
        {
            if (MapGenerator.FindSqrDistanceBetweenTwoPoints(chunk.position, player.position) > distanceToChunkThreshold) continue;

            bool noLOD = MapGenerator.FindSqrDistanceBetweenTwoPoints(chunk.position, player.position) <= distanceToLOD;

            CullGrass(noLOD, chunk, VP);
            if(noLOD)
                Graphics.DrawMeshInstancedIndirect(mesh, 0, chunk.material, chunk.bounds, chunk.argsBuffer);
            else
                Graphics.DrawMeshInstancedIndirect(meshLOD, 0, chunk.material, chunk.bounds, chunk.argsBufferLOD);
        }
    }

    private void CullGrass(bool noLOD, ObjectChunk chunk, Matrix4x4 VP)
    {
        if (noLOD) chunk.argsBuffer.SetData(args);
        else chunk.argsBufferLOD.SetData(argsLOD);

/*        //Vote
        cullGrassShader.SetMatrix("MATRIX_VP", VP);
        cullGrassShader.SetBuffer(0, "_ObjectDataBuffer", chunk.positionsBuffer);
        cullGrassShader.SetVector("_ChunkPos", chunk.position);
        cullGrassShader.SetBuffer(0, "_VoteBuffer", voteBuffer);
        cullGrassShader.SetVector("_CameraPos", Camera.main.transform.position);
        cullGrassShader.SetFloat("_DistanceToGrassThreshold", distanceToGrassThreshold);
        cullGrassShader.Dispatch(0, numVoteThreadGroups, 1, 1);

        //Scan Instances
        cullGrassShader.SetBuffer(1, "_VoteBuffer", voteBuffer);
        cullGrassShader.SetBuffer(1, "_ScanBuffer", scanBuffer);
        cullGrassShader.SetBuffer(1, "_GroupSumArray", groupSumArrayBuffer);
        cullGrassShader.Dispatch(1, numThreadGroups, 1, 1);

        // Scan Groups
        cullGrassShader.SetInt("_NumOfGroups", numThreadGroups);
        cullGrassShader.SetBuffer(2, "_GroupSumArrayIn", groupSumArrayBuffer);
        cullGrassShader.SetBuffer(2, "_GroupSumArrayOut", scannedGroupSumBuffer);
        cullGrassShader.Dispatch(2, numGroupScanThreadGroups, 1, 1);

        // Compact
        cullGrassShader.SetBuffer(3, "_ObjectDataBuffer", chunk.positionsBuffer);
        cullGrassShader.SetBuffer(3, "_VoteBuffer", voteBuffer);
        cullGrassShader.SetBuffer(3, "_ScanBuffer", scanBuffer);
        cullGrassShader.SetBuffer(3, "_ArgsBuffer", noLOD ? chunk.argsBuffer : chunk.argsBufferLOD);
        cullGrassShader.SetBuffer(3, "_CulledObjectDataOutputBuffer", chunk.culledPositionsBuffer);
        cullGrassShader.SetBuffer(3, "_GroupSumArray", scannedGroupSumBuffer);
        cullGrassShader.Dispatch(3, numThreadGroups, 1, 1);*/

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

        voteBuffer.Release();
        scanBuffer.Release();
        groupSumArrayBuffer.Release();
        scannedGroupSumBuffer.Release();
    }

    private void OnApplicationQuit()
    {
        FreeBuffers();
    }

    private List<ObjectData> BuildNewBatch()
    {
        return new List<ObjectData>();
    }
}