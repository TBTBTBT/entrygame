using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebugSequence : SingletonMonoBehaviourWithStatemachine<DebugSequence,DebugSequence.State>
{
    public enum State
    {
        Init,
        Main,
        End
    }
    [SerializeField] private NetworkModuleManager[] _networkModules;
    [SerializeField] private LogicManager _logicManager;
    [SerializeField] private GameViewManager _gameView;
    [SerializeField] private TouchUIManager _touch;
    
    protected void Start()
    {
        _touch.AddCallback(OnPointerDown,TouchMode.PointerDown);
        _touch.AddCallback(OnPointerUp, TouchMode.PointerUp);
    }

    private Vector2 _touchStartPos;
    void OnPointerDown(PointerEventData pe)
    {
        _touchStartPos = pe.position;
    }
    void OnPointerUp(PointerEventData pe)
    {
        SendInput(_touchStartPos,pe.position);
        _touchStartPos = new Vector2(0,0);
    }

    void SendInput(Vector2 start, Vector2 end)
    {
        //Debug.Log($"{start} , {end}");
        int angle = (int)(MathUtil.PointToAngle(start, end)*10);
        int strong = (int)Mathf.Clamp((end - start).magnitude/10,0f,100f);
        //Debug.Log($"{angle} , {strong}");
        _networkModules[0].SendInput(
            new SendInput()
            {
                angle = angle,
                type = "bullet",
                strong = strong,
                frame = _logicManager.Frame
            }
        );
    }
    IEnumerator Init()
    {
        if (_networkModules.Length <= 0)
        {
            yield break;
        }
        foreach (var networkModule in _networkModules)
        {
            networkModule.StartNetworking();
        }
        yield return null;
        while (!_networkModules.Aggregate(true, (current, networkModule) => current & networkModule.Current != NetworkModuleManager.State.Init))
        {
            yield return null;
        }
        _networkModules[0].OnRecieveMessageGame(_logicManager.OnMessageGame);
        _gameView.SetupView(_logicManager);
        Next(State.Main);
    }

}
