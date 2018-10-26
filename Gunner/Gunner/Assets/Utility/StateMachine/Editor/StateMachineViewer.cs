using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;

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
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(t.ToString(), new GUIStyle() { fontSize = 14},GUILayout.Width(300));
                GUILayout.BeginHorizontal();
                GUILayout.Label(t.StateName(),(GUIStyle)"OL Title", GUILayout.Width(150));
                GUILayout.Label(t.IsContinued() ? "継続" : "終了" , (GUIStyle)"OL Title",GUILayout.Width(50));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                //EditorGUILayout.LabelField(t.ToString(),Color.white, Color.black, 18);
            }
        });
        GUILayout.EndScrollView();

    }

}
