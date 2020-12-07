using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISaveAndLoad : MonoBehaviour
{
    SaveAndLoadMap saveAndLoad = new SaveAndLoadMap();
    TerrainGenerator terrainGenerator;

    private void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    public void OnClickSave()
    {
        saveAndLoad.Save(terrainGenerator);
    }

    public void OnClickLoad()
    {
        saveAndLoad.Load(terrainGenerator);
    }
}
