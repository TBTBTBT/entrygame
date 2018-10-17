using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class WsMatchingModule
{
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

    private string _matchingId = "";
    public string Server { get; set; } = "";

    public string GameServerAddress { get; private set; } = "";
    public string RoomId { get; private set; } = "";
    private WebSocketModule _wsModule;



    private Statemachine<State> _statemachine;
    public State Current => _statemachine.GetCurrentState();
    public bool IsClose => _statemachine.GetCurrentState() == State.End;
    public bool IsCancel { get; set; } = false;
    public void SetWsModule(WebSocketModule ws) => _wsModule = ws;
    public WsMatchingModule()
    {
        InitState();
    }
    public void InitState()
    {
        _statemachine = new Statemachine<State>();
        _statemachine.Init(this);
        _statemachine.Next(State.Init);
    }

    public void Update()
    {
        _statemachine.Update();
    }

    IEnumerator Init()
    {
        while (_wsModule == null)
        {
            yield return null;
        }
        IsCancel = false;
        _matchingId = "";
        GameServerAddress = "";
        RoomId = "";
        yield return _wsModule.SetupAsync(Server,
            null,
            OnMessage,
            (o,e)=>Debug.Log($"[ws] WebSocket Close Code: {e.Code} Reason: {e.Reason}"),
            null);
        
        _statemachine.Next(State.Connect);
    }
    IEnumerator Connect()
    {
        yield return null;
    }

    IEnumerator Close()
    {
        yield return _wsModule.CloseAsync();
        _statemachine.Next(State.End);
    }
    void OnMessage(object sender,MessageEventArgs e)
    {
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(e.Data);
        switch (obj.type)
        {
            case "connect":
                ResConnect(e.Data);
                break;
            case "match":
                ResMatching(e.Data);
                break;
        }
    }

    void ResConnect(string msg)
    {
        MsgRoot<MsgConnect> obj = JsonUtility.FromJson<MsgRoot<MsgConnect>>(msg);
        _matchingId = obj.data.id;
        _statemachine.Next(State.Entry);
        Debug.Log("[matching] id :" + _matchingId);
    }

    void ResMatching(string msg)
    {
        MsgRoot<MsgMatching> obj = JsonUtility.FromJson<MsgRoot<MsgMatching>>(msg);
        GameServerAddress = obj.data.address;
        RoomId = obj.data.room;
        Debug.Log("[matching] room :" + obj.data.room + ", address : " + obj.data.address);
        _statemachine.Next(State.Close);
    }
    //request
    public void ReqEntry(string name)
    {
        if (_statemachine.GetCurrentState() != State.Entry)
        {
            return;
        }
        MsgRoot<SendEntry> obj =
            new MsgRoot<SendEntry>()
            {
                type = "connect",
                data = new SendEntry()
                {
                    name = name
                }
            };
        _wsModule.Send(JsonUtility.ToJson(obj));
        _statemachine.Next(State.Wait);
        Debug.Log("[matching] entry name : " +name);
    }

}
