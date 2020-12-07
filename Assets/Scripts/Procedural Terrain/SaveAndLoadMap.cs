using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveAndLoadMap
{
    public void Save(TerrainGenerator terrainGenerator)
    {
        Debug.Log(Application.persistentDataPath);
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        Stream fileStream = File.Open(path, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fileStream);

        terrainGenerator.Save(writer);

        writer.Close();
        fileStream.Close();
    }

    public void Load(TerrainGenerator terrainGenerator)
    {
        Debug.Log(Application.persistentDataPath);
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        Stream fileStream = File.OpenRead(path);
        BinaryReader reader = new BinaryReader(fileStream);

        terrainGenerator.Load(reader);

        reader.Close();
        fileStream.Close();
    }


}
