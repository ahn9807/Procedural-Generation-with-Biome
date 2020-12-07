using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CloudData
{
    public GameObject cloudPrefab;
    [Range(0, 1)] public float spwanRate;
    public float cloudSpeed;
    public float minCloudScale;
    public float maxCloudScale;
    public float minCloudAltitude;
    public float maxCloudAltitude;
}

public class CloudSpawner : MonoBehaviour
{
    [Header("--- Cloud Setting ---")]
    public CloudData[] clouds;

    [Header("--- Chunk Settings ---")]
    public int cloudDistance;
    public int chunkNumber;

    [Header("--- Noise Settings ---")]
    public int seed;
    public float noiseScale;

    Dictionary<Vector3, GameObject> cloudDictionary;

    // Start is called before the first frame update
    void Start()
    {
        cloudDictionary = new Dictionary<Vector3, GameObject>();

        for(int x=-chunkNumber;x<=chunkNumber;x++)
        {
            for (int y = -chunkNumber; y <= chunkNumber; y++)
            {
                for(int z = 0; z < clouds.Length; z++)
                {
                    cloudDictionary.Add(new Vector3(x, y, z), SpwanInitialCloud(x, y, z));
                }
            }
        }
    }

    public GameObject SpwanInitialCloud(int coordX, int coordY, int layerCount)
    {
        //starting point of coord X and coord Y
        float chunkX = coordX * cloudDistance;
        float chunkZ = coordY * cloudDistance;
        float chunkY = Random.Range(clouds[layerCount].minCloudAltitude, clouds[layerCount].maxCloudAltitude);

        chunkX += Random.Range(0f, cloudDistance);
        chunkZ += Random.Range(0f, cloudDistance);

        if(Random.Range(0,1f) < clouds[layerCount].spwanRate)
        {
            GameObject cloud = Instantiate(clouds[layerCount].cloudPrefab);
            cloud.transform.parent = this.transform;
            cloud.name = "cloud";
            cloud.transform.position = new Vector3(chunkX, chunkY, chunkZ);

            return cloud;
        } else
        {
            return null;
        }
    }

    public void UpdateCloudMovementAndScale()
    {
        for (int x = -chunkNumber; x <= chunkNumber; x++)
        {
            for (int y = -chunkNumber; y <= chunkNumber; y++)
            {
                for(int z = 0; z < clouds.Length; z++)
                {
                    var cloud = cloudDictionary[new Vector3(x, y, z)];
                    if(cloud == null)
                    {
                        break;
                    } else
                    {
                        cloud.transform.Translate(Vector3.right * clouds[z].cloudSpeed * Time.deltaTime);
                        cloud.transform.localScale = Vector3.one * (clouds[z].minCloudScale + Mathf.PerlinNoise(seed + cloud.transform.position.x * noiseScale, seed + cloud.transform.position.z * noiseScale) * (clouds[z].maxCloudScale - clouds[z].minCloudScale));

                        if (cloud.transform.position.x > cloudDistance * chunkNumber)
                        {
                            cloud.transform.position = new Vector3(-cloud.transform.position.x, cloud.transform.position.y, cloud.transform.position.z);
                        }
                    }
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCloudMovementAndScale();
    }
}
