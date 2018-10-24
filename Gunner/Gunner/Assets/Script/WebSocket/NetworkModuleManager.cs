using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class NetworkModuleManager: MonoBehaviourWithStatemachine<NetworkModuleManager.State> {
    public enum State
    {
        None,
        Init,
        Matching,
        Game
    }
    private readonly string DevMatchingServer = "ws://localhost:3000";
    private readonly string StgMatchingServer = "wss://socketmmo.herokuapp.com/";
   
    [SerializeField] private bool _isDevelop;
    [SerializeField] private Text _inputName;
    [SerializeField] private Text _outputText;

    private WebSocketModule _wsModule;
    private WsGameModule _gameModule;
    private WsMatchingModule _matchingModule;


    IEnumerator Init()
    {
        _wsModule = new WebSocketModule();
        _matchingModule = new WsMatchingModule();
        _gameModule = new WsGameModule();
        _matchingModule.SetWsModule(_wsModule);
        _matchingModule.Server = _isDevelop ? DevMatchingServer : StgMatchingServer;
        _gameModule.SetWsModule(_wsModule);
        _gameModule.Server = "";


        Next(State.Matching);
        yield return null;
    }

    IEnumerator Matching()
    {
        while (!_matchingModule.IsClose)
        {
            _matchingModule.Update();
            yield return null;
        }
        _gameModule.Server = _matchingModule.GameServerAddress;
        _gameModule.RoomId = _matchingModule.RoomId;
        _gameModule.Name = _inputName.text;
        if (_matchingModule.IsCancel)
        {
            Next(State.None);
        }
        else
        {
            Next(State.Game);
        }

        DebugText("room " + _gameModule.RoomId);
    }

    IEnumerator Game()
    {
        while (!_gameModule.IsClose)
        {
            _gameModule.Update();
            yield return null;
        }
    }
    public void StartNetworking()
    {
        Next(State.Init);
    }
    public void OnTapEntryName()
    {
        _matchingModule?.ReqEntry(_inputName.text);
    }
    /*
    public void OnTapSendInput(){
        SendInput(
            new SendInput()
            {
                angle = 1350,
                type = "bullet",
                strong = 100,
            }
        );
    }*/
    public void SendInput(SendInput input)
    {
        if (Current == State.Game)
        {
            _gameModule?.ReqInput(input);
        }
    }
    public void OnRecieveMessageGame(UnityAction<string> cb){
        _gameModule?.OnRecieveMessage.AddListener(cb);
    }
    public void OnRecieveMessageMatching(UnityAction<string> cb)
    {
        _matchingModule?.OnRecieveMessage.AddListener(cb);
    }

    void DebugText(string add)
    {
        if (_outputText != null)
        {
            _outputText.text += add;
        }
    }
    void OnApplicationQuit()
    {
        _wsModule?.Close();
    }

}
