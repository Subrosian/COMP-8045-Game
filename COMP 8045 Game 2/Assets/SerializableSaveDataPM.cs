using Markov;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//Credit goes to https://answers.unity.com/questions/8480/how-to-scrip-a-SaveLoadPM-game-option.html for this implementation.
[Serializable()]
public class SerializableSaveDataPM : ISerializable {

    public Dictionary<string, Dictionary<string, int>> AIPM;
    public Dictionary<string, List<KeyValuePair<string, int>>> AIPM_maintainOrder;
    public Dictionary<string, List<List<KeyValuePair<string, int>>>> AIPM_ofmaintainOrderCycles;
    public Dictionary<string, List<List<ShadowPM.Pair<ShadowPM.genBehaviourData, float>>>> AIPMOrdered3_ofFixedLengthCycles;

    public SerializableSaveDataPM() { }

    public SerializableSaveDataPM(SerializationInfo info, StreamingContext ctxt)
    {
        AIPM = (Dictionary<string, Dictionary<string, int>>)info.GetValue("AIPM", typeof(Dictionary<string, Dictionary<string, int>>));
        AIPM_maintainOrder = (Dictionary<string, List<KeyValuePair<string, int>>>)info.GetValue("AIPM_maintainOrder", typeof(Dictionary<string, List<KeyValuePair<string, int>>>));
        AIPM_ofmaintainOrderCycles = (Dictionary<string, List<List<KeyValuePair<string, int>>>>)info.GetValue("AIPM_ofmaintainOrderCycles", typeof(Dictionary<string, List<List<KeyValuePair<string, int>>>>));
        AIPMOrdered3_ofFixedLengthCycles = (Dictionary<string, List<List<ShadowPM.Pair<ShadowPM.genBehaviourData, float>>>>)info.GetValue("AIPMOrdered3_ofFixedLengthCycles", typeof(Dictionary<string, List<List<ShadowPM.Pair<ShadowPM.genBehaviourData, float>>>>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("AIPM", AIPM);
        info.AddValue("AIPM_maintainOrder", AIPM_maintainOrder);
        info.AddValue("AIPM_ofmaintainOrderCycles", AIPM_ofmaintainOrderCycles);
        info.AddValue("AIPMOrdered3_ofFixedLengthCycles", AIPMOrdered3_ofFixedLengthCycles);
    }
}

// === This is the class that will be accessed from scripts ===
public class SaveLoadPM
{

    public static string currentFilePath = Path.Combine(Application.persistentDataPath, "SaveDataPM.cjc");    // Edit this for different save files
    // Call this to write data
    public static void Save()  // Overloaded
    {
        Save(currentFilePath);
    }
    public static void Save(string filePath)
    {   
        SerializableSaveDataPM data = new SerializableSaveDataPM();

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
        SerializableSaveDataPM data = new SerializableSaveDataPM();
        Stream stream = File.Open(filePath, FileMode.Open);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        data = (SerializableSaveDataPM)bformatter.Deserialize(stream);
        stream.Close();

        // Now use "data" to access your Values
    }

    //public static void LoadAndSaveAsUnserializedData(string filePath)
    //{
    //    SerializableSaveDataPM data = new SerializableSaveDataPM();
    //    Stream stream = File.Open(filePath, FileMode.Open);
    //    BinaryFormatter bformatter = new BinaryFormatter();
    //    bformatter.Binder = new VersionDeserializationBinder();
    //    data = (SerializableSaveDataPM)bformatter.Deserialize(stream);
    //    stream.Close();

    //    string unserializedFilePath = Path.Combine(Application.persistentDataPath, "SaveDataPM_unserialized.cjc");

    //    Stream savestream = File.Open(filePath, FileMode.Create);
    //    BinaryFormatter bformatter_save = new BinaryFormatter();
    //    bformatter_save.Binder = new VersionDeserializationBinder();
    //    bformatter_save.Deserialize(savestream);
    //    savestream.Close();
    //}

    public static void LoadUnserializedAndSaveAsSerializedData(string filePath)
    {

    }
}