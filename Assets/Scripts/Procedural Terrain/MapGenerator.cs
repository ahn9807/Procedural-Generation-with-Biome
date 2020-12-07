using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, BiomeMap, ColorMap, Mesh, Falloff }
    public DrawMode drawMode;
    public const int mapChunkSize = 95;
    public float noiseScale;
    [Range(0, 6)]
    public int levelOfDetail;
    public Noise.NormalizeMode normalizeMode;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool useFalloff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    float[,] falloffMap;

    //public TerrainType[] regions;
    public BiomeType[] biomes;
    public float biomeDiffuse;
    float maxHeightOfMap = 0;

    private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        maxHeightOfMap = TerrainGenerator.scale * mapChunkSize * TerrainGenerator.chunkRenderNumber;
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, mapData.terrainMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        } else if( drawMode == DrawMode.BiomeMap)
        {
            //display.DrawTexture(TextureGenerator.TextureFromBiomeMap(mapData.terrainMap));
        }
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity,center +  offset, normalizeMode);

        int[,] terrainMap = Biome.GenerateBiomeMap(biomes, noiseMap, maxHeightOfMap, center, biomeDiffuse);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        return new MapData(noiseMap, terrainMap, colorMap);
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        callback(mapData);
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, mapData.terrainMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        callback(meshData);
    } 

    void OnValidate()
    {
        if (octaves < 0)
        {
            octaves = 0;
        }

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colorMap;
    public int[,] terrainMap;

    public MapData(float [,] heightMap, int [,] terrainMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
        this.terrainMap = terrainMap;
    }

    public byte[] SerializeMapData()
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write(heightMap.GetLength(0));
        writer.Write(heightMap.GetLength(1));

        for(int x = 0; x < heightMap.GetLength(0); x++)
        {
            for(int y = 0; y < heightMap.GetLength(1);y++)
            {
                writer.Write(heightMap[x, y]);
            }
        }

        writer.Write(colorMap.GetLength(0));

        for(int i=0;i<colorMap.GetLength(0);i++)
        {
            writer.Write(colorMap[i].r);
            writer.Write(colorMap[i].g);
            writer.Write(colorMap[i].b);
        }

        writer.Close();
        stream.Close();

        return stream.ToArray();
    }

    public static MapData DeSerializeMapData(byte[] bytes)
    {
        var reader = new BinaryReader(new MemoryStream(bytes));

        var s = default(MapData);

        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

        s.heightMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                s.heightMap[x, y] = reader.ReadSingle();
            }
        }

        int colorMapSize = reader.ReadInt32();

        s.colorMap = new Color[colorMapSize];

        for (int i = 0; i< colorMapSize;i++)
        {
            s.colorMap[i].r = reader.ReadSingle();
            s.colorMap[i].g = reader.ReadSingle();
            s.colorMap[i].b = reader.ReadSingle();
        }

        reader.Close();

        return s;
    }
}