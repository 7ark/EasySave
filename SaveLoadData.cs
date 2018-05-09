using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

// Save and Load Data Class
//
// Simple way to save and load any data type - even custom classes
//
// Functions:
//  - Save(string, List<Type>, Optional string): Saves a list of data, the string is the name to save it as (The filetype is the first and last letter of the name)
//    the data coming in must be in the form of a list, at least currently. No need to specific type in the saving, as it can infer it from the list
//  - Load<Type>(string, Optional string): Loads data from a certain file name, returns a list of that data type. You will need to specify the type of the data.
//    There are also three different loading functions for each type. LoadVar (Variables), LoadArray (Arrays), and LoadList (Lists).
//
//  - Optional string: You can choose to enter an optional string for the file which determines the filetype, otherwise it will be the data type
//
// Author: Cory (7ark)
public static class SaveLoadData
{
    public static void Save<T>(string DataFileName, T DataToSave, string OptionalPath = "")
    {
        Type DataType = DataToSave.GetType();

        Directory.CreateDirectory(Application.persistentDataPath + "/Extensions");

        BinaryFormatter BFD = new BinaryFormatter();
        string FilePathData = "/Extensions/" + DataFileName + ".ext";
        FileStream SaveStreamData = File.Create(Application.persistentDataPath + FilePathData);

        string Data = DataType.Name;//.Split('`')[0].Split('[')[1];
        if (Data.Contains("`")) Data = Data.Split('`')[0];
        if (Data.Contains("[]")) { Data = "Array"; }


        if (OptionalPath != "") Data = OptionalPath;

        BFD.Serialize(SaveStreamData, Data);
        SaveStreamData.Close();

        BinaryFormatter BF = new BinaryFormatter();


        string FilePath = "/" + DataFileName + "." + Data;
        if (OptionalPath != "")
            FilePath = "/" + DataFileName + "." + OptionalPath;
        FileStream SaveStream = File.Create(Application.persistentDataPath + FilePath);
        BF.Serialize(SaveStream, DataToSave);
        SaveStream.Close();
    }

    public static T Load<T>(string DataFileName, string OptionalPath = "")
    {
        T Result = default(T);

        string FilePathData = "/Extensions/" + DataFileName + ".ext";

        string FileExt = "";

        if (File.Exists(Application.persistentDataPath + FilePathData))
        {
            BinaryFormatter BFD = new BinaryFormatter();
            FileStream LoadStreamData = File.Open(Application.persistentDataPath + FilePathData, FileMode.Open);
            FileExt = (string)BFD.Deserialize(LoadStreamData);
            LoadStreamData.Close();
        }
        else
            return Result;

        string FilePath = "/" + DataFileName + "." + FileExt;
        if (OptionalPath != "")
            FilePath = "/" + DataFileName + "." + OptionalPath;
        //if (!new FileInfo(FilePath).Exists) Debug.LogError("File does not exist");
        if (File.Exists(Application.persistentDataPath + FilePath))
        {
            BinaryFormatter BF = new BinaryFormatter();
            FileStream LoadStream = File.Open(Application.persistentDataPath + FilePath, FileMode.Open);
            Result = (T)BF.Deserialize(LoadStream);
            LoadStream.Close();
        }
        else
            Debug.Log("Doesn't exist: " + FilePath);
        return Result;
    }

}
