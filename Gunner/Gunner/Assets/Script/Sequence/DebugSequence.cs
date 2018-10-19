using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        Next(State.Main);
    }

}
