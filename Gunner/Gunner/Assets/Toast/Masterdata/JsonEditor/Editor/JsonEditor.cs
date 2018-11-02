using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class JsonEditor : EditorWindow{
  

    private string _filePath = "ここに.jsonファイルをドラッグ&ドロップ";
    private string _jsonData;
    private Dictionary<string, List<string>> _data = new Dictionary<string,List<string>>();
    private Dictionary<string, int> _viewconf = new Dictionary<string,int>();//枠サイズ調整
    private IEnumerator _coroutine;
    [MenuItem("MasterData/JSONEditor")]
    private static void CreateWindow()
    {
        var window = GetWindow(typeof(JsonEditor)) as JsonEditor;
        window.autoRepaintOnSceneChange = true;
    }
    private List<Object> CreateDragAndDropGUI(Rect rect)
    {
        List<Object> list = new List<Object>();

        //D&D出来る場所を描画
        GUI.Box(rect, "");

        //マウスの位置がD&Dの範囲になければスルー
        if (!rect.Contains(Event.current.mousePosition))
        {
            return list;
        }

        //現在のイベントを取得
        EventType eventType = Event.current.type;

        //ドラッグ＆ドロップで操作が 更新されたとき or 実行したとき
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            //カーソルに+のアイコンを表示
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            //ドロップされたオブジェクトをリストに登録
            if (eventType == EventType.DragPerform)
            {
                list = new List<Object>(DragAndDrop.objectReferences);

                //ドラッグを受け付ける(ドラッグしてカーソルにくっ付いてたオブジェクトが戻らなくなる)
                DragAndDrop.AcceptDrag();
            }

            //イベントを使用済みにする
            Event.current.Use();
        }

        return list;
    }

    private bool IsJSONFile(string path)
    {
        if (Regex.Match(path, "\\.json$").Success)
        {
            return true;
        }
        return false;
    }

    private bool SetFilePath(string path)
    {
        if (_filePath == path)
        {
            return false;
        }

        _filePath = path;
        return true;
    }

    private void LoadJSON(string path)
    {
        _data.Clear();
        _viewconf.Clear();
        Debug.Log("LoadJson");
        _jsonData = File.ReadAllText(path);
        StartCoroutine(ParseJSON());
    }

    IEnumerator ParseJSON()
    {
        string list = Regex.Match(_jsonData, "\\[([\\s\\S]*)\\]").Value;
        Debug.Log(list);
        string[] comma = list.Split(',');
        foreach (var s in comma)
        {
            yield return null;
            string[] colon = s.Split(':');
            if (colon.Length < 2)
            {
                continue;
            }

            string key = Regex.Match(colon[0], "\"(((\\\\\")|[^\"])*)\"").Value.Replace("\"", "");
            if (!_data.ContainsKey(key))
            {
                _data.Add(key, new List<string>());
            }

            string value = Regex.Match(colon[1], "^.*").Value;
            _data[key].Add(value);
        }

        SetViewConf();
    }

    void SetViewConf()
    {
        foreach (var d in _data)
        {
            int max = 10;
            max = Math.Max(d.Key.Length * 8, max);
            foreach (var s in d.Value)
            {
                max = Math.Max(s.Length * 10, max);
            }

            max = Math.Min(max, 150);
            _viewconf.Add(d.Key,max);
            
        }
    }

    private void StartCoroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine;
    }
    private void UpdateCoroutine()
    {
        if (_coroutine == null)
        {
            return;
        }
        while (_coroutine.MoveNext())
        {
        }
    }

    private void AddData()
    {
        foreach (var keyValuePair in _data)
        {
            string data = "";
            if (keyValuePair.Key == "id")
            {
                data = _data["id"].Count.ToString();
            }
            keyValuePair.Value.Add(data);
            
        }
    }

    private void AddColom(string col)
    {
        if (col == "")
        {
            return;
        }
        int max = 0;
        if (_data.Count > 0)
        {
            max = _data.First().Value.Count;
        }
        _data.Add(col,new List<string>(new string[max]));
    }
    private void Save()
    {
        string p = Regex.Match(_filePath, "/(.*)?/").Value;
        Debug.Log(Application.dataPath +  p);
        string[] getPath = Regex.Split(_filePath, ".*/");
        
        var path = EditorUtility.SaveFilePanel("", Application.dataPath +  p, getPath[1], "json");
        string save = DictionalyToJson(_data);
        if (save != "" && path != "")
        {
            File.WriteAllText(path,save);
            Debug.Log("save to "+path);
        }
    }

    private string DictionalyToJson(Dictionary<string,List<string>> dic)
    {
        int max = -1;
        foreach (var d in dic)
        {
            if (max == -1 || max == d.Value.Count)
            {
                max = d.Value.Count;
            }
            else
            {
                max = -2;
                break;
            }
        }

        if (max == -2 || max == -1)
        {
            Debug.LogError("長さが均一ではない");
            return "";
        }
        string ret = "";
        ret += "{\"Records\":[\n";
        bool isFirst1 = true;
        for (int i = 0 ; i < max ;i ++)
        {
            if (!isFirst1)
            {
                ret += ",";
                ret += "\n";
            }
            isFirst1 = false;
            ret += "{\n";
            bool isFirst2 = true;
            foreach (var d in dic)
            {

                if (!isFirst2)
                {
                    ret += ",";
                    ret += "\n";
                }
                isFirst2 = false;
                ret += $"\"{d.Key}\":{d.Value[i]}";

            }
            ret += "\n";
            ret += "}";

        }
        ret += "\n";
        ret += "]}";
        return ret;
    }
    private Vector2 scroll;
    private int page = 0;
    private int numPerPage = 50;
    private string columName = "";
    private void OnGUI()
    {
        GUILayout.Label("※多階層には対応していません。");


        //GUILayout.Label(size.ToString());
        List<Object> dropObjectList = CreateDragAndDropGUI(new Rect(Vector2.zero,position.size));
        bool isNew = false;
        if (dropObjectList.Count > 0)
        {
            if (IsJSONFile(AssetDatabase.GetAssetPath(dropObjectList[0])))
            {
                isNew = SetFilePath(AssetDatabase.GetAssetPath(dropObjectList[0]));
     
            }

        }
        if (isNew)
        {
            LoadJSON(_filePath);
        }
        GUILayout.Label(_filePath);
        //EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存", GUILayout.Width(60)))
        {
            Save();
        }
        if (GUILayout.Button("リセット", GUILayout.Width(60)))
        {

        }
        //EditorGUILayout.EndHorizontal();
        int count = 0;
       
        scroll = EditorGUILayout.BeginScrollView(scroll);
        GUILayout.Label("PAGE " + page,"box");
        EditorGUILayout.BeginHorizontal("box");
        
        foreach (var pair in _data)
        {

            int max = Math.Min(pair.Value.Count, numPerPage * (page + 1));
            int width = 30;
            if (_viewconf.ContainsKey(pair.Key))
            {
                width = _viewconf[pair.Key];
            }
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(pair.Key, GUILayout.Width(width));
            if (pair.Key == "id")
            {
               
                
                for (int i = page * numPerPage; i < max; i++)
                {
                    EditorGUILayout.LabelField(pair.Value[i], GUILayout.Width(width));
                }
                EditorGUILayout.EndVertical();
                continue;
            }
            for (int i = page * numPerPage; i < max; i++)
            {
                pair.Value[i] = EditorGUILayout.TextField( pair.Value[i], GUILayout.Width(width));
            }
            EditorGUILayout.EndVertical();
            count++;
        }
        EditorGUILayout.BeginVertical("box");
        columName = EditorGUILayout.TextField(columName, GUILayout.Width(100));
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            AddColom(columName);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            AddData();
        };
    
        if (page > 0)
        {
            if (GUILayout.Button("<<", GUILayout.Width(30)))
            {
                page--;
            }
        }
        //if (_data.Count > (page) * 10)
        {
            if (GUILayout.Button(">>", GUILayout.Width(30)))
            {
                page++;
            }

        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        
        UpdateCoroutine();
    }
}
