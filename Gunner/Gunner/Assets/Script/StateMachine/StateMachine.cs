using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
public interface IStatemachine
{
    bool Update();
    string StateName();
    bool IsContinued();
}
public class Statemachine<T> : IStatemachine where T : struct
{

    Dictionary<T, MethodBase> _stateList        = new Dictionary<T, MethodBase>();
    object                    _target;//このクラスを所持するオブジェクト
    T                         _currentState;
    IEnumerator               _currentFunc;
    Stack<IEnumerator>        _nestStack        = new Stack<IEnumerator>();//currentへんこうじに破棄
    bool                      _isNewState       = false;
    bool                      _currentResult    = false;//関数の戻り値 コレクションが正常に進むとtrue 末尾まで行くとfalse
    IEnumerator               CurrentFunc
    { 
        get
        {
            return _currentFunc;
        }
        set
        {
           // Debug.Log("ClearNest Count:" + _nestStack.Count);
            _nestStack.Clear();
            _currentFunc = value;
        }
    }

    public T GetCurrentState() => _currentState;
    public string StateName() =>  _currentState.ToString();
    public bool IsContinued() => _currentResult;
    public void Init(object obj)
    {
        _target = obj;
        // _currentState = first;
        Type type = obj.GetType();
        //_target = c;
        //Debug.Log(type +" , "+ obj);
        foreach (T state in Enum.GetValues(typeof(T)))
        {
            MethodInfo method = type.GetMethod(state.ToString(),
                                               BindingFlags.Public |
                                               BindingFlags.NonPublic |
                                               BindingFlags.Instance |
                                               BindingFlags.Static |
                                               BindingFlags.DeclaredOnly, null, CallingConventions.Any, new Type[0],null);
            
            if (method != null)
            {
               // Debug.Log(method.Name);
                _stateList.Add(state, method);
            }
            
        }
        _isNewState = true;
#if UNITY_EDITOR
        StatemachineInfo.Add(this);
#endif
    }
    public void Add(T state,IEnumerator func){
        if (_stateList.ContainsKey(state) == false)
        {//一つに制限
            //_stateList.Add(state, func);
            _isNewState = true;
        }
    }

    private void SetCurrentFunc(){
        if (_isNewState)
        {
            if (_stateList.ContainsKey(_currentState))
            {
                CurrentFunc = (IEnumerator)_stateList[_currentState].Invoke(_target,null);//Debug.Log(_currentFunc);
            }
            else{
                CurrentFunc = null;
            }
            _isNewState = false;
        }
    }
    /// <summary>
    /// Todo: yield return の戻り値に関数かYieldInstructionが含まれていた場合の処理
    /// </summary>
    /// <returns>The nest.</returns>
    /// <param name="func">Func.</param>
    public bool Nest(IEnumerator func){
        if (func != null)
        {
            //Debug.Log(func.ToString());
            bool result = func.MoveNext();
            if (result)
            {
                _nestStack.Push(func);
                var current = func.Current;
                var instruction = current as CustomYieldInstruction;
                if (instruction != null)
                {
                    _nestStack.Push(WaitCustomYieldInstruction(instruction));
                }else if (current is IEnumerator)
                {
                    var nested = func.Current as IEnumerator;
                    return Nest(nested);
                }else if (current is YieldInstruction)
                {
                    Debug.LogWarning("YieldInstruction not supported");
                }
               
                
            }
            else{
                if (_nestStack.Count > 0)
                {
                    return Nest(_nestStack.Pop());
                }
            }
            _currentResult = result;
            //Debug.Log(func.ToString() + "," + result);
            return result;
        }
        return false;
    }
    private static IEnumerator WaitCustomYieldInstruction(CustomYieldInstruction instruction)
    {
        while (instruction.keepWaiting)
        {
            
            yield return null;
        }
    }
    public bool Update(){

        SetCurrentFunc();

        if (_currentFunc == null)
        {
            return false;
        }
        if (_nestStack.Count == 0)
        {
            return Nest(_currentFunc);
        } else{
            return Nest(_nestStack.Pop());
        }
  
    }
    public void Next(T state,bool _transitionSelf = false){
        //ここで比較したい
            _currentState = state;
            _isNewState = true;
    }
    /*
    /// <summary>
    /// ステートを変更し次フレームへ
    /// </summary>
    /// <returns>The and next.</returns>
    /// <param name="state">State.</param>
    /// <param name="_transitionSelf">If set to <c>true</c> transition self.</param>
    public IEnumerator EndAndNext(T state, bool _transitionSelf = false)
    {
        Next(state, _transitionSelf);
        Debug.Log("ChangeCurrent");
        yield return null;
    }*/
}
