using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using System;

public class SaveNLoadData : MonoBehaviour
{
    /// <typeparam name="T"></typeparam>
    /// <param name="data">Serializable data</param>
    /// <param name="path">Full path including file extension</param>
    public static void SaveData<T>(T data, string path)
    {
        if (!typeof(T).IsSerializable)
        {
            Debug.Log("The Data is not serializable");
            return;
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
        {
            binaryFormatter.Serialize(fileStream, data);
        }
    }

    /// <summary>
    /// Load data at path
    /// </summary>
    public static T LoadData<T>(string path)
    {
        if (!File.Exists(path))
            return (T)Activator.CreateInstance(typeof(T));

        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fileStream = File.Open(path, FileMode.Open))
        {
            return (T)binaryFormatter.Deserialize(fileStream);
        }
    }

    public static void EraseData(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}