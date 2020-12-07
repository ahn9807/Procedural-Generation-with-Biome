using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectGenerator : MonoBehaviour
{
    const string terrainName = "Terrain Chunk"; //터렌인 청크를 확인하는데 사용한다 
    public float maxDistance = 1000f; //얼만큼 깊이로 RayCast 를 할건지 정한다 

    [System.NonSerialized]
    public static bool genreatingTerrainObjectCompleted;

    public int totalNumber;

    public DynamicTerrainObjectType terrainObjectType;

    //밀도 관련 변수들
    float totalDensity;

    //청크 관련 변수
    int initialChunkCoordX = 0;
    int initialChunkCoordY = 0;
    int chunkSize = 95;
    int chunkNumber = 1;
    int terrainXSize;
    int terrainZSize;
    float scale;
    int totalMapSize;
    int populationPerChunk;

    LinkedList<GameObject> dynamicObjectList = new LinkedList<GameObject>();

    public void Start()
    {
        chunkNumber = TerrainGenerator.chunkRenderNumber;
        chunkSize = MapGenerator.mapChunkSize;
        scale = TerrainGenerator.scale;
        terrainXSize = (chunkNumber * 2 + 1) * chunkSize;
        terrainZSize = (chunkNumber * 2 + 1) * chunkSize;
        totalMapSize = terrainXSize * terrainZSize * 4;
        populationPerChunk = Mathf.RoundToInt(terrainObjectType.spwanNumber / (float)((chunkNumber * 2 + 1) * (chunkNumber * 2 + 1)));

        StartCoroutine(tempCorutine());
    }

    void OnValidate()
    {
        if (terrainObjectType.startPosition > terrainObjectType.endPosition)
            terrainObjectType.endPosition = 1;
    }


    IEnumerator tempCorutine()
    {
        yield return new WaitForSeconds(3);
        GenerateInitialDynamicObject();
    }

    IEnumerator SpwanObjectCorutine()
    {
        while (true)
        {
            if (dynamicObjectList.Count < terrainObjectType.spwanNumber)
            {
                for (int i = 0; i < terrainObjectType.spawnIntervalNumber; i++)
                {
                    int yOffset = Random.Range(-chunkNumber, chunkNumber);
                    int xOffset = Random.Range(-chunkNumber, chunkNumber);

                    GenerateTerrainObjectAtChunk(new Vector2(xOffset, yOffset), 1);
                }
            }
            totalNumber = dynamicObjectList.Count;
            yield return new WaitForSecondsRealtime(terrainObjectType.spwanIntervalTime);
        }
    }

    void GenerateInitialDynamicObject()
    {
        initialChunkCoordX = 0;
        initialChunkCoordY = 0;

        for (int yOffset = -chunkNumber; yOffset <= chunkNumber; yOffset++)
        {
            for (int xOffset = -chunkNumber; xOffset <= chunkNumber; xOffset++)
            {
                Vector2 chunkCoord = new Vector2(initialChunkCoordX + xOffset, initialChunkCoordY + yOffset);
                GenerateTerrainObjectAtChunk(chunkCoord, populationPerChunk);
            }
        }

        genreatingTerrainObjectCompleted = true;
        StartCoroutine(SpwanObjectCorutine());
    }

    public bool RemoveObject(GameObject gameObject)
    {
        try
        {
            dynamicObjectList.Remove(gameObject);
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }

        return true;
    }


    void GenerateTerrainObjectAtChunk(Vector2 coord, int size)
    {
        Vector2 position = coord * chunkSize;

        int yBound = Mathf.RoundToInt((position.x - chunkSize / 2f) * scale);
        int xBound = Mathf.RoundToInt((position.y - chunkSize / 2f) * scale);


        for (int i = 0; i < size; i++)
        {
            float coordX = Random.Range((float)xBound, (float)(xBound + chunkSize * scale));
            float coordZ = Random.Range((float)yBound, (float)(yBound + chunkSize * scale));
            //오브젝트가 스폰됨    
            GameObject generatedObject = SpwanTerrainObject(new Vector2(coordX, coordZ), terrainObjectType);
            if (generatedObject != null)
                dynamicObjectList.AddLast(generatedObject);
        }
    }

    public GameObject SpwanTerrainObject(Vector2 position, DynamicTerrainObjectType terrainObjectType)
    {
        //스폰되면 게임 오브젝트를 아니면 null 을 반환한다 
        RaycastHit raycastHit;
        Vector3 origin = new Vector3(position.x, maxDistance / 2, position.y);

        Vector3 spwanPosition;
        Vector3 spwanNormal;
        Quaternion spwanRotation;

        bool isHit = Physics.Raycast(origin, Vector3.down, out raycastHit, maxDistance);

        if (isHit == true && raycastHit.collider.name.Contains(terrainName))
        {
            spwanPosition = raycastHit.point;
            //만약 위치가 생성하고자 하는 위치가 아니면 건너 뛴다 
            if (spwanPosition.y < terrainObjectType.startPosition || spwanPosition.y > terrainObjectType.endPosition)
            {
                return null;
            }
            spwanNormal = raycastHit.normal;

            if (!terrainObjectType.isErect)
                spwanRotation = Quaternion.FromToRotation(Vector3.up, 360 * spwanNormal) * terrainObjectType.prefab.transform.rotation;
            else
                spwanRotation = terrainObjectType.prefab.transform.rotation;
        }
        else
        {
            return null;
        }

        MeshCollider object_meshCollider;
        Rigidbody object_rigidbody;

        GameObject rock = Instantiate(terrainObjectType.prefab);
        //Initializing spwaned object
        if (terrainObjectType.insertDepth <= 0.001)
            rock.transform.position = new Vector3(spwanPosition.x, spwanPosition.y - terrainObjectType.insertDepth * scale, spwanPosition.z);
        else
            rock.transform.position = new Vector3(spwanPosition.x, spwanPosition.y, spwanPosition.z);
        rock.transform.rotation = spwanRotation;
        rock.transform.parent = this.transform;
        if (terrainObjectType.sizeMultiplier > 0.001)
            rock.transform.localScale *= terrainObjectType.sizeMultiplier;
        else
            rock.transform.localScale *= scale;

        if (terrainObjectType.useRigidbody)
        {
            object_rigidbody = rock.AddComponent<Rigidbody>();
            object_meshCollider = rock.AddComponent<MeshCollider>();
            object_meshCollider.convex = true;
            if (terrainObjectType.isKnematic)
            {
                object_rigidbody.isKinematic = true;
            }
        }
        else if (terrainObjectType.useCollider)
        {
            object_meshCollider = rock.AddComponent<MeshCollider>();
        }

        return rock;
    }
}

[System.Serializable]
public class DynamicTerrainObjectType
{
    public bool useRigidbody;
    public bool useCollider;
    public bool isKnematic;
    public bool isErect;

    public float height;
    public GameObject prefab;

    public int spwanNumber;
    public float sizeMultiplier; //0 means no change to object size
    public float insertDepth;
    public float spwanIntervalTime;
    public int spawnIntervalNumber;

    public float startPosition;
    public float endPosition;
}
