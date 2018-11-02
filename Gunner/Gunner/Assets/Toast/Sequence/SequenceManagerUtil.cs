using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class EntireSequenceManagerBase<T> : SingletonMonoBehaviourWithStatemachine<EntireSequenceManagerBase<T>, T> where T : struct
{

    protected void NextScene(T scene){
        Debug.Log("[Sequence]Load Scene Named : " + scene);
        SceneManager.LoadScene(scene.ToString());
    }


    /*
    protected Type GetManager(T state)
    {
        var field = state.GetType().GetField(state.ToString());
        var attribute = Attribute.GetCustomAttribute(field, typeof(StateManager)) as StateManager;

        return attribute?.GetManager();
    }*/
}

public interface ISequenceManager
{
    int CanTransition();
}
/*
public abstract class SequenceManagerBase<T> : SingletonMonoBehaviourWithStatemachine<SequenceManagerBase<T>, T>,ISequenceManager where T :struct
{
    
}*/
/*
/// <summary>
/// 全体シーケンス管理時、各ステートのシーケンスマネージャを指定するためのAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class StateManager : Attribute
{
    private Type _type = null;

    public Type GetManager()
    {
        return _type;
    }
    public StateManager(Type type)
    {
        
        _type = type;
    }
}
    *//*
        if (!type.IsSubclassOf(typeof(ISequenceManager)))
        {
            Debug.LogError("StateManagerアトリビュートにSequenceManagerBaseを継承していないクラスが設定されました。");
            return;
        }*/
