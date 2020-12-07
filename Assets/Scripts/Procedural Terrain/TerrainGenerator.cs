using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

public class TerrainGenerator : MonoBehaviour
{
    public static Vector2 offetPosition;
    public static int chunkRenderNumber = 4;
    public Material mapMaterial;
    static MapGenerator mapGenerator;
    public const float scale = 2f;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    List<TerrainChunk> terrainChunkList = new List<TerrainChunk>();

    public void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    public void GenerateTerrain()
    {
        if(transform.Find("Terrain Dictionary"))
        {
            Destroy(transform.Find("Terrain Dictionary").gameObject);
        }
        MakeChunks(MapGenerator.mapChunkSize - 1);
    }

    public void MakeChunks(int chunkSize)
    {
        int currentChunkCoordX = Mathf.RoundToInt(offetPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(offetPosition.y / chunkSize);

        GameObject terrainParent = new GameObject("Terrain Dictionary");
        terrainParent.transform.parent = this.transform;

        for (int yOffset = -chunkRenderNumber; yOffset <= chunkRenderNumber; yOffset++)
        {
            for (int xOffset = -chunkRenderNumber; xOffset <= chunkRenderNumber; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                terrainChunkList.Add(new TerrainChunk(viewedChunkCoord, chunkSize, terrainParent.transform, mapMaterial));
            }
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(terrainChunkList.Count);
        for(int i=0;i<terrainChunkList.Count;i++)
        {
            terrainChunkList[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        Destroy(transform.Find("Terrain Dictionary").gameObject);
        GameObject terrainParent = new GameObject("Terrain Dictionary");
        terrainParent.transform.parent = this.transform;

        int chunkSize = reader.ReadInt32();
        for(int i=0;i<chunkSize;i++)
        {
            terrainChunkList.Add(TerrainChunk.Load(reader, terrainParent.transform, mapMaterial));
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        MapData mapData;
        int size;

        public void Save(BinaryWriter writer)
        {
            byte[] buffer = mapData.SerializeMapData();
            writer.Write(buffer.GetLength(0));
            writer.Write(buffer);
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(size);

        }

        public static TerrainChunk Load(BinaryReader reader, Transform parent, Material material)
        {
            TerrainChunk terrain = new TerrainChunk();
            int sizeOfMapdata = reader.ReadInt32();
            terrain.position = new Vector2();
            
            terrain.mapData = MapData.DeSerializeMapData(reader.ReadBytes(sizeOfMapdata));
            terrain.position.x = reader.ReadSingle();
            terrain.position.y = reader.ReadSingle();
            terrain.size = reader.ReadInt32();

            terrain.bounds = new Bounds(terrain.position, Vector2.one * terrain.size);
            Vector3 positionV3 = new Vector3(terrain.position.x, 0, terrain.position.y);

            terrain.meshObject = new GameObject("Terrain Chunk");
            terrain.meshObject.transform.position = positionV3 * scale;
            terrain.meshObject.transform.parent = parent;
            terrain.meshObject.transform.localScale = Vector3.one * scale;
            terrain.meshRenderer = terrain.meshObject.AddComponent<MeshRenderer>();
            terrain.meshFilter = terrain.meshObject.AddComponent<MeshFilter>();
            terrain.meshCollider = terrain.meshObject.AddComponent<MeshCollider>();

            terrain.meshRenderer.material = material;

            terrain.OnMapDataReceived(terrain.mapData);

            return terrain;
        }

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            this.size = size;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        public TerrainChunk()
        {

        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            this.mapData = mapData;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            //meshRenderer.material.mainTexture = texture;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
            meshCollider.sharedMesh = meshFilter.mesh;
        }
    }
}
