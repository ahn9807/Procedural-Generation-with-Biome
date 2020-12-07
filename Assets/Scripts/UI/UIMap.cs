using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMap : MonoBehaviour
{
    TerrainGenerator terrainGenerator;
    public StaticObjectGenerator[] staticGenerators;
    public DynamicObjectGenerator[] dynamicObjectGenerators;

    private void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    public void OnClickGenerate()
    {
        terrainGenerator.GenerateTerrain();
        for(int i=0;i<staticGenerators.Length;i++)
        {
            staticGenerators[i].Generate();
        }
        //Dynamic Genertor 
    }
}
