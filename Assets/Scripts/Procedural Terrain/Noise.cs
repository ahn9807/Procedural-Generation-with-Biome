using UnityEngine;
using System.Collections;

public static class Noise
{

    public enum NormalizeMode { Local, Global };
    public const float mapHeightStep = 0.05f;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //시드 값을 반영시켜 시드에 따라서 같은 그러나 다른 시드에 따라 다른 노이즈 맵이 나오도록 한다.
        //또한 오프셋을 수정하여 맵 생성과정에서 스크롤 가능하게 만든다 
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            //예측 보간에서 사용할 최대 높이를 구한다 
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f; //나누기 오류를 수정
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        //스케일 변경시 맵 중앙으로 시야가 이동하도록 한다
        float halfWidht = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                //octaves 얼마나 많은 세부 지형을 만들 것인가
                //persistance 세부지형을 어느 정도 반영 시킬 것인가 (진폭으로 반영되며 1보다 작은 값을 가진다)
                //lacunarity 지형이 분산된 정도 (주기로 반영되며 1보다 큰 값을 가진다)
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidht + octaveOffsets[i].x) / scale * frequency; //scale 이 0 이면 나누기 오류가 발생
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // - 1부터 1까지의 부드러운 난수를 샘플 크기만큼 획득
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;

                }

                //언제나 더 작은 값이 더 작은 지형지물에 반영되도록 한다
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]); //noiseMap 값을 normalized 함
                else
                {
                    float normailzedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f); //최고 높이를 예측하여 통일시킴
                    noiseMap[x, y] = Mathf.Clamp(normailzedHeight, 0, int.MaxValue);
                }
                //noiseMap[x, y] = RoundToStep(noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    public static float RoundToStep(float height)
    {
        return Mathf.Round(height / mapHeightStep) * mapHeightStep;

    }
}
