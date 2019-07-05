using Markov;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//Credit goes to https://answers.unity.com/questions/8480/how-to-scrip-a-saveload-game-option.html for this implementation.
[Serializable()]
public class SerializableSaveData : ISerializable {

    public MarkovChain<ShadowAIMarkov.MarkovUnit> AIMarkovChain;

    public SerializableSaveData() { }

    public SerializableSaveData(SerializationInfo info, StreamingContext ctxt)
    {
        AIMarkovChain = (MarkovChain<ShadowAIMarkov.MarkovUnit>)info.GetValue("AIMarkovChain", typeof(MarkovChain<ShadowAIMarkov.MarkovUnit>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("AIMarkovChain", AIMarkovChain);
    }
}

public sealed class VersionDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            Type typeToDeserialize = null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;
            // The following line of code returns the type. 
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
            return typeToDeserialize;
        }
        return null;
    }
}
// === This is the class that will be accessed from scripts ===
public class SaveLoad
{

    public static string currentFilePath = "SaveData.cjc";    // Edit this for different save files

    // Call this to write data
    public static void Save()  // Overloaded
    {
        Save(currentFilePath);
    }
    public static void Save(string filePath)
    {
        SerializableSaveData data = new SerializableSaveData();

        Stream stream = File.Open(filePath, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, data);
        stream.Close();
    }

    // Call this to load from a file into "data"
    public static void Load() { Load(currentFilePath); }   // Overloaded
    public static void Load(string filePath)
    {
        SerializableSaveData data = new SerializableSaveData();
        Stream stream = File.Open(filePath, FileMode.Open);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        data = (SerializableSaveData)bformatter.Deserialize(stream);
        stream.Close();

        // Now use "data" to access your Values
    }

}