using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Biome
{
    public static int[,] GenerateBiomeMap(BiomeType[] biomes, float[,] heightMap, float maxHeightOfMap, Vector2 center, float diffuse)
    {
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);
        int[,] biomeMap = new int[mapWidth, mapHeight];
        float maxTemp = 1;
        float minTemp = 0;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float sampleX = (x - (float)mapHeight / 2) /2 + center.x;
                float sampleZ = (y - (float)mapHeight / 2) /2 + center.y;
                float temperature = DetermineTeperature(sampleZ, sampleX, heightMap[x, y], maxHeightOfMap, minTemp, maxTemp, diffuse);
                biomeMap[x, y] = CalculateBiomeMap(biomes, temperature, heightMap[x,y]);
            }
        }

        return biomeMap;
    }
    public static float DetermineTeperature (float xcoordinate, float zcoordinate, float ycoordinate, float maxz, float mintemp, float maxtemp, float diffuse)
    {
        float latitude = Mathf.Abs(zcoordinate / maxz);
        float temperature = Mathf.LerpUnclamped(mintemp, maxtemp, latitude);
        temperature += Mathf.PerlinNoise(xcoordinate, zcoordinate) * diffuse;
        temperature += ycoordinate;

        return temperature;
    }

    public static int CalculateBiomeMap(BiomeType[] biomes, float temperature, float height)
    {
        int returnBiomeIndex = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            if (height >= biomes[i].height)
            {
                returnBiomeIndex = i;
                //terrainMap[x, y] = i;
            }
        }

        if(biomes[returnBiomeIndex].isWaterLayer == true)
        {
            return biomes[returnBiomeIndex].textureIndex;
        } else
        {
            //biome by temperature
            for (int i = 0; i < biomes.Length; i++)
            {
                if (temperature >= biomes[i].temperature)
                {
                    returnBiomeIndex = i;
                    //terrainMap[x, y] = i;
                }
            }
        }

        return biomes[returnBiomeIndex].textureIndex;
    }
}

[System.Serializable]
public struct BiomeType
{
    public string name;
    public int textureIndex;
    [Range(0,1)]
    public float temperature;
    [Range(0, 1)]
    public float height;
    public bool isWaterLayer;
}
