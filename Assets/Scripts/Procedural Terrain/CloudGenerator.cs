using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class CloudPointData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;
    private bool _isActive;

    public bool IsActive
    {
        get
        {
            return _isActive;
        }
    }

    public int x;
    public int y;
    public float distFromCam;

    public Matrix4x4 Matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

    public CloudPointData(Vector3 pos, Vector3 scale, Quaternion rot, int x, int y, float distFromCam)
    {
        this.pos = pos;
        this.scale = scale;
        this.rot = rot;
        SetActive(true);
        this.x = x;
        this.y = y;
        this.distFromCam = distFromCam;
    }

    public void SetActive(bool desState)
    {
        _isActive = desState;
    }
}

[ExecuteInEditMode]
public class CloudGenerator : MonoBehaviour
{
    public Mesh cloudMesh;
    public Material cloudMat;

    public float cloudPointDistance = 5;
    public float maxScale = 1;
    public float timeScale = 1;
    public float noiseScale = 1;

    public float minNoiseScale = 0.5f;
    public float sizeScale = 0.25f;

    public Camera cam;
    public int maxDist;

    public int batchesToCreate;

    public float cloudAltitude;

    public int chunkStartX = 0;
    public int chunkStartZ = 0;

    Vector3 prevCamPos;
    float noiseOffsetX = 1;
    float noiseOffsetZ = 1;

    List<List<CloudPointData>> chunks = new List<List<CloudPointData>>();
    List<List<CloudPointData>> chunksToUpdateForRender = new List<List<CloudPointData>>();

    private void Start()
    {
        int chunkOffsetX = Mathf.CeilToInt(batchesToCreate / 2f);
        int chunkOffsetZ = chunkOffsetX;

        for(int batchesX = -chunkOffsetX; batchesX<chunkOffsetX; batchesX++)
        {
            for (int batchesY = -chunkOffsetZ; batchesY < chunkOffsetZ; batchesY++)
            {
                BuildCloudChunk(batchesX, batchesY);
            }
        }
    }

    //31 * 31 의 chunk를 설정한다.
    void BuildCloudChunk(int xLoop, int yLoop)
    {
        bool markToRender = false;
        List<CloudPointData> currCloudChunk = new List<CloudPointData>();

        for(int x = 0;x<31;x++)
        {
            for(int y=0;y<32;y++)
            {
                AddCloudPoint(currCloudChunk, x + xLoop * 31, y + yLoop * 31);
            }
        }

        markToRender = CheckForActiveBatch(currCloudChunk);
        chunks.Add(currCloudChunk);

        if (markToRender) chunksToUpdateForRender.Add(currCloudChunk);
    }

    //화면에 나타낼 클라우드를 설정한다. 
    bool CheckForActiveBatch(List<CloudPointData> batch)
    {
        foreach(var cloud in batch)
        {
            cloud.distFromCam = Vector3.Distance(cloud.pos, cam.transform.position);
            if (cloud.distFromCam < maxDist) return true;
        }

        return false;
    }

    void AddCloudPoint(List<CloudPointData> cloudChunkList, int x, int y)
    {
        Vector3 position = new Vector3(chunkStartX + x * cloudPointDistance, cloudAltitude, chunkStartZ + y * cloudPointDistance);

        float distToCam = Vector3.Distance(new Vector3(x, transform.position.y, y), cam.transform.position);

        cloudChunkList.Add(new CloudPointData(position, Vector3.zero, Quaternion.identity, x, y, distToCam));
    }

    private void Update()
    {
        UpdateCloudRender();
        noiseOffsetX += Time.deltaTime * timeScale;
        noiseOffsetZ += Time.deltaTime * timeScale;
    }

    void UpdateCloudRender()
    {
        if(cam.transform.position == prevCamPos)
        {
            UpdateCloudMovement();
        } 
        else
        {
            prevCamPos = cam.transform.position;
            UpdateCloudChunkUpdateList();
            UpdateCloudMovement();
        }
        RenderCloudChunk();
        prevCamPos = cam.transform.position;
    }

    void UpdateCloudMovement()
    {
        foreach(var batch in chunksToUpdateForRender)
        {
            foreach(var cloud in batch)
            {
                float size = Mathf.PerlinNoise(cloud.x * noiseScale + noiseOffsetX, cloud.y * noiseScale + noiseOffsetZ);

                if(size > minNoiseScale)
                {
                    float localScaleX = cloud.scale.x;

                    if(!cloud.IsActive)
                    {
                        cloud.SetActive(true);
                        cloud.scale = Vector3.zero;
                    }
                    if(localScaleX < maxScale)
                    {
                        ScaleCloud(cloud, 1);

                        if(cloud.scale.x > maxScale)
                        {
                            cloud.scale = new Vector3(maxScale, maxScale, maxScale);
                        }
                    }
                } 
                else
                {
                    float localScaleX = cloud.scale.x;
                    ScaleCloud(cloud, -1);

                    if(localScaleX <= 0.1)
                    {
                        cloud.SetActive(false);
                        cloud.scale = Vector3.zero;
                    }
                }
            }
        }
    }

    void ScaleCloud(CloudPointData cloud, int direction)
    {
        cloud.scale += new Vector3(sizeScale * Time.deltaTime * direction, sizeScale * Time.deltaTime * direction, sizeScale * Time.deltaTime * direction);
    }

    //
    void UpdateCloudChunkUpdateList()
    {
        chunksToUpdateForRender.Clear();

        foreach(var batch in chunks)
        {
            if(CheckForActiveBatch(batch))
            {
                chunksToUpdateForRender.Add(batch);
            }
        }
    }

    //GPU를 사용해서 중복된 모양의 MESH를 위치 로테이션 스케일을 다르게 하여 그려준다. 
    //여기서 TRS는 행렬로 주어야한다. 
    void RenderCloudChunk()
    {
        foreach(var batch in chunksToUpdateForRender)
        {
            Graphics.DrawMeshInstanced(cloudMesh, 0, cloudMat, batch.Select((a) => a.Matrix).ToList());
        }
    }
}
