using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGrassPositions(GrassData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = "D:/Hristijan Gajdov/Projects/Unity/Surprise/Assets/Scripts/test.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static GrassData LoadGrassPositions()
    {
        string path = "D:/Hristijan Gajdov/Projects/Unity/Surprise/Assets/Scripts/test.fun";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GrassData data = formatter.Deserialize(stream) as GrassData;
            stream.Close();

            return data;
        } 
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
