using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class GameNetworkManager : MonoBehaviour {
    public enum State
    {
        Init,
        Connect,
        Entry,
        Wait,
        Cancel,
        Close,
    }
    [SerializeField] private WebSocketManager _wsModule;
    private readonly string DevServer = "ws://localhost:4000"; 
    private readonly string StgServer = "wss://gamemain.herokuapp.com/"; 
    private string _gameId = "";
    public string GameServerAddress { get; set; } = "";
    public string RoomId { get; set; } = "";
    private Statemachine<State> _statemachine;
    void Awake()
    {
        _statemachine = new Statemachine<State>();
        _statemachine.Init(this);
        _statemachine.Next(State.Init);
    }
    void Update()
    {
        _statemachine.Update();
    }
    public void SetGameRoom(string server,string room){
        GameServerAddress = server;
        RoomId = room;
    }
    IEnumerator Init()
    {
        _gameId = "";
        GameServerAddress = "";
        RoomId = "";
        string server = DevServer;
        _wsModule.Setup(server,
        null,
        OnMessage,
        (o, e) => Debug.Log($"WebSocket Close Code: {e.Code} Reason: {e.Reason}"),
        null);
        while (!_wsModule.IsConnect())
        {
            //Debug.Log("[ws]Connecting...");
            // yield return new WaitForSeconds(1f);
            yield return null;
        }
        _statemachine.Next(State.Connect);
        yield return null;
    }
    void OnMessage(object sender, MessageEventArgs e)
    {
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(e.Data);
        switch (obj.type)
        {
            case "connect":
                ResConnect(e.Data);
                break;
            case "ready":
                //ResMatching(e.Data);
                break;
        }
    }
    void ResConnect(string msg)
    {
        MsgRoot<MsgConnect> obj = JsonUtility.FromJson<MsgRoot<MsgConnect>>(msg);
        _gameId = obj.data.id;
        _statemachine.Next(State.Entry);
        Debug.Log("[matching] id :" + _gameId);
    }
}
