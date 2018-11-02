using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
public class StateMachineViewer: EditorWindow{
    


    [MenuItem("StateMachine/Viewer")]
    static void Open(){
        EditorWindow.GetWindow<StateMachineViewer>("StateMachineViewer");
    }
    void Update()
    {
        Repaint();
    }
    Vector2 _pos;
    void OnGUI()
    {
        // 試しにラベルを表示
        EditorGUILayout.LabelField(EditorSceneManager.GetActiveScene().name + " シーン ステートマシン一覧");
        if (GUILayout.Button("Reflesh")){
            StatemachineInfo.Statemachines.Clear();
        }
        _pos = GUILayout.BeginScrollView(_pos);
        StatemachineInfo.Statemachines.ForEach((w) =>
        {
            IStatemachine t = null;
            w.TryGetTarget(out t);
            if (t != null)
            {
                GUILayout.BeginVertical("box", GUILayout.Height(20), GUILayout.Width(150),GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                string list = Regex.Match(t.ToString(), "\\[([\\s\\S]*)\\]").Value;
                GUILayout.Label(list);//,GUILayout.Height(20));
                GUILayout.BeginHorizontal("box",GUILayout.ExpandHeight(false),GUILayout.ExpandWidth(false));
                GUILayout.Label(t.StateName(), GUILayout.Width(120));
                GUILayout.Label(t.IsContinued() ? "継続" : "終了" , GUILayout.Width(30));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                //EditorGUILayout.LabelField(t.ToString(),Color.white, Color.black, 18);
            }
        });
        GUILayout.EndScrollView();

    }

}

