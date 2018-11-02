using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoBehaviourWithStatemachine<T> : MonoBehaviour where T : struct 
{

    protected Statemachine<T> _statemachine;

    protected virtual void Awake()
    {
        _statemachine = new Statemachine<T>();
        _statemachine.Init(this);
        _statemachine.Next((T)Enum.GetValues(typeof(T)).GetValue(0));
    }

    void Update()
    {
        _statemachine.Update();
    }
    protected void Next(T state) => _statemachine.Next(state);
    public T Current => _statemachine.GetCurrentState();
}
/*
public abstract class SingletonMonoBehaviourWithStatemachine<T> : SingletonMonoBehaviour<SingletonMonoBehaviourWithStatemachine<T>> where T : struct
{

    protected Statemachine<T> _statemachine;

    protected virtual void Awake()
    {
        base.Awake();
        _statemachine = new Statemachine<T>();
        _statemachine.Init(this);
    }

    void Update()
    {
        _statemachine.Update();
    }
    protected void Next(T state) => _statemachine.Next(state);
    public T Current => _statemachine.GetCurrentState();

}*/
public abstract class SingletonMonoBehaviourWithStatemachine<T,T2> : MonoBehaviourWithStatemachine<T2> where T2 : struct where T : SingletonMonoBehaviourWithStatemachine<T, T2>
{
    public static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Type t = typeof(T);

                _instance = (T)FindObjectOfType(t);
                if (_instance == null)
                {
                    Debug.LogError(t + "が見つからない");
                }
            }

            return _instance;
        }
    }
    protected virtual void Awake()
    {
        // 他のGameObjectにアタッチされているか調べる.
        // アタッチされている場合は破棄する.
        if (this != Instance)
        {
            Destroy(this);
            //Destroy(this.gameObject);
            Debug.Log(
                typeof(T) + "はすでににアタッチされています。");
            return;
        }

        Debug.Log(typeof(T) + "が" + this.gameObject.name + "にアタッチされました");

        base.Awake();
    }

}