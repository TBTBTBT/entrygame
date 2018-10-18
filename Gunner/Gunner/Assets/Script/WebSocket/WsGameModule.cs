using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebSocketSharp;

public class WsGameModule {
    public enum State
    {
        Init,
        Connect,
        Entry,
        Wait,
        Cancel,
        Close,
        End
    }

    private WebSocketModule _wsModule;
    private string _gameId = "";
    public string Server { get; set; } = "";
    public string Name { get; set; } = "";
    public string RoomId { get; set; } = "";
    private Statemachine<State> _statemachine;
    public State Current => _statemachine.GetCurrentState();
    public bool IsClose => _statemachine.GetCurrentState() == State.End;
    public void SetWsModule(WebSocketModule ws) => _wsModule = ws;
    public UnityEvent<string> OnRecieveMessage = new MessageEvent();
    public void InitState()
    {
        _statemachine = new Statemachine<State>();
        _statemachine.Init(this);
        _statemachine.Next(State.Init);
    }
    public WsGameModule()
    {
        InitState();
    }
    public void Update()
    {
        _statemachine.Update();
    }
    IEnumerator Init()
    {
        _gameId = "";
        while (_wsModule == null)
        {
            yield return null;
        }
        yield return _wsModule.SetupAsync(Server,
            null,
            OnMessage,
            (o, e) => Debug.Log($"WebSocket Close Code: {e.Code} Reason: {e.Reason}"),
            null);
       
        _statemachine.Next(State.Connect);
    }
    IEnumerator Close()
    {
        yield return _wsModule.CloseAsync();
        _statemachine.Next(State.End);
    }
    void OnMessage(object sender, MessageEventArgs e)
    {
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(e.Data);
        switch (obj.type)
        {
            case "connect": ResConnect(e.Data); break;
            case "ready": ResReady(e.Data); break;
            case "start": ResStart(); break;
            case "input": break; 
        }
        OnRecieveMessage.Invoke(e.Data);
    }
    void ResConnect(string msg)
    {
        MsgRoot<MsgConnect> obj = JsonUtility.FromJson<MsgRoot<MsgConnect>>(msg);
        _gameId = obj.data.id;
        _statemachine.Next(State.Entry);
        ReqEntry(Name, RoomId);
        Debug.Log("[game] id :" + _gameId);
    }

    void ResReady(string msg)
    {

    }

    void ResStart()
    {

    }
    void ResInput(string msg){
        MsgRoot<MsgInput> obj = JsonUtility.FromJson<MsgRoot<MsgInput>>(msg);
        //OnRecieveInput(obj.data)
    }
    //request
    public void ReqEntry(string name,string room)
    {
        if (_statemachine.GetCurrentState() != State.Entry)
        {
            return;
        }
        MsgRoot<SendConnectGame> obj =
            new MsgRoot<SendConnectGame>()
            {
                type = "connect",
                data = new SendConnectGame()
                {
                    name = name,
                    room = room
                }
            };
        _wsModule.Send(JsonUtility.ToJson(obj));
        _statemachine.Next(State.Wait);
        Debug.Log("[game] entry name : " + name);
    }

    public void ReqInput(SendInput input)
    {
        MsgRoot<SendInput> obj =
            new MsgRoot<SendInput>()
            {
                type = "input",
                data = input
            };
        _wsModule.Send(JsonUtility.ToJson(obj));
    }
}
