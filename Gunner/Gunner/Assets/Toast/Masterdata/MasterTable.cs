using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MasterTable<T> where T : class, IMasterRecord
{
    public Master<T> Records { get; private set; } //静的に確保
    public static MasterTable<T> _instance;
    public static MasterTable<T> Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MasterTable<T>();
            }

            return _instance;
        }
    }

    public T Get(int id)
    {
        return Records.Records.FirstOrDefault(_ => _.id == id);
    }
    public void Init(string path)
    {
        string assetPath = Application.streamingAssetsPath + path;
        Debug.Log("[Master] load path : " + assetPath);
        string text = System.IO.File.ReadAllText(assetPath);
        // Debug.Log(text);
        Records = JsonUtility.FromJson<Master<T>>(text);
        //Debug.Log(Records.Records);
    }
}

public interface IMasterRecord
{
    int id { get; set; }

}

[Serializable]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MasterPath : Attribute
{
    public string Path;

    public MasterPath(string path)
    {
        Path = path;
    }
}
[Serializable]
public class Master<T> where T:class, IMasterRecord{
    public T[] Records;
}